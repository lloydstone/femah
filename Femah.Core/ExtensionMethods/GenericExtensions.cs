using System.IO;

namespace Femah.Core.ExtensionMethods
{
    static public class GenericExtensions
    {
        public static void CopyTo(this Stream source, Stream destination)
        {
            // TODO: Argument validation
            var buffer = new byte[16384]; // For example...
            int bytesRead;
            while ((bytesRead = source.Read(buffer, 0, buffer.Length)) > 0)
            {
                destination.Write(buffer, 0, bytesRead);
            }
        }

    }
}
