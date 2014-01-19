using System;

namespace Femah.Core
{
    public class FemahException : Exception
    {
        public FemahException(string message)
            : base(message)
        {
        }

        public FemahException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
