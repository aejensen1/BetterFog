﻿using UnityEngine;
using UnityEngine.UI;
using TMPro;  // Make sure you have this namespace for TextMeshPro
using System.Collections.Generic;
using System.Collections;
using UnityEngine.EventSystems;
using BepInEx;
using System.IO;

namespace BetterFog.Assets
{
    public class FogSettingsManager : MonoBehaviour
    {
        private AssetBundle fogsettingsgui;
        private GameObject settingsCanvas;
        private TMP_FontAsset customFont;  // Store the custom font asset
        public TMP_Dropdown presetDropdown; // Dropdown for fog preset
        public TMP_Dropdown modeDropdown; // Dropown for fog mode
        private bool isSettingsEnabled = false;

        private Slider fogDensitySlider; // Slider for fog density
        private TextMeshProUGUI densityVal; // Display for fog density value
        private Button densityUp;
        private Button densityDown;

        private Slider fogRedSlider; // Slider for fog density
        private TextMeshProUGUI redVal; // Display for fog density value
        private Button redUp;
        private Button redDown;

        private Slider fogGreenSlider; // Slider for fog density
        private TextMeshProUGUI greenVal; // Display for fog density value
        private Button greenUp;
        private Button greenDown;

        private Slider fogBlueSlider; // Slider for fog density
        private TextMeshProUGUI blueVal; // Display for fog density value
        private Button blueUp;
        private Button blueDown;

        //public Toggle noFogCheckbox;
        public Toggle weatherScaleCheckbox;

        private static FogSettingsManager instance;
        private static bool isInitializing = false;

        private int previousModeIndex;

