using BepInEx;
using BepInEx.Logging;

namespace AddMakerButton
{
    [BepInPlugin(GUID, PluginName, Version)]
    public partial class MakerButtonPlugin : BaseUnityPlugin
    {
        public const string GUID = "AddMakerButton";
        public const string Version = "1.0.0";
        
        public const string PluginName = "Add character maker button to main menu";

        internal static new ManualLogSource Logger;
        internal static MakerButtonPlugin Instance;

        private MakerButtonPlugin()
        {
            Logger = base.Logger;
            Instance = this;
        }
    }
}
