using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeignRestClient
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class RequestMappingAttribute : Attribute
    {
        public HttpMethod Method;
        public string path;

        public enum HttpMethod
        {
            GET,
            POST,
            PUT,
            DELETE,
            PATCH
        }
    }
}
