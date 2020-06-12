using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeignRestClient
{
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false)]
    public class FeignClientAttribute : Attribute
    {
        public string Url;

        public FeignClientAttribute(string url)
        {
            Url = url;
        }
    }
}
