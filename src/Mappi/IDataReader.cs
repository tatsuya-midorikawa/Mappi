using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#if NET45 || NET46 || NET472 || NET48 || NETCOREAPP3_1 || NET5_0
using System.Threading.Tasks;
#endif

namespace Mappi
{
    internal interface IDataReader
    {
        IEnumerable<T> Read<T>() where T : new();

#if NET45 || NET46 || NET472 || NET48 || NETCOREAPP3_1 || NET5_0
        Task<IEnumerable<T>> ReadAsync<T>(int baseCapacity = 128) 
            where T : class;
#endif
    }
}
