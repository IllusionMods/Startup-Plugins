using System;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using KKAPI.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace IntroBegone
{
    [BepInProcess("PlayHome32bit")]
    [BepInProcess("PlayHome64bit")]
    [BepInDependency(KKAPI.KoikatuAPI.GUID, KKAPI.KoikatuAPI.VersionConst)]
    public partial class IntroBegonePlugin : BaseUnityPlugin
    {
        public const string PluginName = "Maker button and autostart to maker";

        private static ConfigEntry<bool> _addMakerButton;

        private void Awake()
        {
            _addMakerButton = Config.Bind("Main menu", "Add Character Maker button", true,
                "Adds a 'Character Maker' button to the main menu, so you can open it directly wihtout having to start a new game.");

            Harmony.CreateAndPatchAll(typeof(Hooks), GUID);
        }

        private static class Hooks
        {
            private static bool _roadwayToMaker;
            private static bool _usedMakerButton;

            [HarmonyPostfix]
            [HarmonyPatch(typeof(CautionScene), "Start")]
            private static void CautionSceneOverridePatch(CautionScene __instance)
            {
                _roadwayToMaker = Environment.GetCommandLineArgs().Any(x => x == "-maker");
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
            [HarmonyPatch(typeof(TitleScene), "Start")]
            private static void TitleSceneAddMakerButtonPatch(TitleScene __instance, ref Button[] ___buttons)
            {
                if (!_roadwayToMaker && _addMakerButton.Value)
                {
                    // Create a new Maker button
                    var startButton = ___buttons[1];
                    var newButtonObj =
                        Instantiate(startButton.gameObject, startButton.transform.parent, true);
                    var newButtonCmp = newButtonObj.GetComponent<Button>();
                    newButtonCmp.onClick.ActuallyRemoveAllListeners();
                    newButtonCmp.onClick.AddListener(() =>
                    {
                        _usedMakerButton = true;
                        __instance.GC.audioCtrl.Play2DSE(__instance.GC.audioCtrl.systemSE_yes);
                        StartMaker(1f, __instance);
                    });
                    newButtonCmp.GetComponentInChildren<Text>().text = "Character Maker";

                    // Realign the buttons
                    var btnList = ___buttons.ToList();
                    btnList.Insert(2, newButtonCmp);
                    var startPos = btnList[0].transform.localPosition;
                    for (int i = 0; i < btnList.Count; i++)
                        btnList[i].transform.localPosition = startPos + new Vector3(0, -75, 0) * i;

                    // Add new button to button array so it is activated with others
                    ___buttons = btnList.ToArray();
                }
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(TitleScene), "ShowButtons")]
            private static bool TitleSceneOverridePatch(TitleScene __instance, ref System.Collections.IEnumerator __result)
            {
                if (_roadwayToMaker)
                {
                    __result = CoroutineUtils.CreateCoroutine(() => StartMaker(0f, __instance));
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
                else if (_usedMakerButton)
                {
                    ___toReturn.onClick.ActuallyRemoveAllListeners();
                    var txt = ___toReturn.GetComponentInChildren<Text>();
                    txt.text = "Back to title";
                    ___toReturn.onClick.AddListener(() => __instance.GC.CreateModalYesNoUI(
                        "Are you sure you want to go back to the title screen?\nUnsaved changes will be lost.",
                        () => { __instance.GC.ChangeScene("TitleScene", "", 1); }));
                }
            }
        }
    }
}
