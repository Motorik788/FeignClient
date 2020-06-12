using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;


namespace FeignRestClient
{
    internal class ClientHandler
    {
        private class HttpMethodParameters
        {
            public Dictionary<string, object> RequestParams { get; private set; }
            public Dictionary<string, object> RequestFormData { get; private set; }
            public Dictionary<string, string> RequestHeaders { get; private set; }

            public MethodInfo Method { get; private set; }

            public ParameterInfo RequestBody { get; private set; }
            public RequestMappingAttribute RequestMappingAttribute { get; private set; }

            public HttpMethodParameters(object[] args)
            {
                StackTrace s = new StackTrace();
                const int TARGET_METHOD_STACK_POS = 2;
                Method = s.GetFrame(TARGET_METHOD_STACK_POS).GetMethod() as MethodInfo;
                var interfaceMethod = Method.DeclaringType.GetTypeInfo().ImplementedInterfaces.ElementAt(0).GetMethod(Method.Name);
                var parameters = interfaceMethod.GetParameters();
                RequestBody = parameters.FirstOrDefault(x => x.GetCustomAttribute(typeof(RequestBodyAttribute)) != null);

                RequestMappingAttribute = interfaceMethod.GetCustomAttribute(typeof(RequestMappingAttribute)) as RequestMappingAttribute;

                RequestParams = new Dictionary<string, object>();
                RequestFormData = new Dictionary<string, object>();
                RequestHeaders = new Dictionary<string, string>();

                for (int i = 0; i < parameters.Length; i++)
                {
                    var pathVarAttr = parameters[i].GetCustomAttribute(typeof(PathVariableAttribute)) as PathVariableAttribute;
                    var requestParamAttr = parameters[i].GetCustomAttribute(typeof(RequestParamAttribute)) as RequestParamAttribute;
                    var requestBodyAttr = parameters[i].GetCustomAttribute(typeof(RequestBodyAttribute)) as RequestBodyAttribute;
                    var requestHeader = parameters[i].GetCustomAttribute(typeof(RequestHeaderAttribute)) as RequestHeaderAttribute;

                    if (requestHeader != null)
                    {
                        if (!RequestHeaders.ContainsKey(requestHeader.Name))
                            RequestHeaders.Add(requestHeader.Name, args[i].ToString());
                        else
                            RequestHeaders[requestHeader.Name] = args[i].ToString();
                        continue;
                    }

                    if (pathVarAttr == null && requestBodyAttr == null && !IsCustomClassType(parameters[i].ParameterType))
                        RequestParams.Add(parameters[i].Name, args[i]);
                    else if (pathVarAttr != null)
                    {
                        string namePathVar = !string.IsNullOrEmpty(pathVarAttr.Name) ? pathVarAttr.Name : parameters[i].Name;
                        RequestMappingAttribute.path = RequestMappingAttribute.path.Replace("{" + namePathVar + "}", args[i].ToString());
                    }
                    else if (requestBodyAttr == null)
                        ParsePublicMembers(args[i], RequestFormData);
                }
            }
            private bool IsCustomClassType(Type type)
            {
                return type.IsClass && type != typeof(string);
            }

            private void ParsePublicMembers(object classObj, Dictionary<string, object> requestFormData)
            {
                var type = classObj.GetType();
                var publField = type.GetFields();
                for (int i = 0; i < publField.Length; i++)
                {
                    requestFormData.Add(publField[i].Name, publField[i].GetValue(classObj));
                }
            }
        }


        protected ClientHandler() { }
        protected string baseUrl;
        protected RestClient client;

        private void fillHeaders(Dictionary<string, string> requestHeaders)
        {
            foreach (var header in requestHeaders)
            {
                client.AddHeader(header.Key, header.Value);
            }
        }

        protected object Handle(object[] args)
        {
            if (client == null)
                client = new RestClient(baseUrl);
            var parameters = new HttpMethodParameters(args);
            var returnType = parameters.Method.ReturnType;

            fillHeaders(parameters.RequestHeaders);

            if (parameters.RequestMappingAttribute != null)
            {
                var method = parameters.RequestMappingAttribute.Method;
                var path = parameters.RequestMappingAttribute.path;
                var requestBody = parameters.RequestBody;
                var requestBodyValue = requestBody != null ? args[requestBody.Position] : null;
                var requestParams = parameters.RequestParams;
                var requestFormData = parameters.RequestFormData;
                switch (method)
                {
                    case RequestMappingAttribute.HttpMethod.GET:
                        return SendRequest(returnType, () => client.Get(path, returnType, requestParams));
                    case RequestMappingAttribute.HttpMethod.POST:
                        if (requestBody == null && requestFormData.Count == 0)
                            throw new Exception("Для POST запроса необходим параметр с аттрибутом RequestBody");

                        return SendRequest(returnType,
                            () => client.Post(path, requestBodyValue,
                            returnType, requestParams, requestFormData));
                    case RequestMappingAttribute.HttpMethod.PUT:
                        if (requestBody == null && requestFormData.Count == 0)
                            throw new Exception("Для PUT запроса необходим параметр с аттрибутом RequestBody");

                        return SendRequest(returnType,
                            () => client.Put(path, requestBodyValue,
                            returnType, requestParams, requestFormData));
                    case RequestMappingAttribute.HttpMethod.DELETE:
                        return SendRequest(returnType, () => client.Delete(path, returnType, requestParams));
                    default:
                        Console.WriteLine(method + " not supported");
                        break;
                }
            }
            return null;
        }

        private object SendRequest(Type returnType, Func<object> func)
        {
            if (returnType == typeof(void))
            {
                func();
                return null;
            }
            return func();
        }
    }
}
