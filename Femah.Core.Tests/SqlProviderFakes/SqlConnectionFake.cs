using Femah.Core.Providers;
using System;
using System.Collections.Generic;

namespace Femah.Core.Tests.SqlProviderFakes
{
    class SqlConnectionFake : ISqlConnection
    {
        private readonly string _tableName;
        public List<Switch> Features { get; set; }
        public Action OpenAction { get; set; }

        public SqlConnectionFake(string tableName)
        {
            _tableName = tableName;
            Features = new List<Switch>();
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
                return new InsertSwitchCommandFake(this);
            }
            
            if (command == SqlServerProviderSqlDefinitions.CreateDeleteSwitchSql(_tableName))
            {
                return new DeleteSwitchCommandFake(this);
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

            if (command == SqlServerProviderSqlDefinitions.CreateUpdateSwitchSql(_tableName))
            {
                return new UpdateSwitchCommandFake(this);
            }

            throw new NotSupportedException(string.Format("Command {0} is not supported", command));
        }

        public void Dispose() { } // Unused
    }
}
