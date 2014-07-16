using System.Collections.Generic;
using Femah.Core.Tests.SqlProviderFakes;

namespace Femah.Core.Tests
{
    internal class SwitchComparer : IEqualityComparer<Switch>
    {
        public bool Equals(Switch x, Switch y)
        {
            return (x.Name == y.Name) && (x.AssemblyName == y.AssemblyName);
        }

        public int GetHashCode(Switch obj)
        {
            return obj.GetHashCode();
        }
    }
}