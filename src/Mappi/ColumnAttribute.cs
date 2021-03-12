using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mappi
{
    public class ColumnAttribute : Attribute
    {
        public string Name { get; }
        public object DefaultValue { get; }
        public ColumnAttribute(string Name, object DefaultValue = null)
        {
            this.Name = Name;
            this.DefaultValue = DefaultValue;
        }
    }
}
