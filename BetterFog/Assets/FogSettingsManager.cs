using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;  // Make sure you have this namespace for TextMeshPro
using System.Collections.Generic;
using System.Collections;
using UnityEngine.EventSystems;
using BepInEx;
using System.IO;
using System.Runtime.CompilerServices;

namespace BetterFog.Assets
{
    public class FogSettingsManager : MonoBehaviour
    {
        private AssetBundle fogsettingsgui;
        private GameObject settingsCanvas;
        private TMP_FontAsset customFont;  // Store the custom font asset
        public TMP_Dropdown presetDropdown; // Reference to the dropdown in the UI
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

        private Slider fogAlphaSlider; // Slider for fog density
        private TextMeshProUGUI alphaVal; // Display for fog density value
        private Button alphaUp;
        private Button alphaDown;

        public Toggle noFogCheckbox;

        private static FogSettingsManager instance;
        private static bool isInitializing = false;

        public static FogSettingsManager Instance
        {
            get
            {
                if (instance == null && !isInitializing)
                {
                    isInitializing = true;
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
                BetterFog.mls.LogInfo("FogSettingsManager created and started.");
            }
            else if (instance != this)
            {
                Destroy(gameObject);
                BetterFog.mls.LogWarning("FogSettingsManager already exists. Duplicate destroyed.");
            }
        }

        //--------------------------------- Start Initialization ---------------------------------

        private void Initialize()
        {
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

                    presetDropdown = settingsCanvas.transform.Find("PresetDropdown").GetComponent<TMP_Dropdown>();
                    PopulateDropdown();
                    BetterFog.mls.LogInfo("Options are now populated.");
                    SetCurrentPreset();

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

                    fogAlphaSlider = settingsCanvas.transform.Find("AlphaSlider").GetComponent<Slider>();
                    alphaVal = settingsCanvas.transform.Find("AlphaNum").GetComponent<TextMeshProUGUI>();
                    alphaDown = settingsCanvas.transform.Find("AlphaDown").GetComponent<Button>();
                    alphaUp = settingsCanvas.transform.Find("AlphaUp").GetComponent<Button>();

                    noFogCheckbox = settingsCanvas.transform.Find("NoFogToggle").GetComponent<Toggle>();

                    if (fogDensitySlider != null && densityVal != null && noFogCheckbox != null)
                    {
                        // Initialize the text with the current slider value
                        densityVal.text = fogDensitySlider.value.ToString();
                        redVal.text = fogRedSlider.value.ToString();
                        greenVal.text = fogGreenSlider.value.ToString();
                        blueVal.text = fogBlueSlider.value.ToString();
                        alphaVal.text = fogAlphaSlider.value.ToString();

                        // Initialize the checkbox based on the current Anisotropy value
                        noFogCheckbox.isOn = BetterFog.currentPreset.NoFog != false;

                        // Add a listener to update the text and apply the value when the slider changes
                        fogDensitySlider.onValueChanged.AddListener(value => OnSliderValueChanged(fogDensitySlider, value));
                        fogRedSlider.onValueChanged.AddListener(value => OnSliderValueChanged(fogRedSlider, value));
                        fogGreenSlider.onValueChanged.AddListener(value => OnSliderValueChanged(fogGreenSlider, value));
                        fogBlueSlider.onValueChanged.AddListener(value => OnSliderValueChanged(fogBlueSlider, value));
                        fogAlphaSlider.onValueChanged.AddListener(value => OnSliderValueChanged(fogAlphaSlider, value));

                        InitializeButtonListeners();

                        // Add a listener to update the Anisotropy value when the checkbox is toggled
                        noFogCheckbox.onValueChanged.AddListener(isChecked => OnCheckboxValueChanged(isChecked));
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
            if (slider == fogDensitySlider && densityVal != null)
            {
                densityVal.text = value.ToString("0");
                BetterFog.currentPreset.MeanFreePath = value;
            }
            else if (slider == fogRedSlider && redVal != null)
            {
                redVal.text = value.ToString("0");
                BetterFog.currentPreset.AlbedoR = value;
            }
            else if (slider == fogGreenSlider && greenVal != null)
            {
                greenVal.text = value.ToString("0");
                BetterFog.currentPreset.AlbedoG = value;
            }
            else if (slider == fogBlueSlider && blueVal != null)
            {
                blueVal.text = value.ToString("0");
                BetterFog.currentPreset.AlbedoB = value;
            }
            else if (slider == fogAlphaSlider && alphaVal != null)
            {
                alphaVal.text = value.ToString("0.00");
                BetterFog.currentPreset.AlbedoA = value;
            }
            //BetterFog.currentPreset.NoFog = false;
            UpdateNoFogCheckbox();
            BetterFog.ApplyFogSettings();
        }

        private void UpdateSlidersWithCurrentPreset()
        {
            // Ensure sliders and preset are valid
            if (fogDensitySlider != null && densityVal != null &&
                fogRedSlider != null && redVal != null &&
                fogGreenSlider != null && greenVal != null &&
                fogBlueSlider != null && blueVal != null &&
                fogAlphaSlider != null && alphaVal != null &&
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

                fogAlphaSlider.value = BetterFog.currentPreset.AlbedoA;
                alphaVal.text = fogAlphaSlider.value.ToString();

                // Log updates for debugging
                //BetterFog.mls.LogInfo($"Updated sliders to current preset: {BetterFog.currentPreset.PresetName}");
            }
            else
            {
                BetterFog.mls.LogError("Cannot update sliders: One or more components are missing or currentPreset is null.");
            }
        }

        //--------------------------------- End Slider Adjustment ---------------------------------
        //--------------------------------- Start Button Adjustment ---------------------------------

        private void InitializeButtonListeners()
        {
            AddAdjustmentListener(densityDown, -2f, fogDensitySlider);
            AddAdjustmentListener(densityUp, 2f, fogDensitySlider);
            AddAdjustmentListener(redDown, -1f, fogRedSlider);
            AddAdjustmentListener(redUp, 1f, fogRedSlider);
            AddAdjustmentListener(greenDown, -1f, fogGreenSlider);
            AddAdjustmentListener(greenUp, 1f, fogGreenSlider);
            AddAdjustmentListener(blueDown, -1f, fogBlueSlider);
            AddAdjustmentListener(blueUp, 1f, fogBlueSlider);
            AddAdjustmentListener(alphaDown, -0.1f, fogAlphaSlider);
            AddAdjustmentListener(alphaUp, 0.1f, fogAlphaSlider);
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

        private void OnCheckboxValueChanged(bool isChecked)
        {
            BetterFog.currentPreset.NoFog = isChecked;
            BetterFog.ApplyFogSettings();
        }

        private void UpdateNoFogCheckbox()
        {
            if (noFogCheckbox != null)
            {
                noFogCheckbox.isOn = BetterFog.currentPreset.NoFog;
            }
        }

        //--------------------------------- End Checkbox Adjustment ---------------------------------
        //--------------------------------- Start Dropdown Adjustment ---------------------------------

        private void PopulateDropdown()
        {
            // Clear existing options
            presetDropdown.ClearOptions();
            //BetterFog.mls.LogInfo("Cleared dropdown options");

            // Create a list of preset names
            List<string> options = new List<string>();
            foreach (FogConfigPreset preset in BetterFog.FogConfigPresets)
            {
                options.Add(preset.PresetName); // Assuming each preset has a 'name' property
                //BetterFog.mls.LogInfo($"{preset.PresetName} Added to dropdown options");
            }

            // Add the options to the dropdown
            presetDropdown.AddOptions(options);
        }

        private void SetCurrentPreset()
        {
            // Log the current preset details
            //BetterFog.mls.LogInfo($"Setting dropdown value to preset index: {BetterFog.currentPresetIndex}, name: {BetterFog.currentPreset.PresetName}");

            // Set the dropdown to the current preset index
            if (presetDropdown != null)
            {
                presetDropdown.value = BetterFog.currentPresetIndex;

                // Remove any previous listeners to avoid duplicate calls
                presetDropdown.onValueChanged.RemoveAllListeners();

                // Add a listener to handle changes to the dropdown selection
                presetDropdown.onValueChanged.AddListener(OnPresetChanged);
                //BetterFog.mls.LogInfo("Listener added to preset dropdown.");
            }
            else
            {
                //BetterFog.mls.LogError("PresetDropdown is not assigned.");
            }
        }

        public void UpdateSettingsWithCurrentPreset()
        {
            UpdateDropdownWithCurrentPreset();
            UpdateSlidersWithCurrentPreset();
            UpdateNoFogCheckbox();
        }

        private void UpdateDropdownWithCurrentPreset()
        {
            if (presetDropdown != null)
            {
                presetDropdown.value = BetterFog.currentPresetIndex;
                BetterFog.mls.LogInfo($"Dropdown updated to preset: {BetterFog.currentPreset.PresetName}");
            }
            else
            {
                BetterFog.mls.LogError("PresetDropdown is not assigned.");
            }
        }

        private void OnPresetChanged(int index)
        {
            // Update the currentPreset based on the selected dropdown option
            BetterFog.currentPresetIndex = index;
            BetterFog.currentPreset = BetterFog.FogConfigPresets[index];
            UpdateSlidersWithCurrentPreset();
            UpdateNoFogCheckbox();

            // Apply the preset or perform other actions based on the selection
            ApplyPreset(BetterFog.currentPreset);
        }

        private void ApplyPreset(FogConfigPreset preset)
        {
            BetterFog.currentPresetIndex = BetterFog.FogConfigPresets.IndexOf(preset);
            BetterFog.currentPreset = preset;
            BetterFog.mls.LogInfo($"Applying preset: {preset.PresetName}");
            BetterFog.ApplyFogSettings();
        }

        //--------------------------------- End Dropdown Adjustment ---------------------------------
        //--------------------------------- Start Settings Enable/Disable ---------------------------------

        public void EnableSettings()
        {
            isSettingsEnabled = true;

            if (settingsCanvas != null)
            {
                settingsCanvas.SetActive(true);
                SetCurrentPreset();
                BetterFog.mls.LogInfo("Fog Settings enabled.");
            }
            else
            {
                // Destroy the existing settingsCanvas
                Destroy(settingsCanvas);
                settingsCanvas = null; // Clear the reference

                BetterFog.mls.LogWarning("Canvas prefab not found. Initializing new Canvas.");
                UnloadAssetBundle();
                Initialize(); // Method to handle the initialization of settingsCanvas
            }
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
            isSettingsEnabled = false;
            if (settingsCanvas != null)
            {
                settingsCanvas.SetActive(false);
                BetterFog.mls.LogInfo("Fog Settings disabled.");
            }
            else
            {
                // Destroy the existing settingsCanvas
                Destroy(settingsCanvas);
                settingsCanvas = null; // Clear the reference

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
