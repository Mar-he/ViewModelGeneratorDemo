using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneratorsTest
{
    public class Field<T> 
        where T : class
    {
        public T? Value { get; set; }
        public int Fa { get; set; }

        public static Field<T> From(T source, int fieldAccess) {
            return new Field<T> {
                Value = source,
                Fa= fieldAccess,
            };
        }
    }
}
