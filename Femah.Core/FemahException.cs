using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
