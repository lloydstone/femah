using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace Femah.Core.ExtensionMethods
{
    static internal class StringExtensions
    {

        public static string ToJson<T> (this T obj)
        {
            var stream = new MemoryStream();

            try
            {
                Type concreteType = obj.GetType();
                var serializer = new DataContractJsonSerializer(concreteType);
                serializer.WriteObject(stream, obj);

                return Encoding.UTF8.GetString(stream.ToArray());
            }
            catch (System.Runtime.Serialization.InvalidDataContractException exception)
            {
                //Add logging
                throw;
            }
            catch (System.Runtime.Serialization.SerializationException exception)
            {
                //Add logging
                throw;
            }
            catch (Exception exception)
            {
                throw;
            }
            finally
            {
                stream.Close();
                stream.Dispose();
            }
        }
    }
}