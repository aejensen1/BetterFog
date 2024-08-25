using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;  // Make sure you have this namespace for TextMeshPro
using System.Collections.Generic;
using System.Collections;

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

        private Slider fogRedSlider; // Slider for fog density
        private TextMeshProUGUI redVal; // Display for fog density value

        private Slider fogGreenSlider; // Slider for fog density
        private TextMeshProUGUI greenVal; // Display for fog density value

        private Slider fogBlueSlider; // Slider for fog density
        private TextMeshProUGUI blueVal; // Display for fog density value

        private Slider fogAlphaSlider; // Slider for fog density
        private TextMeshProUGUI alphaVal; // Display for fog density value

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

        private void Initialize()
        {
            BetterFog.mls.LogInfo("Initializing FogSettingsManager.");
            string bundlePath = "BepInEx/plugins/Assets/FogAssetBundle/fogsettingsgui";
            if (System.IO.File.Exists(bundlePath))
            {
                fogsettingsgui = AssetBundle.LoadFromFile(bundlePath);

                if (fogsettingsgui != null)
                {
                    BetterFog.mls.LogInfo("AssetBundle loaded successfully.");
                    customFont = fogsettingsgui.LoadAsset<TMP_FontAsset>("3270Condensed-Regular SDF");  // Load the custom font

                    if (customFont != null)
                    {
                        BetterFog.mls.LogInfo(customFont.ToString() + " custom font loaded successfully.");
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
                        BetterFog.mls.LogInfo(customFont.material.shader.ToString() + " shader applied to custom font successfully.");
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
                        //BetterFog.mls.LogInfo("ThicknessSlider and ThicknessNum are the names of the GameObjects in the prefab");

                        fogRedSlider = settingsCanvas.transform.Find("RedSlider").GetComponent<Slider>();
                        redVal = settingsCanvas.transform.Find("RedHueNum").GetComponent<TextMeshProUGUI>();
                        //BetterFog.mls.LogInfo("RedSlider and RedHueNum are the names of the GameObjects in the prefab");

                        fogGreenSlider = settingsCanvas.transform.Find("GreenSlider").GetComponent<Slider>();
                        greenVal = settingsCanvas.transform.Find("GreenHueNum").GetComponent<TextMeshProUGUI>();
                        //BetterFog.mls.LogInfo("GreenSlider and GreenHueNum are the names of the GameObjects in the prefab");

                        fogBlueSlider = settingsCanvas.transform.Find("BlueSlider").GetComponent<Slider>();
                        blueVal = settingsCanvas.transform.Find("BlueHueNum").GetComponent<TextMeshProUGUI>();
                        //BetterFog.mls.LogInfo("BlueSlider and BlueHueNum are the names of the GameObjects in the prefab");

                        fogAlphaSlider = settingsCanvas.transform.Find("AlphaSlider").GetComponent<Slider>();
                        alphaVal = settingsCanvas.transform.Find("AlphaNum").GetComponent<TextMeshProUGUI>();
                        //BetterFog.mls.LogInfo("AlphaSlider and AlphaNum are the names of the GameObjects in the prefab");

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
            else
            {
                BetterFog.mls.LogError("AssetBundle file not found at path: " + bundlePath);
            }
        }

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

        private void OnCheckboxValueChanged(bool isChecked)
        {
            BetterFog.currentPreset.NoFog = isChecked;
            BetterFog.ApplyFogSettings();
        }

        private void PopulateDropdown()
        {
            // Clear existing options
            presetDropdown.ClearOptions();
            BetterFog.mls.LogInfo("Cleared dropdown options");

            // Create a list of preset names
            List<string> options = new List<string>();
            foreach (FogConfigPreset preset in BetterFog.FogConfigPresets)
            {
                options.Add(preset.PresetName); // Assuming each preset has a 'name' property
                BetterFog.mls.LogInfo($"{preset.PresetName} Added to dropdown options");
            }

            // Add the options to the dropdown
            presetDropdown.AddOptions(options);
        }

        private void SetCurrentPreset()
        {
            // Log the current preset details
            BetterFog.mls.LogInfo($"Setting dropdown value to preset index: {BetterFog.currentPresetIndex}, name: {BetterFog.currentPreset.PresetName}");

            // Set the dropdown to the current preset index
            if (presetDropdown != null)
            {
                presetDropdown.value = BetterFog.currentPresetIndex;

                // Remove any previous listeners to avoid duplicate calls
                presetDropdown.onValueChanged.RemoveAllListeners();

                // Add a listener to handle changes to the dropdown selection
                presetDropdown.onValueChanged.AddListener(OnPresetChanged);
                BetterFog.mls.LogInfo("Listener added to preset dropdown.");
            }
            else
            {
                BetterFog.mls.LogError("PresetDropdown is not assigned.");
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
                BetterFog.mls.LogInfo($"Updated sliders to current preset: {BetterFog.currentPreset.PresetName}");
            }
            else
            {
                BetterFog.mls.LogError("Cannot update sliders: One or more components are missing or currentPreset is null.");
            }
        }
        private void UpdateNoFogCheckbox()
        {
            if (noFogCheckbox != null)
            {
                BetterFog.mls.LogInfo("Current preset: " + BetterFog.currentPreset.PresetName + " NoFog: " + BetterFog.currentPreset.NoFog);
                noFogCheckbox.isOn = BetterFog.currentPreset.NoFog;
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
                BetterFog.mls.LogError("Canvas prefab not found.");
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
                BetterFog.mls.LogError("Canvas prefab not found.");
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
