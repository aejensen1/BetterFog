using BetterFog.Assets;
using LethalCompanyInputUtils.Api;
using UnityEngine.InputSystem;

namespace BetterFog.Input
{
    internal class IngameKeybinds : LcInputActions
    {
        internal static IngameKeybinds Instance = new IngameKeybinds();
        internal static InputActionAsset GetAsset() => Instance.Asset;

        public InputAction NextPresetHotkey { get; set; }
        public InputAction RefreshPresetHotKey { get; set; }
        public InputAction WeatherScalePresetHotKey { get; set; }

        internal void InitializeKeybindings(string nextHotkeyString, string refreshHotkeyString, string weatherScaleHotkeyString)
        {
            // Set the hotkey dynamically
            NextPresetHotkey = new InputAction("Next Fog Preset", binding: $"<Keyboard>/{nextHotkeyString}");
            RefreshPresetHotKey = new InputAction("Refresh Fog Preset", binding: $"<Keyboard>/{refreshHotkeyString}");
            WeatherScalePresetHotKey = new InputAction("Weather Scale Preset", binding: $"<Keyboard>/{weatherScaleHotkeyString}");

            // Optionally add a gamepad binding
            NextPresetHotkey.AddBinding("<Gamepad>/leftStickPress");
            RefreshPresetHotKey.AddBinding("<Gamepad>/rightStickPress");

            if (BetterFog.nextHotKeyEnabled.Value)
            {
                // Enable the action
                NextPresetHotkey.Enable();

                // Subscribe to the performed event
                NextPresetHotkey.performed += ctx => BetterFog.NextPreset();
            }
            else
            {
                // Disable the action
                NextPresetHotkey.Disable();
            }

            if (BetterFog.refreshHotKeyEnabled.Value)
            {
                // Enable the action
                RefreshPresetHotKey.Enable();

                // Subscribe to the performed event
                RefreshPresetHotKey.performed += ctx => BetterFog.ApplyFogSettings();
            }
            else
            {
                // Disable the action
                RefreshPresetHotKey.Disable();
            }

            if (BetterFog.weatherScaleHotKeyEnabled.Value)
            {
                // Enable the action
                WeatherScalePresetHotKey.Enable();
                //BetterFog.mls.LogInfo("Weather scaling hotkey enabled.");

                // Subscribe to the performed event
                WeatherScalePresetHotKey.performed += ctx =>
                {
                    BetterFog.isDensityScaleEnabled = !BetterFog.isDensityScaleEnabled;
                    //BetterFog.mls.LogInfo($"Density scaling is now {(BetterFog.isDensityScaleEnabled ? "enabled" : "disabled")}.");
                    BetterFog.ApplyFogSettings();
                    if (FogSettingsManager.Instance.IsSettingsEnabled())
                    {
                        FogSettingsManager.Instance.UpdateSettings();
                    }
                };
            }
            else
            {
                // Disable the action
                WeatherScalePresetHotKey.Disable();
            }
        }

        public static void DisableHotkeys()
        {
            BetterFog.hotkeysEnabled = false;
            Instance.NextPresetHotkey.Disable();
            Instance.RefreshPresetHotKey.Disable();
            Instance.WeatherScalePresetHotKey.Disable();
        }

        public static void EnableHotkeys()
        {
            BetterFog.hotkeysEnabled = true;
            Instance.NextPresetHotkey.Enable();
            Instance.RefreshPresetHotKey.Enable();
            Instance.WeatherScalePresetHotKey.Enable();
        }
    }
}
