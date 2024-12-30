using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;  // Make sure you have this namespace for TextMeshPro
using System.Collections.Generic;
using BepInEx;
using System.IO;
using Unity.Netcode;
using BetterFog.Input;
using System.Runtime.CompilerServices;
using System.Collections;
//using UnityEngine.UIElements;

namespace BetterFog.Assets
{
    public class FogSettingsManager : MonoBehaviour
    {
        private AssetBundle fogsettingsgui;
        private GameObject settingsCanvas;
        private GameObject settingsText;
        private GameObject settingsInteractables;
        private TMP_FontAsset customFont;  // Store the custom font asset

        public TMP_Dropdown modeDropdown; // Dropown for fog mode
        public TMP_Dropdown presetDropdown; // Dropdown for fog 

        private Slider fogDensitySlider; // Slider for fog density
        private TMP_InputField densityValInput; // Input field for fog density value

        private Slider fogRedSlider; // Slider for fog red hue
        TMP_InputField redValInput; // Input field for red hue value

        private Slider fogGreenSlider; // Slider for green hue
        TMP_InputField greenValInput; // Input field for green hue value

        private Slider fogBlueSlider; // Slider for blue hue
        TMP_InputField blueValInput; // Input field for blue hue value

        public TextMeshProUGUI currentWeatherVal;
        public TextMeshProUGUI currentMoonVal;

        public Toggle densityScaleCheckbox;
        public TextMeshProUGUI currentDensityVal;
        public TextMeshProUGUI densityScaleVal;
        public TextMeshProUGUI calcDensityVal;

        public TextMeshProUGUI matchVal; // Did the auto preset mode match a condition?
        public TextMeshProUGUI detectionsVal; // What conditions did the auto preset mode match?

        public Toggle excludeShipCheckbox;
        public Toggle excludeEnemiesCheckbox;
        public Toggle verboseLogsCheckbox;
        public Toggle autoPresetModeCheckbox;

        private UnityEngine.UI.Button closeButton; // Button to close the settings

        private RectTransform rectTransform;
        private bool isMouseDown = false;
        private bool isTyping = false;

        private static FogSettingsManager instance;
        private static readonly object lockObject = new object();
        private static bool isInitializing = false; // Not currently used in 3.3.6, may be removed in future versions
        public static bool supressApplyingFogSettings = false; // Used to prevent fog settings from being applied. When one GUI element is changed, the others should not apply fog settings.

        private int previousModeIndex;

