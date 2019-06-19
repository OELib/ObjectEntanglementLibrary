using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UCExample
{
    [Serializable]
    public class ExampleObject
    {
        public string Name { get; set; }
        public override string ToString()
        {
            return $"Example object {Name}";
        }
    }
}
