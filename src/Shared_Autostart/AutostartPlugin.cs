using BepInEx;
using BepInEx.Logging;

namespace Autostart
{
    [BepInPlugin(GUID, PluginName, Version)]
    public partial class AutostartPlugin : BaseUnityPlugin
    {
        public const string GUID = "Autostart";
        public const string Version = "1.0.0";
        
        public const string PluginName = "Autostart";

        protected const string SECTION_GENERAL = "General";
        protected const string DESCRIPTION_AUTOSTART = "Choose which mode to start automatically when launching the game.\n" +
                                                       "Hold esc or F1 during startup to cancel automatic behaviour or hold another shortcut to use that instead.\n" +
                                                       "You can create game shortcuts that automatically load a specific mode after startup, check readme for details.";

        internal static new ManualLogSource Logger;
        internal static AutostartPlugin Instance;

        private AutostartPlugin()
        {
            Logger = base.Logger;
            Instance = this;
        }
    }
}
