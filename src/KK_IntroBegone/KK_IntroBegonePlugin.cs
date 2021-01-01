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

namespace IntroBegone
{
    [BepInProcess("Koikatu")]
    [BepInProcess("Koikatsu Party")]
    public partial class IntroBegonePlugin : BaseUnityPlugin
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

        private void Awake()
        {
            // Check if any autostart parameters were passed
            Action<TitleScene> autostartCommand = null;
            var _ = Environment.GetCommandLineArgs()
                .FirstOrDefault(x => _supportedArgs.TryGetValue(x.ToLowerInvariant(), out autostartCommand));

            var skip = Config.Bind("Game startup", "Skip showing intro screen", true,
                "Speed up game startup by skippping the logo shown at the start, before title screen.");

            if (autostartCommand != null)
                HooksAutostart.Apply(autostartCommand);
            else if (skip.Value)
                HooksSkip.Apply();
        }

        private static class HooksSkip
        {
            [HarmonyPrefix]
            [HarmonyPatch(typeof(LogoScene), "Start")]
            private static bool DisableLogo(ref IEnumerator __result)
            {
                IEnumerator LoadTitle()
                {
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

            public static void Apply()
            {
                Harmony.CreateAndPatchAll(typeof(HooksSkip), GUID);
            }
        }

        private static class HooksAutostart
        {
            private static Action<TitleScene> _command;

            [HarmonyPrefix]
            [HarmonyPatch(typeof(LogoScene), "Start")]
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

            [HarmonyPrefix]
            [HarmonyPatch(typeof(Scene.Data), nameof(Scene.Data.isFade), MethodType.Setter)]
            private static bool DisableFadeout()
            {
                // Speed up loading drastically
                return _command == null;
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(TitleScene), "Start")]
            private static void TitleStart(TitleScene __instance)
            {
                if (_command != null)
                {
                    __instance.StartCoroutine(TitleInput());

                    IEnumerator TitleInput()
                    {
                        yield return null;
                        _command.Invoke(__instance);
                        _command = null;

                        // Prevent button click sound from playing since we emulate button presses
                        Utils.Sound.Remove(SystemSE.sel);
                        Utils.Sound.Remove(SystemSE.ok_s);
                    }
                }
            }

            public static void Apply(Action<TitleScene> command)
            {
                _command = command;
                Harmony.CreateAndPatchAll(typeof(HooksAutostart), GUID);
            }
        }
    }
}