namespace Femah.Core.Providers
{
    public class SqlServerProviderSqlDefinitions
    {
        public static string DeleteSwitch = "DELETE FROM {0} WHERE name = @SwitchName";
        public static string InsertSwitch = "INSERT INTO {0} (name, isEnabled, assemblyName, typeName, switchXml ) VALUES (@SwitchName, @IsEnabled, @AssemblyName, @TypeName, @SwitchXml)";
        public static string UpdateSwitch = "UPDATE {0} SET isEnabled = @IsEnabled, assemblyName = @AssemblyName, typeName = @TypeName, switchXml = @SwitchXml WHERE name = @SwitchName";
        public static string SwitchCount = "SELECT COUNT(*) FROM {0} WHERE name = @Param1";
        public static string SelectSwitch = "SELECT id, name, isEnabled, assemblyName, typeName, switchXml FROM {0} WHERE name = @NameParam";
        public static string SelectAllSwitches = "SELECT id, name, isEnabled, assemblyName, typeName, switchXml FROM {0}";
    }
}