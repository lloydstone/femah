using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Femah.Core
{
    public interface IFemahContextFactory
    {
        IFemahContext GenerateContext();
    }
}
