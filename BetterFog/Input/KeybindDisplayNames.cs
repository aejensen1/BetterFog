﻿using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.InputSystem;

namespace BetterFog.Input
{
    [HarmonyPatch]
    public static class KeybindDisplayNames
    {
        public static bool usingControllerPrevious = false;
        public static bool usingController { get { return StartOfRound.Instance.localPlayerUsingController; } }
        public static string[] keyboardKeywords = new string[] { "keyboard", "mouse" };
        public static string[] controllerKeywords = new string[] { "gamepad", "controller" };


        public static string GetKeybindDisplayName(InputAction inputAction)
        {
            if (inputAction == null || !inputAction.enabled)
                return "";

            int bindingIndex = usingController ? 1 : 0;
            string displayName = inputAction.bindings[bindingIndex].effectivePath;

            return GetKeybindDisplayName(displayName);
        }


        public static string GetKeybindDisplayName(string controlPath)
        {
            if (controlPath.Length <= 1)
                return "";

            string displayName = controlPath.ToLower();
            int replaceIndex = displayName.IndexOf(">/");
            displayName = replaceIndex >= 0 ? displayName.Substring(replaceIndex + 2) : displayName;

            if (displayName.Contains("not-bound"))
                return "";

            displayName = displayName.Replace("leftalt", "Alt");
            displayName = displayName.Replace("rightalt", "Alt");
            displayName = displayName.Replace("leftctrl", "Ctrl");
            displayName = displayName.Replace("rightctrl", "Ctrl");
            displayName = displayName.Replace("leftshift", "Shift");
            displayName = displayName.Replace("rightshift", "Shift");
            displayName = displayName.Replace("leftbutton", "LMB");
            displayName = displayName.Replace("rightbutton", "RMB");
            displayName = displayName.Replace("middlebutton", "MMB");
            displayName = displayName.Replace("lefttrigger", "LT");
            displayName = displayName.Replace("righttrigger", "RT");
            displayName = displayName.Replace("leftshoulder", "LB");
            displayName = displayName.Replace("rightshoulder", "RB");
            displayName = displayName.Replace("leftstickpress", "LS");
            displayName = displayName.Replace("rightstickpress", "RS");
            displayName = displayName.Replace("dpad/", "DPad-");

            displayName = displayName.Replace("backquote", "`");

            try { displayName = char.ToUpper(displayName[0]) + displayName.Substring(1); }
            catch { }

            return displayName;
        }
    }
}