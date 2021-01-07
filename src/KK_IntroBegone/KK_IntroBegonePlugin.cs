using System.Collections;
using BepInEx;
using HarmonyLib;
using Scene = Manager.Scene;

namespace IntroBegone
{
    [BepInProcess("Koikatu")]
    [BepInProcess("Koikatsu Party")]
    [BepInDependency(KKAPI.KoikatuAPI.GUID, KKAPI.KoikatuAPI.VersionConst)]
    public partial class IntroBegonePlugin : BaseUnityPlugin
    {
        private void Awake()
        {
            var skip = Config.Bind("Game startup", "Skip showing intro screen", true,
                "Speed up game startup by skippping the logo shown at the start, before title screen.");

            if (skip.Value)
                Harmony.CreateAndPatchAll(typeof(HooksSkip), GUID);
        }

        private static class HooksSkip
        {
            [HarmonyPrefix]
            [HarmonyPatch(typeof(LogoScene), "Start")]
            private static bool DisableLogo(ref IEnumerator __result)
            {
                IEnumerator LoadTitle()
                {
                    Logger.LogInfo("Skipping intro");
                    Singleton<Scene>.Instance.LoadReserve(new Scene.Data
                    {
                        levelName = "Title",
                        isFade = false
                    }, false);
                    yield break;
                }

                __result = LoadTitle();
                return false;
            }
        }
    }
}