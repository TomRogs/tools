using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mappers
{
    using System;
    using System.Data;
    using System.Reflection;
    using System.Linq;
    using AdoLibrary;

    namespace Mappers
    {
        public static class MapperExtension
        {
            public static TTo Map<TTo>(this object from)
                where TTo : new()
            {
                // créer une instance vide du nouvel objet
                TTo result = new();
                return from.MapToInstance(result);
            }

            public static TTo MapToInstance<TTo>(this object from, TTo result)
                where TTo : new()
            {
                if (from is not null)
                {
                    // récupérer toutes les propriétes de cet objet 
                    PropertyInfo[] toProperties = typeof(TTo).GetProperties();
                    foreach (PropertyInfo toProperty in toProperties)
                    {
                        PropertyInfo fromProp = from.GetType().GetProperty(toProperty.Name);
                        if (fromProp != null)
                        {
                            // récupérer la valeur ds l'objet de départ
                            object value = fromProp.GetValue(from);
                            try
                            {
                                // insérer cette valeur dans le nouvel objet
                                toProperty.SetValue(result, value);
                            }
                            catch (Exception) { }
                        }
                    }

                    // recuperer une propriété dans l'objet de départ qui porte le meme nom
                }

                return result;
            }
            public static TTo MapToInstance<TTo>(this IDataRecord from, TTo result)
              where TTo : new()
            {
                // récupérer toutes les propriétes de cet objet 
                PropertyInfo[] toProperties = typeof(TTo).GetProperties();
                foreach (PropertyInfo toProperty in toProperties)
                {
                    // recuperer une propriété dans l'objet de départ qui porte le meme nom
                    PropertyInfo fromProp = from.GetType().GetProperty(toProperty.Name);
                    if (fromProp != null)
                    {
                        // récupérer la valeur ds l'objet de départ
                        object value = fromProp.GetValue(from);
                        try
                        {
                            // insérer cette valeur dans le nouvel objet
                            toProperty.SetValue(result, value);
                        }
                        catch (Exception) { }
                    }
                }
                return result;
            }
            public static TTo Map<TTo>(this IDataRecord from)
              where TTo : new()
            {
                // créer une instance vide du nouvel objet
                TTo result = new();
                return from.MapToInstance(result);
            }
            public static TTo MapReader<TTo>(this IDataRecord reader)
                where TTo : new()
            {
                TTo result = new TTo();
                var Properties = typeof(TTo).GetProperties().Where(prop => prop.GetCustomAttribute<ReadIgnoreAttribute>() == null);
                foreach (PropertyInfo item in Properties)
                {


                    object value = reader[item.Name];
                    item.SetValue(result, value == DBNull.Value ? null : value);


                }
                return result;
            }

            public static void MapToCommand(this Command cmd, object from)
            {
                var properties = from.GetType().GetProperties().Where(prop => prop.GetCustomAttribute<MapIgnoreAttribute>() == null);
                foreach (var item in properties)
                {
                    cmd.AddParameter("@" + item.Name.ToLowerCamelCase(), item.GetValue(from) ?? DBNull.Value);
                }
            }

            private static string ToLowerCamelCase(this string value)
            {
                return value[0].ToString().ToLower() + value.Substring(1, value.Length - 1);
            }
        }

    }


}
