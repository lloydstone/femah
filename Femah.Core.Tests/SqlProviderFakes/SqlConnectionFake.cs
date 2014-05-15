using Femah.Core.Providers;
using System;
using System.Collections.Generic;

namespace Femah.Core.Tests.SqlProviderFakes
{
    class SqlConnectionFake : ISqlConnection
    {
        private readonly string _tableName;
        public List<string> Features { get; set; }
        public Action OpenAction { get; set; }

        private readonly Action<List<string>, string> _deleteAction = (x,y) => x.Remove(y);
        private readonly Action<List<string>, string> _insertAction = (x,y) => x.Add(y);

        public SqlConnectionFake(string tableName)
        {
            _tableName = tableName;
            Features = new List<string>();
        }

        public void Open()
        {
            if (OpenAction != null)
            {
                OpenAction();
            }
        }

        public ISqlCommand CreateCommand(string command)
        {
            if (command == SqlServerProviderSqlDefinitions.CreateInsertSwitchSql(_tableName))
            {
                return new ExecuteNonQueryCommandFake(this, _insertAction);
            }
            
            if (command == SqlServerProviderSqlDefinitions.CreateDeleteSwitchSql(_tableName))
            {
                return new ExecuteNonQueryCommandFake(this, _deleteAction);
            }

            if (command == SqlServerProviderSqlDefinitions.CreateSelectAllSwitchesSql(_tableName))
            {
                return new SelectAllSwitchesCommandFake(this);
            }

            if (command == SqlServerProviderSqlDefinitions.CreateSwitchCountSql(_tableName))
            {
                return new SwitchCountCommandFake(this);
            }

            if (command == SqlServerProviderSqlDefinitions.CreateSelectSwitchSql(_tableName))
            {
                return new SelectSwitchCommandFake(this);
            }

            throw new NotSupportedException(string.Format("Command {0} is not supported", command));
        }

        public void Dispose() { } // Unused
    }
}