        public static FogSettingsManager Instance
        {
            get
            {
                lock (lockObject)
                {
                    if (instance == null)
                    {
                        isInitializing = true;
                        if (BetterFog.verboseLoggingEnabled)
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
        }

        private void Awake()
        {
            lock (lockObject)
            {
                if (instance == null)
                {
                    instance = this;
                    DontDestroyOnLoad(gameObject);
                    if (BetterFog.verboseLoggingEnabled)
                        BetterFog.mls.LogInfo("FogSettingsManager created and started.");
                }
                else if (instance != this)
                {
                    Destroy(gameObject);
                    if (BetterFog.verboseLoggingEnabled)
                        BetterFog.mls.LogWarning("FogSettingsManager already exists. Duplicate destroyed.");
                }
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
                    BetterFog.mls.LogInfo("Custom font applied to TextMeshPro components.");

                    // Find Settings Content
                    settingsInteractables = settingsCanvas.transform.Find("Interactables").gameObject;
                    settingsText = settingsCanvas.transform.Find("Text").gameObject;

                    // Find dropdowns and populate
                    presetDropdown = settingsInteractables.transform.Find("PresetDropdown").GetComponent<TMP_Dropdown>();
                    PopulateDropdown(presetDropdown);
                    if (BetterFog.verboseLoggingEnabled)
                        BetterFog.mls.LogInfo("Fog preset dropdown is now populated.");
                    SetCurrentOption(presetDropdown);

                    modeDropdown = settingsInteractables.transform.Find("ModeDropdown").GetComponent<TMP_Dropdown>();
                    //BetterFog.mls.LogInfo(modeDropdown.ToString() + "Found");
                    PopulateDropdown(modeDropdown);
                    if (BetterFog.verboseLoggingEnabled)
                        BetterFog.mls.LogInfo("Fog mode dropdown is now populated.");
                    SetCurrentOption(modeDropdown);
                    //BetterFog.mls.LogInfo("SetCurrentOption complete.");

                    // Find the slider and input objects. Text values can be extracted from the inputs.
                    fogDensitySlider = settingsInteractables.transform.Find("DensitySlider").GetComponent<Slider>();
                    densityValInput = settingsInteractables.transform.Find("DensityInput").GetComponent<TMP_InputField>();

                    fogRedSlider = settingsInteractables.transform.Find("RedHueSlider").GetComponent<Slider>();
                    redValInput = settingsInteractables.transform.Find("RedHueInput").GetComponent<TMP_InputField>();

                    //BetterFog.mls.LogInfo("Fog red hue slider and input found.");
                    fogGreenSlider = settingsInteractables.transform.Find("GreenHueSlider").GetComponent<Slider>();
                    greenValInput = settingsInteractables.transform.Find("GreenHueInput").GetComponent<TMP_InputField>();

                    fogBlueSlider = settingsInteractables.transform.Find("BlueHueSlider").GetComponent<Slider>();
                    blueValInput = settingsInteractables.transform.Find("BlueHueInput").GetComponent<TMP_InputField>();

                    currentWeatherVal = settingsText.transform.Find("CurrentWeatherVal").GetComponent<TextMeshProUGUI>();
                    currentMoonVal = settingsText.transform.Find("CurrentMoonVal").GetComponent<TextMeshProUGUI>();

                    densityScaleCheckbox = settingsInteractables.transform.Find("DensityScaleToggle").GetComponent<Toggle>();
                    densityScaleCheckbox.isOn = BetterFog.densityScaleEnabled;
                    currentDensityVal = settingsText.transform.Find("CurrentDensityVal").GetComponent<TextMeshProUGUI>();
                    densityScaleVal = settingsText.transform.Find("DensityScaleVal").GetComponent<TextMeshProUGUI>();
                    calcDensityVal = settingsText.transform.Find("CalcDensityVal").GetComponent<TextMeshProUGUI>();

                    matchVal = settingsText.transform.Find("MatchVal").GetComponent<TextMeshProUGUI>();
                    detectionsVal = settingsText.transform.Find("DetectionsVal").GetComponent<TextMeshProUGUI>();

                    excludeShipCheckbox = settingsInteractables.transform.Find("ExcludeShipToggle").GetComponent<Toggle>();
                    excludeShipCheckbox.isOn = BetterFog.excludeShipFogEnabled;

                    excludeEnemiesCheckbox = settingsInteractables.transform.Find("ExcludeEnemiesToggle").GetComponent<Toggle>();
                    excludeEnemiesCheckbox.isOn = BetterFog.excludeEnemyFogEnabled;
                    //BetterFog.mls.LogInfo("Fog exclude enemies checkbox found.");

                    autoPresetModeCheckbox = settingsInteractables.transform.Find("AutoPresetModeToggle").GetComponent<Toggle>();
                    autoPresetModeCheckbox.isOn = BetterFog.autoPresetModeEnabled;
                    //BetterFog.mls.LogInfo("Fog auto preset mode checkbox found.");

                    verboseLogsCheckbox = settingsInteractables.transform.Find("VerboseLogsToggle").GetComponent<Toggle>();
                    verboseLogsCheckbox.isOn = BetterFog.verboseLoggingEnabled;

                    closeButton = settingsInteractables.transform.Find("CloseButton").GetComponent<UnityEngine.UI.Button>();

                    if (fogDensitySlider != null && densityValInput != null)
                    {
                        // Initialize the text with the current slider value
                        densityValInput.text = fogDensitySlider.value.ToString();
                        redValInput.text = fogRedSlider.value.ToString();
                        greenValInput.text = fogGreenSlider.value.ToString();
                        blueValInput.text = fogBlueSlider.value.ToString();

                        // Add a listener to update the text and apply the value when the slider changes
                        fogDensitySlider.onValueChanged.AddListener(value => OnSliderValueChanged(fogDensitySlider, value));
                        fogRedSlider.onValueChanged.AddListener(value => OnSliderValueChanged(fogRedSlider, value));
                        fogGreenSlider.onValueChanged.AddListener(value => OnSliderValueChanged(fogGreenSlider, value));
                        fogBlueSlider.onValueChanged.AddListener(value => OnSliderValueChanged(fogBlueSlider, value));

                        // Add a listener to check if the player is typing in an input field
                        densityValInput.onSelect.AddListener(delegate { isTyping = true; supressApplyingFogSettings = true; });
                        redValInput.onSelect.AddListener(delegate { isTyping = true; supressApplyingFogSettings = true; });
                        greenValInput.onSelect.AddListener(delegate { isTyping = true; supressApplyingFogSettings = true; });
                        blueValInput.onSelect.AddListener(delegate { isTyping = true; supressApplyingFogSettings = true; });

                        // Add a listener to check if the player is done typing in an input field
                        densityValInput.onDeselect.AddListener(delegate { isTyping = false; supressApplyingFogSettings = false; });
                        redValInput.onDeselect.AddListener(delegate { isTyping = false; supressApplyingFogSettings = false; });
                        greenValInput.onDeselect.AddListener(delegate { isTyping = false; supressApplyingFogSettings = false; });
                        blueValInput.onDeselect.AddListener(delegate { isTyping = false; supressApplyingFogSettings = false; });

                        // Add a listener to update the input value and text
                        densityValInput.onValueChanged.AddListener(value => OnInputValueChanged(densityValInput, value));
                        redValInput.onValueChanged.AddListener(value => OnInputValueChanged(redValInput, value));
                        greenValInput.onValueChanged.AddListener(value => OnInputValueChanged(greenValInput, value));
                        blueValInput.onValueChanged.AddListener(value => OnInputValueChanged(blueValInput, value));

                        // Add a listener to update the Weather Scale value when the checkbox is toggled
                        densityScaleCheckbox.onValueChanged.AddListener(isChecked => OnDensityScaleCheckboxValueChanged(isChecked));
                        excludeEnemiesCheckbox.onValueChanged.AddListener(isChecked => OnExcludeEnemiesCheckboxValueChanged(isChecked));
                        excludeShipCheckbox.onValueChanged.AddListener(isChecked => OnExcludeShipCheckboxValueChanged(isChecked));
                        autoPresetModeCheckbox.onValueChanged.AddListener(isChecked => OnAutoPresetModeCheckboxValueChanged(isChecked));
                        verboseLogsCheckbox.onValueChanged.AddListener(isChecked => OnVerboseLogsCheckboxValueChanged(isChecked));

                        closeButton.onClick.AddListener(delegate
                        {
                            DisableSettings();
                        });
                    }
                }
                else
                {
                    BetterFog.mls.LogError("FogSettings prefab not found in AssetBundle.");
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
        //--------------------------------- Start Values Management ---------------------------------

        private void OnValueChanged(string sourceName, float value)
        {
            if (BetterFog.verboseLoggingEnabled)
                BetterFog.mls.LogInfo($"Value changed: {sourceName} = {value}");

            switch (sourceName)
            {
                case "Density":
                    if (fogDensitySlider != null && densityValInput != null)
                    {
                        // Synchronize slider and input field
                        if (value < 0)
                            value = 0;
                        else if (value > 15000)
                            value = 15000;
                        
                        fogDensitySlider.value = value;
                        densityValInput.text = value.ToString("0");
                        currentDensityVal.text = BetterFog.currentPreset.MeanFreePath.ToString("00000.000");
                        calcDensityVal.text = (BetterFog.currentPreset.MeanFreePath * BetterFog.combinedDensityScale).ToString("00000.000");
                        BetterFog.currentPreset.MeanFreePath = value;
                    }
                    break;

                case "Red":
                    if (fogRedSlider != null && redValInput != null)
                    {
                        fogRedSlider.value = value;
                        redValInput.text = value.ToString("0.00");
                        BetterFog.currentPreset.AlbedoR = value;
                    }
                    break;

                case "Green":
                    if (fogGreenSlider != null && greenValInput != null)
                    {
                        fogGreenSlider.value = value;
                        greenValInput.text = value.ToString("0.00");
                        BetterFog.currentPreset.AlbedoG = value;
                    }
                    break;

                case "Blue":
                    if (fogBlueSlider != null && blueValInput != null)
                    {
                        fogBlueSlider.value = value;
                        blueValInput.text = value.ToString("0.00");
                        BetterFog.currentPreset.AlbedoB = value;
                    }
                    break;
            }
        }

        private IEnumerator ApplySliderSettingsWhilePressed()
        {
            while (isMouseDown)
            {
                Vector2 mousePreviousPosition = Mouse.current.position.ReadValue();
                yield return new WaitForSeconds(0.001f);
                isMouseDown = Mouse.current.leftButton.IsPressed();
                if (isMouseDown && !(mousePreviousPosition == Mouse.current.position.ReadValue()))
                {
                    Debug.Log("Applying fog settings while mouse is pressed.");
                    BetterFog.ApplyFogSettings(false);
                }
            }
            supressApplyingFogSettings = false;
            Debug.Log("Stopped applying fog settings. Mouse is no longer down.");
        }

        private void OnSliderValueChanged(Slider slider, float value)
        {
            if (slider == fogDensitySlider)
                OnValueChanged("Density", value);
            else if (slider == fogRedSlider)
                OnValueChanged("Red", value);
            else if (slider == fogGreenSlider)
                OnValueChanged("Green", value);
            else if (slider == fogBlueSlider)
                OnValueChanged("Blue", value);

            // Check if mouse is over the TMP_Slider and the left mouse button is pressed
            rectTransform = slider.GetComponent<RectTransform>(); // Get the RectTransform component to detect contact with mouse
            Vector2 mousePosition = Mouse.current.position.ReadValue();

            if (!BetterFog.lockPresetValueModification && !supressApplyingFogSettings && RectTransformUtility.RectangleContainsScreenPoint(rectTransform, mousePosition, Camera.main))
            {
                if (Mouse.current.leftButton.IsPressed()) // 0 is for left-click
                {
                    supressApplyingFogSettings = true;
                    isMouseDown = true;
                    Debug.Log("Mouse is down on TMP_Slider. Starting coroutine to apply fog settings.");
                    StartCoroutine(ApplySliderSettingsWhilePressed());
                }
            }
        }

        private void OnInputValueChanged(TMP_InputField input, string inputValue)
        {
            float value;
            if (float.TryParse(inputValue, out value))
            {
                if (input == densityValInput)
                    OnValueChanged("Density", value);
                else if (input == redValInput)
                    OnValueChanged("Red", value);
                else if (input == greenValInput)
                    OnValueChanged("Green", value);
                else if (input == blueValInput)
                    OnValueChanged("Blue", value);
            }

            // Check if the player is typing in an input field
            if (isTyping)
            {
                BetterFog.ApplyFogSettings(false);
            }
        }


        //--------------------------------- End Values Management ---------------------------------
        //--------------------------------- Start Slider Adjustment ---------------------------------

        private void UpdateSlidersWithCurrentPreset()
        {
            // Ensure sliders and preset are valid
            if (fogDensitySlider != null && densityValInput != null &&
                fogRedSlider != null && redValInput != null &&
                fogGreenSlider != null && greenValInput != null &&
                fogBlueSlider != null && blueValInput != null &&
                BetterFog.currentPreset != null)
            {
                // Example update logic: assuming currentPreset has properties for these values
                fogDensitySlider.value = BetterFog.currentPreset.MeanFreePath;
                densityValInput.text = fogDensitySlider.value.ToString();

                fogRedSlider.value = BetterFog.currentPreset.AlbedoR;
                redValInput.text = fogRedSlider.value.ToString();

                fogGreenSlider.value = BetterFog.currentPreset.AlbedoG;
                greenValInput.text = fogGreenSlider.value.ToString();

                fogBlueSlider.value = BetterFog.currentPreset.AlbedoB;
                blueValInput.text = fogBlueSlider.value.ToString();

                // Log updates for debugging
                //BetterFog.mls.LogInfo($"Updated sliders to current preset: {BetterFog.currentPreset.PresetName}");
            }
            else
            {
                //BetterFog.mls.LogError("Cannot update sliders: One or more components are missing or currentPreset is null.");
            }
        }

        //--------------------------------- End Slider Adjustment ---------------------------------
        //--------------------------------- Start Checkbox Adjustment ---------------------------------

        private void OnDensityScaleCheckboxValueChanged(bool isChecked)
        {
            if (BetterFog.verboseLoggingEnabled)
                BetterFog.mls.LogInfo($"Density Scale Checkbox value changed: {isChecked}");
            BetterFog.densityScaleEnabled = isChecked;
            if (!supressApplyingFogSettings)
            {
                supressApplyingFogSettings = true;
                BetterFog.ApplyFogSettings(false);
                supressApplyingFogSettings = false;
            }
            UpdateText();
        }

        private void OnExcludeShipCheckboxValueChanged(bool isChecked)
        {
            if (BetterFog.verboseLoggingEnabled)
                BetterFog.mls.LogInfo($"Exclude Ship Checkbox value changed: {isChecked}");
            BetterFog.excludeShipFogEnabled = isChecked;
            if (!supressApplyingFogSettings)
            {
                supressApplyingFogSettings = true;
                BetterFog.ApplyFogSettings(false);
                supressApplyingFogSettings = false;
            }
                
        }

        private void OnExcludeEnemiesCheckboxValueChanged(bool isChecked)
        {
            if (BetterFog.verboseLoggingEnabled)
                BetterFog.mls.LogInfo($"Exclude Enemies Checkbox value changed: {isChecked}");
            BetterFog.excludeEnemyFogEnabled = isChecked;
            if (!supressApplyingFogSettings)
            {
                supressApplyingFogSettings = true;
                BetterFog.ApplyFogSettings(false);
                supressApplyingFogSettings = false;
            }
        }

        private void OnAutoPresetModeCheckboxValueChanged(bool isChecked)
        {
            if (BetterFog.verboseLoggingEnabled)
                BetterFog.mls.LogInfo($"Auto Preset Mode Checkbox value changed: {isChecked}");
            BetterFog.autoPresetModeEnabled = isChecked;
            if (isChecked && !supressApplyingFogSettings)
            {
                supressApplyingFogSettings = true;
                BetterFog.ApplyFogSettings(true);
                supressApplyingFogSettings = false;
            }
            else
            {
                BetterFog.lockModeDropdownModification = false;
                LockModeDropdownInteract(BetterFog.lockModeDropdownModification);
                if (!(BetterFog.currentMode.Name == "Vanilla" || BetterFog.currentMode.Name == "No Fog"))
                {
                    BetterFog.lockPresetDropdownModification = false;
                    BetterFog.lockPresetValueModification = false;
                    LockPresetDropdownInteract(BetterFog.lockPresetDropdownModification);
                    LockPresetValueInteract(BetterFog.lockPresetValueModification);
                }
                IngameKeybinds.Instance.nextPresetHotkey.Enable();
                IngameKeybinds.Instance.nextModeHotkey.Enable();
            }
            UpdateText();
        }

        private void OnVerboseLogsCheckboxValueChanged(bool isChecked)
        {
            BetterFog.mls.LogInfo($"Verbose Logs Checkbox value changed: {isChecked}");
            BetterFog.verboseLoggingEnabled = isChecked;
            if (!supressApplyingFogSettings)
            {
                supressApplyingFogSettings = true;
                BetterFog.ApplyFogSettings(false);
                supressApplyingFogSettings = false;
            }
        }

        private void UpdateCheckboxValues()
        {
            UpdateDensityScaleCheckbox();
            UpdateExcludeShipCheckbox();
            UpdateExcludeEnemiesCheckbox();
            UpdateVerboseLogsCheckbox();
            UpdateAutoPresetModeCheckbox();
        }

        private void UpdateDensityScaleCheckbox()
        {
            if (densityScaleCheckbox != null)
            {
                densityScaleCheckbox.isOn = BetterFog.densityScaleEnabled;
            }
        }
        private void UpdateExcludeShipCheckbox()
        {
            if (excludeShipCheckbox != null)
            {
                excludeShipCheckbox.isOn = BetterFog.excludeShipFogEnabled;
            }
        }

        private void UpdateExcludeEnemiesCheckbox()
        {
            if (excludeEnemiesCheckbox != null)
            {
                excludeEnemiesCheckbox.isOn = BetterFog.excludeEnemyFogEnabled;
            }
        }

        private void UpdateVerboseLogsCheckbox()
        {
            if (verboseLogsCheckbox != null)
            {
                verboseLogsCheckbox.isOn = BetterFog.verboseLoggingEnabled;
            }
        }

        private void UpdateAutoPresetModeCheckbox()
        {
            if (autoPresetModeCheckbox != null)
            {
                autoPresetModeCheckbox.isOn = BetterFog.autoPresetModeEnabled;
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
            supressApplyingFogSettings = true;
            UpdateText();
            UpdateDropdownWithCurrentOption(presetDropdown);
            UpdateDropdownWithCurrentOption(modeDropdown);
            UpdateSlidersWithCurrentPreset();
            UpdateCheckboxValues();
            supressApplyingFogSettings = false;
        }

        private void UpdateText()
        {
            currentWeatherVal.text = BetterFog.currentWeatherType;
            currentMoonVal.text = BetterFog.currentLevel;
            currentDensityVal.text = BetterFog.currentPreset.MeanFreePath.ToString("00000.0000");
            densityScaleVal.text = ("x" + BetterFog.combinedDensityScale.ToString("00.0000"));
            // calcDensityVal.text = (BetterFog.maxDensitySliderValue - ((BetterFog.maxDensitySliderValue - BetterFog.currentPreset.MeanFreePath) * BetterFog.combinedDensityScale)).ToString("00000.000");
            calcDensityVal.text = (BetterFog.currentPreset.MeanFreePath * BetterFog.combinedDensityScale).ToString("00000.000");
            BetterFog.mls.LogInfo($"{BetterFog.maxDensitySliderValue} - {BetterFog.maxDensitySliderValue - BetterFog.currentPreset.MeanFreePath} * {BetterFog.combinedDensityScale}");
            if (BetterFog.autoPresetModeEnabled)
            {
                if (!(BetterFog.matchedPreset == null))
                {
                    matchVal.text = $"Match Found!";
                    detectionsVal.text = $"{BetterFog.matchedPreset.Conditions[0]},\n" +
                    (BetterFog.matchedPreset.Conditions.Count > 1 ? $"{BetterFog.matchedPreset.Conditions[1]}\n" : "") +
                    $">{BetterFog.matchedPreset.Effect}<\n";
                }
                else
                {
                    matchVal.text = "Match Not Found";
                    detectionsVal.text = "";
                }

            }
            else
            {
                matchVal.text = "";
                detectionsVal.text = "";
            }
        }

        private void UpdateDropdownWithCurrentOption(TMP_Dropdown dropdown)
        {
            if (dropdown != null)
            {
                if (dropdown == presetDropdown)
                {
                    dropdown.value = BetterFog.currentPresetIndex;
                    //BetterFog.mls.LogInfo($"Preset dropdown updated to: {BetterFog.currentPreset.PresetName}");
                    if (BetterFog.verboseLoggingEnabled)
                        BetterFog.mls.LogInfo($"Preset dropdown updated to: {BetterFog.fogConfigPresets[dropdown.value].PresetName}");
                }
                else if (dropdown == modeDropdown)
                {
                    dropdown.value = BetterFog.currentModeIndex;
                    //BetterFog.mls.LogInfo($"Mode dropdown updated to: {BetterFog.currentMode.Name}");
                    if (BetterFog.verboseLoggingEnabled)
                        BetterFog.mls.LogInfo($"Mode dropdown updated to: {BetterFog.fogModes[dropdown.value].Name}");
                }
            }
            else
            {
                if (BetterFog.verboseLoggingEnabled)
                    BetterFog.mls.LogError("Dropdown is null. Cannot update");
            }
        }

        private void OnPresetChanged(int index)
        {
            // Update the currentPreset based on the selected dropdown option
            BetterFog.currentPresetIndex = index;
            BetterFog.currentPreset = BetterFog.fogConfigPresets[index];
            UpdateSlidersWithCurrentPreset();
            if (!supressApplyingFogSettings)
            {
                supressApplyingFogSettings = true;
                BetterFog.ApplyFogSettings(false);
                supressApplyingFogSettings = false;
            }
        }

        private void OnModeChanged(int currentIndex, int previousIndex)
        {
            BetterFog.mls.LogInfo($"GUI Mode Update - Mode changed from: {BetterFog.currentMode.Name} to: {BetterFog.fogModes[currentIndex].Name}");
            // Update the current mode in BetterFog
            BetterFog.currentModeIndex = currentIndex;
            BetterFog.currentMode = BetterFog.fogModes[currentIndex];

            BetterFog.Instance.UpdateMode();

            BetterFog.mls.LogInfo($"SupressApplyingFogSettings before mode change: {supressApplyingFogSettings}");

            if (!supressApplyingFogSettings)
            {
                supressApplyingFogSettings = true;
                BetterFog.ApplyFogSettings(false);
                supressApplyingFogSettings = false;
            }

            BetterFog.mls.LogInfo($"SupressApplyingFogSettings after mode change: {supressApplyingFogSettings}");
            BetterFog.UpdateLockInteractionSettings();
        }

        //--------------------------------- End Dropdown Adjustment ---------------------------------
        //--------------------------------- Start Settings Enable/Disable ---------------------------------

        public void ToggleSettings()
        {
            if (!(NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsClient))
            {
                BetterFog.mls.LogError("FogSettingsManager cannot be manipulated when not in a lobby.");
                return;
            }

            if (BetterFog.isFogSettingsActive)
            {
                DisableSettings();
            }
            else
            {
                EnableSettings();
                UpdateSettings();
            }
        }

        public void EnableSettings()
        {
            ulong localClientId = NetworkManager.Singleton.LocalClientId; // Get the local client's ID

            // Assign the local player using the same logic as for the host
            BetterFog.player = BetterFog.Instance.FindLocalPlayer(localClientId);
            BetterFog.player.disableInteract = true;
            BetterFog.player.disableLookInput = true;
            BetterFog.player.disableMoveInput = true;
            BetterFog.player.inSpecialMenu = true;

            BetterFog.isFogSettingsActive = true;

            if (settingsCanvas == null) // If the canvas is null, reinitialize
            {
                // Destroy the existing settingsCanvas
                Destroy(settingsCanvas);
                settingsCanvas = null; // Clear the reference

                if (BetterFog.verboseLoggingEnabled)
                    BetterFog.mls.LogWarning("Canvas prefab not found. Initializing new Canvas.");
                UnloadAssetBundle();
                Initialize(); // Method to handle the initialization of settingsCanvas
            }

            // Assume the canvas is ready
            settingsCanvas.SetActive(true);
            SetCurrentOption(presetDropdown);
            SetCurrentOption(modeDropdown);

            BetterFog.UpdateLockInteractionSettings();
            LockPresetDropdownInteract(BetterFog.lockPresetDropdownModification);
            LockPresetValueInteract(BetterFog.lockPresetValueModification);
            LockPresetDropdownInteract(BetterFog.lockPresetDropdownModification);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            BetterFog.mls.LogInfo("Fog Settings opened.");

            if (BetterFog.verboseLoggingEnabled)
                BetterFog.mls.LogInfo($"disableInteract: {BetterFog.player.disableInteract}, disableLookInput { BetterFog.player.disableLookInput}, disableMoveInput: {BetterFog.player.disableMoveInput}");
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
            BetterFog.player.disableInteract = false;
            BetterFog.player.disableLookInput = false;
            BetterFog.player.disableMoveInput = false;
            BetterFog.player.inSpecialMenu = false;

            BetterFog.isFogSettingsActive = false;
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

                if (BetterFog.verboseLoggingEnabled)
                    BetterFog.mls.LogWarning("Canvas prefab not found. Initializing new Canvas.");
                UnloadAssetBundle();
                Initialize(); // Method to handle the initialization of settingsCanvas
            }

            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsClient)
                Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public void LockPresetDropdownInteract(bool isLocked)
        {
            presetDropdown.interactable = !isLocked;
        }

        public void LockPresetValueInteract(bool isLocked)
        {

            redValInput.interactable = !isLocked;
            greenValInput.interactable = !isLocked;
            blueValInput.interactable = !isLocked;
            densityValInput.interactable = !isLocked;

            fogRedSlider.interactable = !isLocked;
            fogGreenSlider.interactable = !isLocked;
            fogBlueSlider.interactable = !isLocked;
            fogDensitySlider.interactable = !isLocked;

            densityScaleCheckbox.interactable = !isLocked;
        }

        public void LockModeDropdownInteract(bool isLocked)
        {
            modeDropdown.interactable = !isLocked;
        }

        private void OnDestroy()
        {
            if (fogsettingsgui != null)
            {
                fogsettingsgui.Unload(false);
            }
            instance = null; // Ensure instance is nullified when destroyed
        }
    }
}
