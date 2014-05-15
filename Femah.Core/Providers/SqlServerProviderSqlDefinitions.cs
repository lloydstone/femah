namespace Femah.Core.Providers
{
    public static class SqlServerProviderSqlDefinitions
    {
        public static string DeleteSwitch = "DELETE FROM {0} WHERE name = @SwitchName";
        public static string InsertSwitch = "INSERT INTO {0} (name, isEnabled, assemblyName, typeName, switchXml ) VALUES (@SwitchName, @IsEnabled, @AssemblyName, @TypeName, @SwitchXml)";
        public static string UpdateSwitch = "UPDATE {0} SET isEnabled = @IsEnabled, assemblyName = @AssemblyName, typeName = @TypeName, switchXml = @SwitchXml WHERE name = @SwitchName";
        public static string SwitchCount = "SELECT COUNT(*) FROM {0} WHERE name = @Param1";
        public static string SelectSwitch = "SELECT id, name, isEnabled, assemblyName, typeName, switchXml FROM {0} WHERE name = @NameParam";
        public static string SelectAllSwitches = "SELECT id, name, isEnabled, assemblyName, typeName, switchXml FROM {0}";

        public static string CreateSelectSwitchSql(string tableName)
        {
            return string.Format(SelectSwitch, tableName); 
        }

        public static string CreateSwitchCountSql(string tableName)
        {
            return string.Format(SwitchCount, tableName); 
        }

        public static string CreateInsertSwitchSql(string tableName)
        {
            return string.Format(InsertSwitch, tableName); 
        }

        public static string CreateSelectAllSwitchesSql(string tableName)
        {
            return string.Format(SelectAllSwitches, tableName);
        }

        public static string CreateDeleteSwitchSql(string tableName)
        {
            return string.Format(DeleteSwitch, tableName);
        }

        public static string CreateUpdateSwitchSql(string tableName)
        {
            return string.Format(UpdateSwitch, tableName);
        }
    }
}