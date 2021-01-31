using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mappi
{
    public class ColumnAttribute : Attribute
    {
        public string Name { get; }
        public ColumnAttribute(string Name)
            => this.Name = Name;
    }
}
