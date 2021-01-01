using BepInEx;
using BepInEx.Logging;

namespace IntroBegone
{
    [BepInPlugin(GUID, PluginName, Version)]
    public partial class IntroBegonePlugin : BaseUnityPlugin
    {
        public const string GUID = "IntroBegone";
        public const string Version = "1.0.0";
        
        protected const string SECTION_GENERAL = "General";
        protected const string DESCRIPTION_AUTOSTART = "Choose which mode to start automatically when launching the game.\n" +
                                                       "Hold esc or F1 during startup to cancel automatic behaviour or hold another shortcut to use that instead.";

        internal static new ManualLogSource Logger;
        internal static IntroBegonePlugin Instance;

        private IntroBegonePlugin()
        {
            Logger = base.Logger;
            Instance = this;
        }
    }
}
