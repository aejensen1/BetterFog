using LethalCompanyInputUtils.Api;
using UnityEngine.InputSystem;

namespace BetterFog.Input
{
    internal class IngameKeybinds : LcInputActions
    {
        internal static IngameKeybinds Instance = new IngameKeybinds();
        internal static InputActionAsset GetAsset() => Instance.Asset;

        

        public InputAction NextPresetHotkey { get; set; }

        internal void InitializeKeybindings(string hotkeyString)
        {
            // Set the hotkey dynamically
            NextPresetHotkey = new InputAction("Next Fog Preset", binding: $"<Keyboard>/{hotkeyString}");

            // Optionally add a gamepad binding
            NextPresetHotkey.AddBinding("<Gamepad>/leftStickPress");

            if(BetterFog.hotKeysEnabled.Value)
            {
                // Enable the action
                NextPresetHotkey.Enable();

                // Subscribe to the performed event
                NextPresetHotkey.performed += ctx => BetterFog.NextPreset();
            }
            else {
                // Disable the action
                NextPresetHotkey.Disable();
            }
        }
    }
}
