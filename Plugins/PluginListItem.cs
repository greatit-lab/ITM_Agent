// Plugins\PluginListItem.cs
using System;

namespace ITM_Agent.Plugins
{
    public class PluginListItem
    {
        public string PluginName { get; set; }
        public string AssemblyPath { get; set; }
        public override string ToString() => PluginName;
    }
}
