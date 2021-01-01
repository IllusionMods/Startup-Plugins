using BepInEx;
using BepInEx.Logging;

namespace IntroBegone
{
    [BepInPlugin(GUID, PluginName, Version)]
    public partial class IntroBegonePlugin : BaseUnityPlugin
    {
        public const string PluginName = "Intro Begone";
        public const string GUID = "IntroBegone";
        public const string Version = "1.0.0";

        internal static new ManualLogSource Logger;
        internal static IntroBegonePlugin Instance;

        private IntroBegonePlugin()
        {
            Logger = base.Logger;
            Instance = this;
        }
    }
}