        public static FogSettingsManager Instance
        {
            get
            {
                if (instance == null && !isInitializing)
                {
                    isInitializing = true;
                    if (BetterFog.verboseLoggingEnabled.Value)
                        BetterFog.mls.LogInfo("Instance is null, creating new instance.");
                    var gameObject = new GameObject("FogSettingsManager");
                    DontDestroyOnLoad(gameObject);
                    instance = gameObject.AddComponent<FogSettingsManager>();
                    instance.Initialize();
                    isInitializing = false;
                }
                return instance;
            }
        }

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                if (BetterFog.verboseLoggingEnabled.Value)
                    BetterFog.mls.LogInfo("FogSettingsManager created and started.");
            }
            else if (instance != this)
            {
                Destroy(gameObject);
                if (BetterFog.verboseLoggingEnabled.Value)
                    BetterFog.mls.LogWarning("FogSettingsManager already exists. Duplicate destroyed.");
            }
        }

        //--------------------------------- Start Initialization ---------------------------------

        private void Initialize()
        {
            if (BetterFog.guiEnabled.Value == false)
            {
                BetterFog.mls.LogWarning("FogSettingsManager GUI is disabled by config file.");
                return;
            }

            BetterFog.mls.LogInfo("Initializing FogSettingsManager.");

            string[] assetPaths = Directory.GetFiles(Paths.PluginPath, "fogsettingsgui", SearchOption.AllDirectories);

            if (assetPaths.Length > 0)
            {
                string bundlePath = assetPaths[0];
                fogsettingsgui = AssetBundle.LoadFromFile(bundlePath);

                if (fogsettingsgui != null)
                {
                    BetterFog.mls.LogInfo("AssetBundle loaded successfully.");
                    LoadAssetsFromBundle();
                }
                else
                {
                    BetterFog.mls.LogError("Failed to load AssetBundle.");
                }
            }
            else
            {
                BetterFog.mls.LogError("fogsettingsgui file not found in any subdirectory of BepInEx/plugins.");
            }

            previousModeIndex = BetterFog.currentModeIndex;
        }

        private void LoadAssetsFromBundle()
        {
            if (fogsettingsgui != null)
            {
                BetterFog.mls.LogInfo("AssetBundle loaded successfully.");
                customFont = fogsettingsgui.LoadAsset<TMP_FontAsset>("3270Condensed-Regular SDF");  // Load the custom font
                BetterFog.mls.LogInfo("If you see an error indicating 'shader compiler platform 4 is not available', nothing is broken.");

                if (customFont != null)
                {
                    //BetterFog.mls.LogInfo(customFont.ToString() + " custom font loaded successfully.");
                }
                else
                {
                    BetterFog.mls.LogError("Custom font asset not found in AssetBundle.");
                }

                // Apply the Distance Field shader to the custom font
                Shader textShader = Shader.Find("TextMeshPro/Distance Field");
                if (textShader != null)
                {
                    customFont.material.shader = textShader;
                    //BetterFog.mls.LogInfo(customFont.material.shader.ToString() + " shader applied to custom font successfully.");
                }
                else
                {
                    BetterFog.mls.LogError("TextMeshPro/Distance Field shader not found!");
                }

                GameObject canvasPrefab = fogsettingsgui.LoadAsset<GameObject>("FogSettingsCanvas");

                if (canvasPrefab != null)
                {
                    settingsCanvas = Instantiate(canvasPrefab);
                    settingsCanvas.SetActive(false);
                    BetterFog.mls.LogInfo("FogSettingsCanvas instantiated and hidden.");

                    // Apply the custom font to TextMeshPro components
                    ApplyCustomFont(settingsCanvas);

                    // Find dropdowns and populate
                    presetDropdown = settingsCanvas.transform.Find("PresetDropdown").GetComponent<TMP_Dropdown>();
                    PopulateDropdown(presetDropdown);
                    if (BetterFog.verboseLoggingEnabled.Value)
                        BetterFog.mls.LogInfo("Fog preset dropdown is now populated.");
                    SetCurrentOption(presetDropdown);

                    modeDropdown = settingsCanvas.transform.Find("ModeDropdown").GetComponent<TMP_Dropdown>();
                    //BetterFog.mls.LogInfo(modeDropdown.ToString() + "Found");
                    PopulateDropdown(modeDropdown);
                    if (BetterFog.verboseLoggingEnabled.Value)
                        BetterFog.mls.LogInfo("Fog mode dropdown is now populated.");
                    SetCurrentOption(modeDropdown);
                    //BetterFog.mls.LogInfo("SetCurrentOption complete.");

                    // Find the slider and text components
                    fogDensitySlider = settingsCanvas.transform.Find("ThicknessSlider").GetComponent<Slider>();
                    densityVal = settingsCanvas.transform.Find("ThicknessNum").GetComponent<TextMeshProUGUI>();
                    densityDown = settingsCanvas.transform.Find("ThicknessDown").GetComponent<Button>();
                    densityUp = settingsCanvas.transform.Find("ThicknessUp").GetComponent<Button>();

                    fogRedSlider = settingsCanvas.transform.Find("RedSlider").GetComponent<Slider>();
                    redVal = settingsCanvas.transform.Find("RedHueNum").GetComponent<TextMeshProUGUI>();
                    redDown = settingsCanvas.transform.Find("RedDown").GetComponent<Button>();
                    redUp = settingsCanvas.transform.Find("RedUp").GetComponent<Button>();

                    fogGreenSlider = settingsCanvas.transform.Find("GreenSlider").GetComponent<Slider>();
                    greenVal = settingsCanvas.transform.Find("GreenHueNum").GetComponent<TextMeshProUGUI>();
                    greenDown = settingsCanvas.transform.Find("GreenDown").GetComponent<Button>();
                    greenUp = settingsCanvas.transform.Find("GreenUp").GetComponent<Button>();

                    fogBlueSlider = settingsCanvas.transform.Find("BlueSlider").GetComponent<Slider>();
                    blueVal = settingsCanvas.transform.Find("BlueHueNum").GetComponent<TextMeshProUGUI>();
                    blueDown = settingsCanvas.transform.Find("BlueDown").GetComponent<Button>();
                    blueUp = settingsCanvas.transform.Find("BlueUp").GetComponent<Button>();

                    weatherScaleCheckbox = settingsCanvas.transform.Find("WeatherScaleToggle").GetComponent<Toggle>();
                    weatherScaleCheckbox.isOn = BetterFog.isDensityScaleEnabled;

                    if (fogDensitySlider != null && densityVal != null)
                    {
                        // Initialize the text with the current slider value
                        densityVal.text = fogDensitySlider.value.ToString();
                        redVal.text = fogRedSlider.value.ToString();
                        greenVal.text = fogGreenSlider.value.ToString();
                        blueVal.text = fogBlueSlider.value.ToString();

                        // Add a listener to update the text and apply the value when the slider changes
                        fogDensitySlider.onValueChanged.AddListener(value => OnSliderValueChanged(fogDensitySlider, value));
                        fogRedSlider.onValueChanged.AddListener(value => OnSliderValueChanged(fogRedSlider, value));
                        fogGreenSlider.onValueChanged.AddListener(value => OnSliderValueChanged(fogGreenSlider, value));
                        fogBlueSlider.onValueChanged.AddListener(value => OnSliderValueChanged(fogBlueSlider, value));

                        InitializeButtonListeners();

                        // Add a listener to update the Weather Scale value when the checkbox is toggled
                        weatherScaleCheckbox.onValueChanged.AddListener(isChecked => OnDensityScaleCheckboxValueChanged(isChecked));
                    }
                }
                else
                {
                    BetterFog.mls.LogError("FogSettingsCanvas prefab not found in AssetBundle.");
                }
            }
            else
            {
                BetterFog.mls.LogError("Failed to load AssetBundle.");
            }
        }             

        //--------------------------------- End Initialization ---------------------------------
        //--------------------------------- Start Custom Font ---------------------------------

        private void ApplyCustomFont(GameObject canvas)
        {
            // Apply custom font to all TextMeshProUGUI components
            TextMeshProUGUI[] textComponents = canvas.GetComponentsInChildren<TextMeshProUGUI>();
            foreach (var textComponent in textComponents)
            {
                textComponent.font = customFont;  // Assign the custom font
            }

            // Apply custom font to dropdown item labels
            TMP_Dropdown[] dropdowns = canvas.GetComponentsInChildren<TMP_Dropdown>();
            foreach (var dropdown in dropdowns)
            {
                // Apply the custom font to the dropdown label
                if (dropdown.captionText != null)
                {
                    dropdown.captionText.font = customFont;
                }

                // Access the dropdown item template
                Transform itemTemplate = dropdown.template?.Find("Viewport/Content/Item");
                if (itemTemplate != null)
                {
                    // Apply the custom font to each item in the dropdown
                    TextMeshProUGUI[] itemTexts = itemTemplate.GetComponentsInChildren<TextMeshProUGUI>();
                    foreach (var itemText in itemTexts)
                    {
                        itemText.font = customFont;
                    }
                }
            }
        }

        //--------------------------------- End Custom Font ---------------------------------
        //--------------------------------- Start Slider Adjustment ---------------------------------

        private void OnSliderValueChanged(Slider slider, float value)
        {
            if (BetterFog.verboseLoggingEnabled.Value)
                BetterFog.mls.LogInfo($"Slider value changed: {slider.name} = {value}");
            if (slider == fogDensitySlider && densityVal != null)
            {
                densityVal.text = value.ToString("0");
                BetterFog.currentPreset.MeanFreePath = value;
            }
            else if (slider == fogRedSlider && redVal != null)
            {
                redVal.text = value.ToString("0.00");
                BetterFog.currentPreset.AlbedoR = value;
            }
            else if (slider == fogGreenSlider && greenVal != null)
            {
                greenVal.text = value.ToString("0.00");
                BetterFog.currentPreset.AlbedoG = value;
            }
            else if (slider == fogBlueSlider && blueVal != null)
            {
                blueVal.text = value.ToString("0.00");
                BetterFog.currentPreset.AlbedoB = value;
            }
            BetterFog.ApplyFogSettings();
        }

        private void UpdateSlidersWithCurrentPreset()
        {
            // Ensure sliders and preset are valid
            if (fogDensitySlider != null && densityVal != null &&
                fogRedSlider != null && redVal != null &&
                fogGreenSlider != null && greenVal != null &&
                fogBlueSlider != null && blueVal != null &&
                BetterFog.currentPreset != null)
            {
                // Example update logic: assuming currentPreset has properties for these values
                fogDensitySlider.value = BetterFog.currentPreset.MeanFreePath;
                redVal.text = fogDensitySlider.value.ToString();

                fogRedSlider.value = BetterFog.currentPreset.AlbedoR;
                redVal.text = fogRedSlider.value.ToString();

                fogGreenSlider.value = BetterFog.currentPreset.AlbedoG;
                greenVal.text = fogGreenSlider.value.ToString();

                fogBlueSlider.value = BetterFog.currentPreset.AlbedoB;
                blueVal.text = fogBlueSlider.value.ToString();

                // Log updates for debugging
                //BetterFog.mls.LogInfo($"Updated sliders to current preset: {BetterFog.currentPreset.PresetName}");
            }
            else
            {
                //BetterFog.mls.LogError("Cannot update sliders: One or more components are missing or currentPreset is null.");
            }
        }

        //--------------------------------- End Slider Adjustment ---------------------------------
        //--------------------------------- Start Button Adjustment ---------------------------------

        private void InitializeButtonListeners()
        {
            AddAdjustmentListener(densityDown, -1f, fogDensitySlider);
            AddAdjustmentListener(densityUp, 1f, fogDensitySlider);
            AddAdjustmentListener(redDown, -0.1f, fogRedSlider);
            AddAdjustmentListener(redUp, 0.1f, fogRedSlider);
            AddAdjustmentListener(greenDown, -0.1f, fogGreenSlider);
            AddAdjustmentListener(greenUp, 0.1f, fogGreenSlider);
            AddAdjustmentListener(blueDown, -0.1f, fogBlueSlider);
            AddAdjustmentListener(blueUp, 0.1f, fogBlueSlider);
        }

        private void AddAdjustmentListener(Button button, float adjustmentStep, Slider slider)
        {
            EventTrigger trigger = button.gameObject.GetComponent<EventTrigger>();
            if (trigger == null)
                trigger = button.gameObject.AddComponent<EventTrigger>();

            EventTrigger.Entry entryDown = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerDown
            };
            entryDown.callback.AddListener((eventData) => StartAdjusting(slider, adjustmentStep));
            trigger.triggers.Add(entryDown);

            EventTrigger.Entry entryUp = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerUp
            };
            entryUp.callback.AddListener((eventData) => StopAdjusting());
            trigger.triggers.Add(entryUp);

            EventTrigger.Entry exitEntry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerExit
            };
            exitEntry.callback.AddListener((eventData) => StopAdjusting());
            trigger.triggers.Add(exitEntry);
        }
        private Coroutine adjustCoroutine;

        private void StartAdjusting(Slider slider, float changeAmount)
        {
            if (adjustCoroutine != null)
                StopCoroutine(adjustCoroutine);

            adjustCoroutine = StartCoroutine(AdjustSliderValueWithDebounce(slider, changeAmount));
        }

        private IEnumerator AdjustSliderValueWithDebounce(Slider slider, float changeAmount)
        {
            float lastUpdateTime = Time.time;

            while (true)
            {
                if (Time.time - lastUpdateTime >= 0.05f) // Debounce interval
                {
                    slider.value = Mathf.Clamp(slider.value + changeAmount, slider.minValue, slider.maxValue);
                    lastUpdateTime = Time.time;
                }

                yield return null;
            }
        }

        private void StopAdjusting()
        {
            if (adjustCoroutine != null)
                StopCoroutine(adjustCoroutine);
        }


        //--------------------------------- End Button Adjustment ---------------------------------
        //--------------------------------- Start Checkbox Adjustment ---------------------------------

        private void OnDensityScaleCheckboxValueChanged(bool isChecked)
        {
            if (BetterFog.verboseLoggingEnabled.Value)
                BetterFog.mls.LogInfo($"Density Scale Checkbox value changed: {isChecked}");
            BetterFog.isDensityScaleEnabled = isChecked;
            BetterFog.ApplyFogSettings();
        }

        private void UpdateDensityScaleCheckbox()
        {
            if (weatherScaleCheckbox != null)
            {
                weatherScaleCheckbox.isOn = BetterFog.isDensityScaleEnabled;
            }
        }

        //--------------------------------- End Checkbox Adjustment ---------------------------------
        //--------------------------------- Start Dropdown Adjustment ---------------------------------

        private void PopulateDropdown(TMP_Dropdown dropdown)
        {
            try
            {
                // Clear existing options
                dropdown.ClearOptions();
                //BetterFog.mls.LogInfo("Cleared dropdown options");

                if (dropdown == presetDropdown)
                    dropdown.AddOptions(GetFogPresets());
                else if (dropdown == modeDropdown)
                    dropdown.AddOptions(GetFogModes());
            }
            catch
            {
                BetterFog.mls.LogError("Dropdown does not exist. Cannot populate");
            }
        }

        private List<string> GetFogPresets()
        {
            // Create a list of preset names
            List<string> options = new List<string>();

            foreach (FogConfigPreset preset in BetterFog.fogConfigPresets)
            {
                options.Add(preset.PresetName); // Assuming each preset has a 'name' property
                //BetterFog.mls.LogInfo($"{preset.PresetName} Added to dropdown options");
            }

            return options;
        }

        private List<string> GetFogModes()
        {
            // Create a list of preset names
            List<string> options = new List<string>();

            foreach (BetterFogMode mode in BetterFog.fogModes)
            {
                options.Add(mode.Name); // Assuming each preset has a 'name' property
                //BetterFog.mls.LogInfo($"{mode.Name} Added to dropdown options");
            }

            return options;
        }

        private void SetCurrentOption(TMP_Dropdown dropdown)
        {
            // Log the current preset details
            //BetterFog.mls.LogInfo($"Setting dropdown value to preset index: {BetterFog.currentPresetIndex}, name: {BetterFog.currentPreset.PresetName}");

            // Set the dropdown to the current preset index
            if (dropdown != null)
            {
                //presetDropdown.value = BetterFog.currentPresetIndex;
                UpdateDropdownWithCurrentOption(dropdown);

                // Remove any previous listeners to avoid duplicate calls
                dropdown.onValueChanged.RemoveAllListeners();

                // Add a listener to handle changes to the dropdown selection
                if (dropdown == presetDropdown )
                    dropdown.onValueChanged.AddListener(OnPresetChanged);
                else if (dropdown == modeDropdown )
                    dropdown.onValueChanged.AddListener(delegate
                    {
                        // Call OnModeChanged with the current and previous index
                        OnModeChanged(dropdown.value, previousModeIndex);

                        // Update the previousModeIndex after the change
                        previousModeIndex = dropdown.value;
                    });
            }
            else
            {
                //BetterFog.mls.LogError("PresetDropdown is not assigned.");
            }
        }

        public void UpdateSettings()
        {
            UpdateDropdownWithCurrentOption(presetDropdown);
            UpdateDropdownWithCurrentOption(modeDropdown);
            UpdateSlidersWithCurrentPreset();
            UpdateDensityScaleCheckbox();
        }

        private void UpdateDropdownWithCurrentOption(TMP_Dropdown dropdown)
        {
            if (dropdown != null)
            {
                if (dropdown == presetDropdown)
                {
                    dropdown.value = BetterFog.currentPresetIndex;
                    //BetterFog.mls.LogInfo($"Preset dropdown updated to: {BetterFog.currentPreset.PresetName}");
                    if (BetterFog.verboseLoggingEnabled.Value)
                        BetterFog.mls.LogInfo($"Preset dropdown updated to: {BetterFog.fogConfigPresets[dropdown.value].PresetName}");
                }
                else if (dropdown == modeDropdown)
                {
                    dropdown.value = BetterFog.currentModeIndex;
                    //BetterFog.mls.LogInfo($"Mode dropdown updated to: {BetterFog.currentMode.Name}");
                    if (BetterFog.verboseLoggingEnabled.Value)
                        BetterFog.mls.LogInfo($"Mode dropdown updated to: {BetterFog.fogModes[dropdown.value].Name}");
                }
            }
            else
            {
                if (BetterFog.verboseLoggingEnabled.Value)
                    BetterFog.mls.LogError("Dropdown is null. Cannot update");
            }
        }

        private void OnPresetChanged(int index)
        {
            // Update the currentPreset based on the selected dropdown option
            BetterFog.currentPresetIndex = index;
            BetterFog.currentPreset = BetterFog.fogConfigPresets[index];
            UpdateSlidersWithCurrentPreset();

            // Apply the preset or perform other actions based on the selection
            ApplyPreset(BetterFog.currentPreset);
        }

        private void OnModeChanged(int currentIndex, int previousIndex)
        {
            // Update the current mode in BetterFog
            BetterFog.currentModeIndex = currentIndex;
            BetterFog.currentMode = BetterFog.fogModes[currentIndex];
            BetterFog.Instance.UpdateMode();

            // Apply the new fog settings
            BetterFog.ApplyFogSettings();
        }

        private void ApplyPreset(FogConfigPreset preset) //Could possibly remove?
        {
            BetterFog.currentPresetIndex = BetterFog.fogConfigPresets.IndexOf(preset);
            BetterFog.currentPreset = preset;
            //BetterFog.mls.LogInfo($"Applying preset: {preset.PresetName}");
            BetterFog.ApplyFogSettings();
        }

        //--------------------------------- End Dropdown Adjustment ---------------------------------
        //--------------------------------- Start Settings Enable/Disable ---------------------------------

        public void EnableSettings()
        {
            if (BetterFog.guiEnabled.Value == false)
            {
                BetterFog.mls.LogWarning("FogSettingsManager GUI is disabled by config file.");
                return;
            }

            isSettingsEnabled = true;

            if (settingsCanvas == null) // If the canvas is null, reinitialize
            {
                // Destroy the existing settingsCanvas
                Destroy(settingsCanvas);
                settingsCanvas = null; // Clear the reference

                if (BetterFog.verboseLoggingEnabled.Value)
                    BetterFog.mls.LogWarning("Canvas prefab not found. Initializing new Canvas.");
                UnloadAssetBundle();
                Initialize(); // Method to handle the initialization of settingsCanvas
            }

            // Assume the canvas is ready
            settingsCanvas.SetActive(true);
            SetCurrentOption(presetDropdown);
            SetCurrentOption(modeDropdown);
            BetterFog.mls.LogInfo("Fog Settings opened.");
        }

        private void UnloadAssetBundle()
        {
            if (fogsettingsgui != null)
            {
                fogsettingsgui.Unload(true); // Unload all assets and the AssetBundle itself
                fogsettingsgui = null; // Clear the reference
                BetterFog.mls.LogInfo("AssetBundle unloaded.");
            }
        }

        public void DisableSettings()
        {
            if (BetterFog.guiEnabled.Value == false)
            {
                if (BetterFog.verboseLoggingEnabled.Value)
                    BetterFog.mls.LogWarning("FogSettingsManager GUI is disabled by config file.");
                return;
            }

            isSettingsEnabled = false;
            if (settingsCanvas != null)
            {
                settingsCanvas.SetActive(false);
                BetterFog.mls.LogInfo("Fog Settings closed.");
            }
            else
            {
                // Destroy the existing settingsCanvas
                Destroy(settingsCanvas);
                settingsCanvas = null; // Clear the reference

                if (BetterFog.verboseLoggingEnabled.Value)
                    BetterFog.mls.LogWarning("Canvas prefab not found. Initializing new Canvas.");
                UnloadAssetBundle();
                Initialize(); // Method to handle the initialization of settingsCanvas
            }
        }

        private void OnDestroy()
        {
            if (fogsettingsgui != null)
            {
                fogsettingsgui.Unload(false);
            }
            instance = null; // Ensure instance is nullified when destroyed
        }

        public bool IsSettingsEnabled()
        {
            return isSettingsEnabled;
        }
    }
}
