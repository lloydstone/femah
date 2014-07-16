namespace Femah.Core.Tests.SqlProviderFakes
{
    using System;

    internal class Switch
    {
        public string Name { get; set; }    
        public object AssemblyName { get; set; }
        public bool IsEnabled { get; set; }

        public static Switch CreateSimpleSwitch(string name)
        {
            return new Switch
            {
                Name = name, 
                AssemblyName = DBNull.Value,
                IsEnabled = true
            };
        }
    }
}