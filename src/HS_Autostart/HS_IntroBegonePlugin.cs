using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using UnityEngine;

namespace Autostart
{
    [BepInProcess("HoneySelect_32")]
    [BepInProcess("HoneySelect_64")]
    public partial class AutostartPlugin : BaseUnityPlugin
    {
        private static readonly Dictionary<string, Action<TitleScene>> _supportedArgs =
            new Dictionary<string, Action<TitleScene>>
            {
                {"-maker", scene => scene.OnCustomFemale()},
                {"-femalemaker", scene => scene.OnCustomFemale()},
                {"-malemaker", scene => scene.OnCustomMale()},
            };

        private ConfigEntry<AutoStartOption> AutoStart { get; set; }

        private bool _checkInput;
        private bool _cancelAuto;
        private TitleScene _titleScene;
        private Action<TitleScene> _autostartCommand = null;

        private void Awake()
        {
            // Check if any autostart parameters were passed
            var _ = Environment.GetCommandLineArgs().FirstOrDefault(x => _supportedArgs.TryGetValue(x.ToLowerInvariant(), out _autostartCommand));

            AutoStart = Config.Bind(SECTION_GENERAL, "Automatic start mode", AutoStartOption.Disabled, new ConfigDescription(DESCRIPTION_AUTOSTART));
        }

        private void OnLevelWasLoaded(int level)
        {
            StopAllCoroutines();

            _titleScene = FindObjectOfType<TitleScene>();

            if (_titleScene)
            {
                if (!_checkInput)
                {
                    _checkInput = true;
                    StartCoroutine(InputCheck());
                }
            }
            else
            {
                _checkInput = false;
            }
        }

        private IEnumerator InputCheck()
        {
            while (_checkInput)
            {
                if (!_cancelAuto && AutoStart.Value != AutoStartOption.Disabled && (Input.GetKey(KeyCode.Escape) || Input.GetKey(KeyCode.F1)))
                {
                    base.Logger.Log(LogLevel.Message, "Automatic start cancelled");
                    _cancelAuto = true;
                }

                if (!Manager.Scene.Instance.IsNowLoadingFade)
                {
                    if (_autostartCommand != null)
                    {
                        _autostartCommand(_titleScene);
                        _autostartCommand = null;
                        _checkInput = false;
                    }
                    else if (!_cancelAuto && AutoStart.Value != AutoStartOption.Disabled)
                    {
                        switch (AutoStart.Value)
                        {
                            case AutoStartOption.FemaleMaker:
                                StartMode(_titleScene.OnCustomFemale, "Automatically starting female maker");
                                break;

                            case AutoStartOption.MaleMaker:
                                StartMode(_titleScene.OnCustomMale, "Automatically starting male maker");
                                break;
                        }
                    }

                    _cancelAuto = true;
                }

                yield return null;

                void StartMode(Action action, string msg)
                {
                    if (!FindObjectOfType<ConfigScene>())
                    {
                        base.Logger.LogMessage(msg);
                        _checkInput = false;
                        action();
                    }
                }
            }
        }

        private enum AutoStartOption
        {
            Disabled,
            [Description("Female maker")]
            FemaleMaker,
            [Description("Male maker")]
            MaleMaker
        }
    }
}