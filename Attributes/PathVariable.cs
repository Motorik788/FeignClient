using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeignRestClient
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class PathVariableAttribute : Attribute
    {
        public string Name;
    }
}
