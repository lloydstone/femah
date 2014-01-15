using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using Newtonsoft.Json;

namespace Femah.Core.ExtensionMethods
{
    static internal class StringExtensions
    {

        public static string ToJson<T>(this T obj)
        {
            try
            {
                //Type concreteType = obj.GetType();

                return JsonConvert.SerializeObject(obj);
            }
            catch (System.Runtime.Serialization.InvalidDataContractException)
            {
                //Add logging
                throw;
            }
            catch (System.Runtime.Serialization.SerializationException)
            {
                //Add logging
                throw;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
            }
        }
    }
}