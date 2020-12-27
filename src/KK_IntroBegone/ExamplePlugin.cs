using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using KKAPI;

namespace PluginCode
{
    [BepInPlugin(GUID, PluginName, Version)]
    [BepInDependency(KoikatuAPI.GUID, KoikatuAPI.VersionConst)]
    public class ExamplePlugin : BaseUnityPlugin
    {
        public const string PluginName = "Intro Begone";
        public const string GUID = "IntroBegone";
        public const string Version = "1.0.0";

        internal static new ManualLogSource Logger;

        private ConfigEntry<bool> _exampleConfigEntry;

        private void Awake()
        {
            Logger = base.Logger;

            _exampleConfigEntry = Config.Bind("General", "Enable this plugin", true, "If false, this plugin will do nothing");

            if (_exampleConfigEntry.Value)
            {
                Harmony.CreateAndPatchAll(typeof(Hooks), GUID);
                //CharacterApi.RegisterExtraBehaviour<MyCustomController>(GUID);
            }
        }

        private static class Hooks
        {
            // [HarmonyPrefix]
            // [HarmonyPatch(typeof(SomeClass), nameof(SomeClass.SomeInstanceMethod))]
            // private static void SomeMethodPrefix(SomeClass __instance, int someParameter, ref int __result)
            // {
            //     ...
            // }
        }
    }
}
