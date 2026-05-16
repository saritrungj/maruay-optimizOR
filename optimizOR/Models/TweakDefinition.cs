using System.Collections.Generic;
using Microsoft.Win32;

namespace optimizOR.Models
{
    public class TweakDefinition
    {
        public TweakDefinition()
        {
            AdditionalRegistryValues = new List<RegistryValueDefinition>();
        }

        public string Id { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public TweakType Type { get; set; }
        public string Target { get; set; }
        public string ValueName { get; set; }
        public object TweakValue { get; set; }
        public object DefaultValue { get; set; }
        public RegistryValueKind ValueKind { get; set; }
        public string Category { get; set; }
        public string IniSection { get; set; }
        public string PowerShellScript { get; set; }
        public List<RegistryValueDefinition> AdditionalRegistryValues { get; private set; }
    }

    public class RegistryValueDefinition
    {
        public string KeyPath { get; set; }
        public string ValueName { get; set; }
        public object TweakValue { get; set; }
        public object DefaultValue { get; set; }
        public RegistryValueKind ValueKind { get; set; }
    }
}

