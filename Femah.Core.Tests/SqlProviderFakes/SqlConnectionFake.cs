namespace Femah.Core.Tests.SqlProviderFakes
{
    using System;
    using System.Collections.Generic;
    using Providers;

    class SqlConnectionFake : ISqlConnection
    {
        public List<string> Features { get; set; }
        public Action OpenAction { get; set; }

        private readonly Action<List<string>, string> _deleteAction = (x,y) => x.Remove(y);
        private readonly Action<List<string>, string> _insertAction = (x,y) => x.Add(y);

        public void Open()
        {
            if (OpenAction != null)
            {
                OpenAction();
            }
        }

        public ISqlCommand CreateCommand(string command)
        {
            if (command == SqlServerProviderSqlDefinitions.InsertSwitch)
            {
                return new ExecuteSwitchNonQueryCommandFake(this, _insertAction);
            }
            
            if (command == SqlServerProviderSqlDefinitions.DeleteSwitch)
            {
                return new ExecuteSwitchNonQueryCommandFake(this, _deleteAction);
            }

            throw new NotImplementedException(string.Format("Command {0} is not supported", command));
        }

        public void Dispose() { } // Unused
    }
}
