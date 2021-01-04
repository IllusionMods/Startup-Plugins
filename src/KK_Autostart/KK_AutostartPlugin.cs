using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using ChaCustom;
using HarmonyLib;
using Illusion.Game;
using KKAPI.Maker;
using KKAPI.Utilities;
using UnityEngine;
using UnityEngine.UI;
using Scene = Manager.Scene;

namespace Autostart
{
    [BepInProcess("Koikatu")]
    [BepInProcess("Koikatsu Party")]
    [BepInDependency(KKAPI.KoikatuAPI.GUID, KKAPI.KoikatuAPI.VersionConst)]
    public partial class AutostartPlugin : BaseUnityPlugin
    {
        private static readonly Dictionary<string, Action<TitleScene>> _supportedArgs =
            new Dictionary<string, Action<TitleScene>>
            {
                {"-maker", scene => scene.OnCustomFemale()},
                {"-femalemaker", scene => scene.OnCustomFemale()},
                {"-malemaker", scene => scene.OnCustomMale()},
                //todo not supported since we rely on the fact the maker is started
                //{"-freeh", scene => scene.OnOtherFreeH()},
                //{"-live", scene => scene.OnOtherIdolLive()}
            };

        private static Action<TitleScene> _autostartCommand;

        private void Awake()
        {
            // Check if any autostart parameters were passed
            var _ = Environment.GetCommandLineArgs()
                .FirstOrDefault(x => _supportedArgs.TryGetValue(x.ToLowerInvariant(), out _autostartCommand));

            if (_autostartCommand != null)
                Harmony.CreateAndPatchAll(typeof(HooksAutostart), GUID);
        }

        private static class HooksAutostart
        {
            [HarmonyPrefix]
            [HarmonyPatch(typeof(LogoScene), "Start")]
            [HarmonyAfter("IntroBegone")]
            private static bool DisableLogo(LogoScene __instance, ref IEnumerator __result)
            {
                IEnumerator LoadTitle()
                {
                    // Keep the splash screen to hide title menu and loading screens flashing by during load
                    var logoC = __instance.GetComponentInChildren<Canvas>();
                    var logoCg = logoC.GetOrAddComponent<CanvasGroup>();
                    logoC.transform.SetParent(BepInEx.Bootstrap.Chainloader.ManagerObject.transform);

                    // Load title
                    Singleton<Scene>.Instance.LoadReserve(new Scene.Data
                    {
                        levelName = "Title",
                        isFade = false
                    }, false);

                    // Wait until maker finishes loading
                    yield return new WaitUntil(() => MakerAPI.InsideAndLoaded);

                    // Make the button quit completely instad of going back to title
                    var b = GameObject.FindObjectOfType<CvsExit>().GetComponentInChildren<Button>();
                    b.onClick.ActuallyRemoveAllListeners();
                    b.onClick.AddListener(() =>
                    {
                        Utils.Sound.Play(SystemSE.window_o);
                        Application.Quit();
                    });

                    // Fade out the logo
                    while (logoCg.alpha > 0)
                    {
                        logoCg.alpha -= Time.deltaTime * 4;
                        yield return null;
                    }

                    // Clean up
                    Destroy(logoC.gameObject);
                }

                Instance.StartCoroutine(LoadTitle());

                __result = null;
                return false;
            }

            // Speed up loading drastically
            [HarmonyPrefix]
            [HarmonyPatch(typeof(Scene.Data), nameof(Scene.Data.isFade), MethodType.Setter)]
            private static bool DisableFadeout()
            {
                return _autostartCommand == null;
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(TitleScene), "Start")]
            private static void TitleStart(TitleScene __instance)
            {
                if (_autostartCommand != null)
                {
                    __instance.StartCoroutine(TitleInput());

                    IEnumerator TitleInput()
                    {
                        yield return null;
                        _autostartCommand.Invoke(__instance);
                        _autostartCommand = null;

                        // Prevent button click sound from playing since we emulate button presses
                        Utils.Sound.Remove(SystemSE.sel);
                        Utils.Sound.Remove(SystemSE.ok_s);
                    }
                }
            }
        }
    }
}