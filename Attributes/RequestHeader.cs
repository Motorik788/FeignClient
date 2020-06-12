using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeignRestClient
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Parameter)]
    public class RequestHeaderAttribute : Attribute
    {
        public string Name;
        public string Value;

        public RequestHeaderAttribute(string name, string value = null)
        {
            Name = name;
            Value = value;
        }
    }
}
