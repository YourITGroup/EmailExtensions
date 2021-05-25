using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Refactored.Email.Extensions {
    internal static class ObjectExtensions
    {
        /// <summary>Turns object into dictionary</summary>
        /// <param name="o"></param>
        /// <returns></returns>
        internal static IDictionary<string, TVal> ToDictionary<TVal>(this object o)
        {
            Dictionary<string, TVal> dictionary = new Dictionary<string, TVal>();
            if (o == null)
            {
                return dictionary;
            }

            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(o);

            foreach (PropertyDescriptor propertyDescriptor in properties.Cast<PropertyDescriptor>())
            {
                object obj = propertyDescriptor.GetValue(o);
                if (obj != null)
                {
                    dictionary.Add(propertyDescriptor.Name, (TVal)obj);
                }
            }

            return dictionary;
        }
    }
}
