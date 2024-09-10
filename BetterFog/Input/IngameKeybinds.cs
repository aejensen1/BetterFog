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

        internal void InitializeKeybindings(string nextHotkeyString, string refreshHotkeyString)
        {
            // Set the hotkey dynamically
            NextPresetHotkey = new InputAction("Next Fog Preset", binding: $"<Keyboard>/{nextHotkeyString}");
            RefreshPresetHotKey = new InputAction("Refresh Fog Preset", binding: $"<Keyboard>/{refreshHotkeyString}");

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

        }

        public static void DisableHotkeys()
        {
            BetterFog.hotkeysEnabled = false;
            Instance.NextPresetHotkey.Disable();
            Instance.RefreshPresetHotKey.Disable();
        }

        public static void EnableHotkeys()
        {
            BetterFog.hotkeysEnabled = true;
            Instance.NextPresetHotkey.Enable();
            Instance.RefreshPresetHotKey.Enable();
        }
    }
}
