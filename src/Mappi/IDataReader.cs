using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mappi
{
    internal interface IDataReader
    {
        IEnumerable<T> Read<T>();
    }
}
