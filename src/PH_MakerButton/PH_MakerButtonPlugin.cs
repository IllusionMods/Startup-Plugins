using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using KKAPI.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace AddMakerButton
{
    [BepInProcess("PlayHome32bit")]
    [BepInProcess("PlayHome64bit")]
    [BepInDependency(KKAPI.KoikatuAPI.GUID, KKAPI.KoikatuAPI.VersionConst)]
    public partial class MakerButtonPlugin : BaseUnityPlugin
    {
        private static ConfigEntry<bool> _addMakerButton;

        private void Awake()
        {
            _addMakerButton = Config.Bind("Main menu", "Add Character Maker button", true,
                "Adds a 'Character Maker' button to the main menu, so you can open it directly wihtout having to start a new game.");

            if (_addMakerButton.Value)
                Harmony.CreateAndPatchAll(typeof(Hooks), GUID);
        }

        private static class Hooks
        {
            private static bool _usedMakerButton;

            private static void StartMaker(float fadeTime, TitleScene __instance)
            {
                var msg = EditScene.CreateMessage("律子", "GameStart");
                __instance.GC.ChangeScene("EditScene", msg, fadeTime);
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(TitleScene), "Start")]
            private static void TitleSceneAddMakerButtonPatch(TitleScene __instance, ref Button[] ___buttons)
            {
                if (_addMakerButton.Value)
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

            [HarmonyPostfix]
            [HarmonyPatch(typeof(EditScene), "Start")]
            private static void EditSceneBackButtonOverridePatch(EditScene __instance, Button ___toReturn)
            {
                if (_usedMakerButton)
                {
                    ___toReturn.onClick.ActuallyRemoveAllListeners();
                    var txt = ___toReturn.GetComponentInChildren<Text>();
                    txt.text = "Back to title";
                    ___toReturn.onClick.AddListener(() => __instance.GC.CreateModalYesNoUI(
                        "Are you sure you want to go back to the title screen?\nUnsaved changes will be lost.",
                        () => { __instance.GC.ChangeScene("TitleScene", "", 1); }));
                    _usedMakerButton = false;
                }
            }
        }
    }
}
