using System;
using System.Linq;
using BepInEx;
using HarmonyLib;
using KKAPI.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace Autostart
{
    [BepInProcess("PlayHome32bit")]
    [BepInProcess("PlayHome64bit")]
    [BepInDependency(KKAPI.KoikatuAPI.GUID, KKAPI.KoikatuAPI.VersionConst)]
    public partial class AutostartPlugin : BaseUnityPlugin
    {
        private static bool _roadwayToMaker;

        private void Awake()
        {
            _roadwayToMaker = Environment.GetCommandLineArgs().Select(x => x.ToLower()).Any(x => x == "-maker" || x == "-femalemaker");

            if (_roadwayToMaker)
                Harmony.CreateAndPatchAll(typeof(Hooks), GUID);
        }

        private static class Hooks
        {
            [HarmonyPostfix]
            [HarmonyPatch(typeof(CautionScene), "Start")]
            private static void CautionSceneOverridePatch(CautionScene __instance)
            {
                if (_roadwayToMaker)
                    __instance.GC.ChangeScene(__instance.nextScene, __instance.nextMessage, 0);
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(LogoScene), "Start")]
            private static void LogoSceneOverridePatch(LogoScene __instance)
            {
                if (_roadwayToMaker)
                    __instance.GC.ChangeScene(__instance.nextScene, __instance.nextMessage, 0);
            }

            private static void StartMaker(float fadeTime, TitleScene __instance)
            {
                var msg = EditScene.CreateMessage("律子", "GameStart");
                __instance.GC.ChangeScene("EditScene", msg, fadeTime);
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(TitleScene), "ShowButtons")]
            private static bool TitleSceneOverridePatch(TitleScene __instance, ref System.Collections.IEnumerator __result)
            {
                if (_roadwayToMaker)
                {
                    // Need to wait for a few seconds because DHH breaks if the game starts too fast
                    __result = CoroutineUtils.CreateCoroutine(new WaitForSeconds(2.5f), () => StartMaker(0f, __instance));
                    return false;
                }

                return true;
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(EditScene), "Start")]
            private static void EditSceneBackButtonOverridePatch(EditScene __instance, Button ___toReturn)
            {
                if (_roadwayToMaker)
                {
                    ___toReturn.onClick.ActuallyRemoveAllListeners();
                    var txt = ___toReturn.GetComponentInChildren<Text>();
                    txt.text = "Exit game";
                    ___toReturn.onClick.AddListener(() =>
                        __instance.GC.CreateModalYesNoUI(
                            "Are you sure you want to exit the game?\nUnsaved changes will be lost.",
                            Application.Quit));

                    GameObject.Find("Pause Menue Canvas(Clone)/Buttons/Button Title")?.SetActive(false);
                }
            }
        }
    }
}
