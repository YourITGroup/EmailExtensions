using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Refactored.Email
{
    internal static class ObjectExtensions
    {
        /// <summary>
        /// Turns object into dictionary
        /// </summary>
        /// <param name="o"></param>
        /// <param name="ignoreProperties">Properties to ignore</param>
        /// <returns></returns>
        internal static IDictionary<string, TVal> ToDictionary<TVal>(this object o)
        {
            if (o != null)
            {
                var props = TypeDescriptor.GetProperties(o);
                var d = new Dictionary<string, TVal>();
                foreach (var prop in props.Cast<PropertyDescriptor>())
                {
                    var val = prop.GetValue(o);
                    if (val != null)
                    {
                        d.Add(prop.Name, (TVal)val);
                    }
                }
                return d;
            }
            return new Dictionary<string, TVal>();
        }

    }
}
