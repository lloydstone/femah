using System;

namespace Femah.Core.Providers
{
    public interface ISqlConnection : IDisposable
    {
        void Open();
        ISqlCommand CreateCommand(string command);
    }
}