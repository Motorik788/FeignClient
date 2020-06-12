using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Serialization;

namespace FeignRestClient
{
    public class RestClient
    {
        private HttpClient httpClient;
        private string baseUrl;
        private DefaultContractResolver defaultContractResolver;
        private JsonSerializerSettings settings;

        private const string CONTENT_TYPE_JSON = "application/json";
        private const string CONTENT_TYPE_TEXT = "text/plain";


        public RestClient(string baseUrl)
        {
            this.baseUrl = baseUrl;
            httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "C# HttpClient");
            if (!string.IsNullOrEmpty(baseUrl))
                httpClient.BaseAddress = new Uri(baseUrl);
            // httpClient.DefaultRequestHeaders.Add("Content-Type", CONTENT_TYPE);
            defaultContractResolver = new DefaultContractResolver() { NamingStrategy = new CamelCaseNamingStrategy() };
            settings = new JsonSerializerSettings()
            {
                // ContractResolver = defaultContractResolver,
                NullValueHandling = NullValueHandling.Ignore
            };
        }


        public void AddHeader(string headerName, string headerValue)
        {
            if (!httpClient.DefaultRequestHeaders.Contains(headerName))
                httpClient.DefaultRequestHeaders.Add(headerName, headerValue);
            else
            {
                httpClient.DefaultRequestHeaders.Remove(headerName);
                httpClient.DefaultRequestHeaders.Add(headerName, headerValue);
            }
        }

        public V Post<T, V>(string relativeUrl, T requestBody, Dictionary<string, object> requestParams = null, Dictionary<string, object> requestFormData = null)
        {
            return (V)Post(relativeUrl, requestBody, requestBody.GetType(), requestParams, requestFormData);
        }


        public void Post<T>(string relativeUrl, T requestBody, Dictionary<string, object> requestParams = null, Dictionary<string, object> requestFormData = null)
        {
            Post(relativeUrl, requestBody, requestBody.GetType(), requestParams, requestFormData);
        }

        public T Get<T>(string relativeUrl, Dictionary<string, object> requestParams = null)
        {
            return (T)Get(relativeUrl, typeof(T), requestParams);
        }


        public V Put<T, V>(string relativeUrl, T requestBody, Dictionary<string, object> requestParams = null, Dictionary<string, object> requestFormData = null)
        {
            return (V)Put(relativeUrl, requestBody, requestBody.GetType(), requestParams, requestFormData);
        }

        public void Put<T>(string relativeUrl, T requestBody, Dictionary<string, object> requestParams = null, Dictionary<string, object> requestFormData = null)
        {
            Put(relativeUrl, requestBody, requestBody != null ? requestBody.GetType() : null, requestParams, requestFormData);
        }

        public T Delete<T>(string relativeUrl, Dictionary<string, object> requestParams = null)
        {
            return (T)Delete(relativeUrl, typeof(T), requestParams);
        }

        internal object Put(string relativeUrl, object requestBody, Type returnType, Dictionary<string, object> requestParams = null, Dictionary<string, object> requestFormData = null)
        {
            if (requestParams != null)
                relativeUrl = GetRequestString(relativeUrl, requestParams);
            HttpResponseMessage response = null;
            if (requestFormData == null || requestFormData.Count == 0)
                response = httpClient.PutAsync(relativeUrl, TrySerialize(requestBody)).Result;
            else
                response = httpClient.PutAsync(relativeUrl, GetFormDataContent(requestFormData)).Result;
            return TryDeserialize(response.Content.ReadAsStringAsync().Result, returnType);
        }

        internal object Delete(string relativeUrl, Type returnType, Dictionary<string, object> requestParams = null)
        {
            if (requestParams != null)
                relativeUrl = GetRequestString(relativeUrl, requestParams);
            var response = httpClient.DeleteAsync(relativeUrl).Result;
            return TryDeserialize(response.Content.ReadAsStringAsync().Result, returnType);
        }

        internal object Post(string relativeUrl, object requestBody, Type returnType, Dictionary<string, object> requestParams = null, Dictionary<string, object> requestFormData = null)
        {
            if (requestParams != null)
                relativeUrl = GetRequestString(relativeUrl, requestParams);
            HttpResponseMessage response = null;
            if (requestFormData == null || requestFormData.Count == 0)
                response = httpClient.PostAsync(relativeUrl, TrySerialize(requestBody)).Result;
            else
                response = httpClient.PostAsync(relativeUrl, GetFormDataContent(requestFormData)).Result;
            return TryDeserialize(response.Content.ReadAsStringAsync().Result, returnType);
        }

        internal object Get(string relativeUrl, Type type, Dictionary<string, object> requestParams = null)
        {
            if (requestParams != null)
                relativeUrl = GetRequestString(relativeUrl, requestParams);
            var response = httpClient.GetStringAsync(relativeUrl).Result;
            return TryDeserialize(response, type);
        }

        private MultipartFormDataContent GetFormDataContent(Dictionary<string, object> requestFormData)
        {
            var multiPart = new MultipartFormDataContent();
            foreach (var item in requestFormData)
                multiPart.Add(new StringContent(item.Value.ToString()), item.Key);
            return multiPart;
        }
        private string GetRequestString(string url, Dictionary<string, object> requestParams)
        {
            var sb = new StringBuilder(url);
            if (requestParams != null && requestParams.Count > 0)
            {
                sb.Append("?");
                int count = requestParams.Count - 1;
                int i = 0;
                foreach (var item in requestParams)
                {
                    sb.Append($"{item.Key}={item.Value.ToString()}");
                    if (i < count)
                        sb.Append("&");
                    i++;
                }
            }
            return sb.ToString();
        }

        private StringContent TrySerialize(object content)
        {
            if (content != null)
            {
                if (content.GetType() == typeof(string))
                    return new StringContent(content.ToString(), Encoding.UTF8, CONTENT_TYPE_TEXT);
                else
                {
                    try
                    {
                        return new StringContent(JsonConvert.SerializeObject(content, settings), Encoding.UTF8, CONTENT_TYPE_JSON);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Ошибка сериализации", ex);
                    }
                }

            }
            return new StringContent("");
        }

        private object TryDeserialize(string content, Type targetType)
        {
            try
            {
                if (targetType != typeof(string) && targetType != typeof(void))
                {
                    if (targetType != null)
                    {
                        return JsonConvert.DeserializeObject(content, targetType);
                    }
                    return JsonConvert.DeserializeObject(content);
                }
                return content;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка диссериализации из json", ex);
                return content;
            }
        }
    }
}
