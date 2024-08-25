using BepInEx;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class PresetGUI : MonoBehaviour
{
    private static bool fogGUIEnabled = false;
    private static Dropdown dropdown;
    private static List<Canvas> canvases;
    private static Canvas customCanvas;

    // Variables for positioning
    private static float dropdownX = 30;
    private static float dropdownY = 35;

    public static void ActivateGUI()
    {
        /*
        // Search the current scene for all Canvases
        canvases = FindObjectsOfType<Canvas>().ToList();

        if (canvases.Count == 0)
        {
            Debug.LogError("No Canvas found in the scene.");
            return;
        }

        Debug.Log("Detected the following canvases in the scene:");
        foreach (Canvas canvas in canvases)
        {
            Debug.Log(canvas.name);
        }

        // Select one of the canvases to add the Dropdown to
        Canvas selectedCanvas = canvases.Find(canvas => canvas.name == "Canvas");

        // Create a Dropdown UI element
        CreateDropdown(selectedCanvas);
        */

        fogGUIEnabled = true;

        // Create a new Canvas set to Screen Space - Overlay
        CreateCanvas();

        // Create a Dropdown UI element
        CreateDropdown(customCanvas);

        Debug.Log("Fog GUI enabled.");
    }

    public static void DeactivateGUI()
    {
        fogGUIEnabled = false;
        Destroy(dropdown.gameObject);
        Destroy(customCanvas.gameObject);
        Debug.Log("Fog GUI disabled.");
    }

    private static void CreateCanvas()
    {
        // Create a new GameObject for the Canvas
        GameObject canvasObject = new GameObject("CustomCanvas");
        customCanvas = canvasObject.AddComponent<Canvas>();

        // Set the Canvas to Screen Space - Overlay
        customCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        customCanvas.sortingOrder = 100; // Ensure this canvas is on top of other UI elements

        // Add a CanvasScaler component to handle screen size scaling
        CanvasScaler canvasScaler = canvasObject.AddComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(1920, 1080); // Adjust based on your needs

        // Add a GraphicRaycaster to the Canvas to handle UI interactions
        canvasObject.AddComponent<GraphicRaycaster>();
    }

    private static void CreateDropdown(Canvas canvas)
    {
        // Create a new GameObject for the Dropdown
        GameObject dropdownObject = new GameObject("ModDropdown");
        dropdownObject.transform.SetParent(canvas.transform, false);

        // Set up the RectTransform
        RectTransform rectTransform = dropdownObject.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(160, 30); // Adjusted size for visibility
        rectTransform.anchoredPosition = new Vector2(0, -30);

        // Add Dropdown component
        dropdown = dropdownObject.AddComponent<Dropdown>();

        // Create and assign the template for the dropdown
        CreateDropdownTemplate(dropdownObject);

        // Set dropdown options
        dropdown.options = new List<Dropdown.OptionData>
    {
        new Dropdown.OptionData("Option 1"),
        new Dropdown.OptionData("Option 2"),
        new Dropdown.OptionData("Option 3")
    };

        // Set the default selected option to "Option 1"
        dropdown.value = 0;

        // Add a Text component for the dropdown label
        GameObject labelObject = new GameObject("Label");
        labelObject.transform.SetParent(dropdownObject.transform, false);
        Text label = labelObject.AddComponent<Text>();
        label.text = "Option 1"; // Default text
        label.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        label.alignment = TextAnchor.MiddleLeft;
        label.color = Color.black; // Set the text color here
        label.rectTransform.anchorMin = new Vector2(0, 0);
        label.rectTransform.anchorMax = new Vector2(1, 1);
        label.rectTransform.offsetMin = new Vector2(10, 0);
        label.rectTransform.offsetMax = new Vector2(-10, 0);

        dropdown.captionText = label; // Assign the caption text to the dropdown
    }


    private static void CreateDropdownTemplate(GameObject dropdownObject)
    {
        // Create the template GameObject
        GameObject templateObject = new GameObject("Template");
        templateObject.transform.SetParent(dropdownObject.transform, false);
        templateObject.SetActive(false); // Template is inactive by default

        // Add RectTransform to template
        RectTransform templateRect = templateObject.AddComponent<RectTransform>();
        templateRect.anchorMin = new Vector2(0, 0);
        templateRect.anchorMax = new Vector2(1, 0);
        templateRect.pivot = new Vector2(0.5f, 1);
        templateRect.sizeDelta = new Vector2(0, 150); // Adjusted size for dropdown list

        // Add Image component to the template for background
        Image templateImage = templateObject.AddComponent<Image>();
        templateImage.color = Color.white;

        // Add ScrollRect component
        ScrollRect scrollRect = templateObject.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;

        // Create a Mask for the ScrollRect
        GameObject maskObject = new GameObject("Mask");
        maskObject.transform.SetParent(templateObject.transform, false);
        RectTransform maskRect = maskObject.AddComponent<RectTransform>();
        maskRect.anchorMin = new Vector2(0, 0);
        maskRect.anchorMax = new Vector2(1, 1);
        maskRect.sizeDelta = Vector2.zero;

        Image maskImage = maskObject.AddComponent<Image>();
        maskImage.color = Color.white;
        maskImage.type = Image.Type.Sliced;

        Mask mask = maskObject.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        // Create Content GameObject inside the template
        GameObject contentObject = new GameObject("Content");
        contentObject.transform.SetParent(maskObject.transform, false);
        RectTransform contentRect = contentObject.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1); // Adjusted for top-down scrolling
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.sizeDelta = new Vector2(0, 300);

        VerticalLayoutGroup layoutGroup = contentObject.AddComponent<VerticalLayoutGroup>();
        layoutGroup.childForceExpandWidth = true;
        layoutGroup.childControlHeight = true;

        ContentSizeFitter fitter = contentObject.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollRect.content = contentRect;

        // Create Item GameObject
        GameObject itemObject = new GameObject("Item");
        itemObject.transform.SetParent(contentObject.transform, false);

        // Add Toggle component to item
        Toggle itemToggle = itemObject.AddComponent<Toggle>();
        itemToggle.isOn = false; // Default to not selected

        // Add Background Image to Item
        Image itemBackground = itemObject.AddComponent<Image>();
        itemBackground.color = Color.gray;  // Set any color you want for the item background
        itemToggle.targetGraphic = itemBackground;

        // Create Checkmark for the item
        GameObject checkmarkObject = new GameObject("Item Checkmark");
        checkmarkObject.transform.SetParent(itemObject.transform, false);
        Image checkmarkImage = checkmarkObject.AddComponent<Image>();
        checkmarkImage.color = Color.green;  // Set any color you want for the checkmark
        itemToggle.graphic = checkmarkImage;

        // Set RectTransform for checkmark
        RectTransform checkmarkRect = checkmarkObject.GetComponent<RectTransform>();
        checkmarkRect.anchorMin = new Vector2(0.5f, 0.5f);
        checkmarkRect.anchorMax = new Vector2(0.5f, 0.5f);
        checkmarkRect.sizeDelta = new Vector2(20, 20);
        checkmarkRect.anchoredPosition = new Vector2(-5, 0);

        // Add Text component to the item for displaying option text
        GameObject itemTextObject = new GameObject("Item Label");
        itemTextObject.transform.SetParent(itemObject.transform, false);
        Text itemText = itemTextObject.AddComponent<Text>();
        itemText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        itemText.alignment = TextAnchor.MiddleLeft;
        itemText.rectTransform.sizeDelta = new Vector2(0, 30);
        itemText.rectTransform.anchorMin = new Vector2(0, 0);
        itemText.rectTransform.anchorMax = new Vector2(1, 1);
        itemText.rectTransform.offsetMin = new Vector2(10, 0);
        itemText.rectTransform.offsetMax = new Vector2(-10, 0);

        // Assign the created template to the dropdown
        dropdown.template = templateRect;
        dropdown.captionText = itemText;  // Reference to the caption text of the dropdown
        dropdown.itemText = itemText;     // Reference to the text component of the item
    
        // Make sure the dropdown is interactable
        dropdown.interactable = true;
        dropdown.RefreshShownValue();
    }

    public static bool IsGUIActive()
    {
        return fogGUIEnabled;
    }
}
