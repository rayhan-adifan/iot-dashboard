using UnityEngine;
using UnityEngine.UI;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;

public class DashboardPrefabSetup : MonoBehaviour
{
    [MenuItem("Tools/Create Dashboard Prefab")]
    public static void CreateDashboardPrefab()
    {
        // Create the main dashboard GameObject
        GameObject dashboard = new GameObject("DashboardPrefab");
        RectTransform dashboardRect = dashboard.AddComponent<RectTransform>();
        dashboardRect.sizeDelta = new Vector2(900, 500);
        
        // Add Canvas component
        Canvas canvas = dashboard.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        dashboard.AddComponent<CanvasScaler>();
        dashboard.AddComponent<GraphicRaycaster>();
        
        // Create a panel for the background
        GameObject panel = new GameObject("Panel");
        panel.transform.SetParent(dashboard.transform, false);
        
        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);
        
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;
        
        // Create chart container
        GameObject chartContainer = new GameObject("ChartContainer");
        chartContainer.transform.SetParent(panel.transform, false);
        
        RectTransform chartRect = chartContainer.AddComponent<RectTransform>();
        chartRect.anchorMin = new Vector2(0, 0);
        chartRect.anchorMax = new Vector2(1, 0.8f);
        chartRect.sizeDelta = Vector2.zero;
        chartRect.offsetMin = new Vector2(50, 50);
        chartRect.offsetMax = new Vector2(-50, -50);
        
        // Create UI elements for axis labels
        GameObject axisYLabels = new GameObject("AxisYLabels");
        axisYLabels.transform.SetParent(chartContainer.transform, false);
        
        RectTransform axisYRect = axisYLabels.AddComponent<RectTransform>();
        axisYRect.anchorMin = new Vector2(0, 0);
        axisYRect.anchorMax = new Vector2(0, 1);
        axisYRect.sizeDelta = new Vector2(40, 0);
        axisYRect.anchoredPosition = new Vector2(-20, 0);
        
        GameObject axisXLabels = new GameObject("AxisXLabels");
        axisXLabels.transform.SetParent(chartContainer.transform, false);
        
        RectTransform axisXRect = axisXLabels.AddComponent<RectTransform>();
        axisXRect.anchorMin = new Vector2(0, 0);
        axisXRect.anchorMax = new Vector2(1, 0);
        axisXRect.sizeDelta = new Vector2(0, 40);
        axisXRect.anchoredPosition = new Vector2(0, -20);
        
        // Create controls panel
        GameObject controlsPanel = new GameObject("ControlsPanel");
        controlsPanel.transform.SetParent(panel.transform, false);
        
        RectTransform controlsRect = controlsPanel.AddComponent<RectTransform>();
        controlsRect.anchorMin = new Vector2(0, 0.8f);
        controlsRect.anchorMax = new Vector2(1, 1);
        controlsRect.sizeDelta = Vector2.zero;
        controlsRect.offsetMin = new Vector2(20, 10);
        controlsRect.offsetMax = new Vector2(-20, -10);
        
        Image controlsImage = controlsPanel.AddComponent<Image>();
        controlsImage.color = new Color(0.2f, 0.2f, 0.2f, 0.5f);
        
        // Add dropdown for sensor selection
        GameObject sensorDropdownObj = CreateDropdown("SensorDropdown", controlsPanel.transform, new Vector2(0.05f, 0.5f), new Vector2(150, 30));
        
        // Add dropdown for year selection
        GameObject yearDropdownObj = CreateDropdown("YearDropdown", controlsPanel.transform, new Vector2(0.25f, 0.5f), new Vector2(100, 30));
        
        // Add dropdown for month selection
        GameObject monthDropdownObj = CreateDropdown("MonthDropdown", controlsPanel.transform, new Vector2(0.4f, 0.5f), new Vector2(100, 30));
        
        // Add parameter dropdown
        GameObject paramDropdownObj = CreateDropdown("ParameterDropdown", controlsPanel.transform, new Vector2(0.55f, 0.5f), new Vector2(150, 30));
        
        // Add multi-series toggle
        GameObject toggleObj = CreateToggle("MultiSeriesToggle", controlsPanel.transform, new Vector2(0.75f, 0.5f));
        
        // Add refresh button
        GameObject refreshButtonObj = CreateButton("RefreshButton", controlsPanel.transform, new Vector2(0.9f, 0.5f), "Refresh");
        
        // Add status text
        GameObject statusTextObj = new GameObject("StatusText");
        statusTextObj.transform.SetParent(controlsPanel.transform, false);
        
        TextMeshProUGUI statusText = statusTextObj.AddComponent<TextMeshProUGUI>();
        statusText.text = "Ready";
        statusText.fontSize = 12;
        statusText.alignment = TextAlignmentOptions.Left;
        
        RectTransform statusRect = statusTextObj.GetComponent<RectTransform>();
        statusRect.anchorMin = new Vector2(0, 0);
        statusRect.anchorMax = new Vector2(1, 0.3f);
        statusRect.sizeDelta = Vector2.zero;
        
        // Add chart title
        GameObject titleTextObj = new GameObject("ChartTitleText");
        titleTextObj.transform.SetParent(panel.transform, false);
        
        TextMeshProUGUI titleText = titleTextObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "Sensor Data Chart";
        titleText.fontSize = 18;
        titleText.alignment = TextAlignmentOptions.Center;
        
        RectTransform titleRect = titleTextObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.3f, 0.9f);
        titleRect.anchorMax = new Vector2(0.7f, 0.95f);
        titleRect.sizeDelta = Vector2.zero;
        
        // Add min/max value texts
        GameObject minValueObj = new GameObject("MinValueText");
        minValueObj.transform.SetParent(chartContainer.transform, false);
        
        TextMeshProUGUI minValueText = minValueObj.AddComponent<TextMeshProUGUI>();
        minValueText.text = "0.00";
        minValueText.fontSize = 12;
        minValueText.alignment = TextAlignmentOptions.Left;
        
        RectTransform minValueRect = minValueObj.GetComponent<RectTransform>();
        minValueRect.anchorMin = new Vector2(0, 0);
        minValueRect.anchorMax = new Vector2(0, 0);
        minValueRect.sizeDelta = new Vector2(60, 20);
        minValueRect.anchoredPosition = new Vector2(-40, 0);
        
        GameObject maxValueObj = new GameObject("MaxValueText");
        maxValueObj.transform.SetParent(chartContainer.transform, false);
        
        TextMeshProUGUI maxValueText = maxValueObj.AddComponent<TextMeshProUGUI>();
        maxValueText.text = "100.00";
        maxValueText.fontSize = 12;
        maxValueText.alignment = TextAlignmentOptions.Left;
        
        RectTransform maxValueRect = maxValueObj.GetComponent<RectTransform>();
        maxValueRect.anchorMin = new Vector2(0, 1);
        maxValueRect.anchorMax = new Vector2(0, 1);
        maxValueRect.sizeDelta = new Vector2(60, 20);
        maxValueRect.anchoredPosition = new Vector2(-40, 0);
        
        // Add buttons for navigation
        GameObject prevButtonObj = CreateButton("PrevButton", chartContainer.transform, new Vector2(0, 0.5f), "<");
        RectTransform prevButtonRect = prevButtonObj.GetComponent<RectTransform>();
        prevButtonRect.anchoredPosition = new Vector2(-30, 0);
        
        GameObject nextButtonObj = CreateButton("NextButton", chartContainer.transform, new Vector2(1, 0.5f), ">");
        RectTransform nextButtonRect = nextButtonObj.GetComponent<RectTransform>();
        nextButtonRect.anchoredPosition = new Vector2(30, 0);
        
        // Add API_Importer1 component
        API_Importer1 apiImporter = dashboard.AddComponent<API_Importer1>();
        apiImporter.sensorDropdown = sensorDropdownObj.GetComponent<TMP_Dropdown>();
        apiImporter.yearDropdown = yearDropdownObj.GetComponent<TMP_Dropdown>();
        apiImporter.monthDropdown = monthDropdownObj.GetComponent<TMP_Dropdown>();
        apiImporter.statusText = statusText;
        apiImporter.refreshButton = refreshButtonObj.GetComponent<Button>();
        
        // Add SensorDataVisualizer component
        SensorDataVisualizer visualizer = dashboard.AddComponent<SensorDataVisualizer>();
        visualizer.apiImporter = apiImporter;
        visualizer.parameterDropdown = paramDropdownObj.GetComponent<TMP_Dropdown>();
        visualizer.chartContainer = chartContainer.transform;
        visualizer.axisYLabels = axisYRect;
        visualizer.axisXLabels = axisXRect;
        visualizer.chartTitleText = titleText;
        visualizer.minValueText = minValueText;
        visualizer.maxValueText = maxValueText;
        visualizer.multiSeriesToggle = toggleObj.GetComponent<Toggle>();
        
        // Save the prefab
        #if UNITY_EDITOR
        string prefabPath = "Assets/Prefabs/DashboardPrefab.prefab";
        
        // Ensure directory exists
        string directory = System.IO.Path.GetDirectoryName(prefabPath);
        if (!System.IO.Directory.Exists(directory))
        {
            System.IO.Directory.CreateDirectory(directory);
        }
        
        // Create the prefab
        PrefabUtility.SaveAsPrefabAsset(dashboard, prefabPath);
        
        // Cleanup scene object
        Object.DestroyImmediate(dashboard);
        
        Debug.Log("Dashboard prefab created at: " + prefabPath);
        #endif
    }
    
    private static GameObject CreateDropdown(string name, Transform parent, Vector2 position, Vector2 size)
    {
        GameObject dropdownObj = new GameObject(name);
        dropdownObj.transform.SetParent(parent, false);
        
        RectTransform rectTransform = dropdownObj.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(position.x, position.y);
        rectTransform.anchorMax = new Vector2(position.x, position.y);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.sizeDelta = size;
        
        Image image = dropdownObj.AddComponent<Image>();
        image.color = Color.white;
        
        TMP_Dropdown dropdown = dropdownObj.AddComponent<TMP_Dropdown>();
        
        // Create template
        GameObject template = new GameObject("Template");
        template.transform.SetParent(dropdownObj.transform, false);
        
        RectTransform templateRect = template.AddComponent<RectTransform>();
        templateRect.anchorMin = new Vector2(0, 0);
        templateRect.anchorMax = new Vector2(1, 0);
        templateRect.pivot = new Vector2(0.5f, 1);
        templateRect.sizeDelta = new Vector2(0, 150);
        templateRect.anchoredPosition = new Vector2(0, 0);
        
        Image templateImage = template.AddComponent<Image>();
        templateImage.color = Color.white;
        
        // Add a mask for scrolling
        template.AddComponent<Mask>();
        
        dropdown.template = templateRect;
        
        // Add label
        GameObject captionTextObj = new GameObject("Label");
        captionTextObj.transform.SetParent(dropdownObj.transform, false);
        
        TextMeshProUGUI captionText = captionTextObj.AddComponent<TextMeshProUGUI>();
        captionText.text = name;
        captionText.alignment = TextAlignmentOptions.Center;
        
        RectTransform labelRect = captionTextObj.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0, 0);
        labelRect.anchorMax = new Vector2(1, 1);
        labelRect.sizeDelta = Vector2.zero;
        
        dropdown.captionText = captionText;
        
        return dropdownObj;
    }
    
    private static GameObject CreateToggle(string name, Transform parent, Vector2 position)
    {
        GameObject toggleObj = new GameObject(name);
        toggleObj.transform.SetParent(parent, false);
        
        RectTransform rectTransform = toggleObj.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(position.x, position.y);
        rectTransform.anchorMax = new Vector2(position.x, position.y);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.sizeDelta = new Vector2(120, 20);
        
        Toggle toggle = toggleObj.AddComponent<Toggle>();
        
        // Create background
        GameObject background = new GameObject("Background");
        background.transform.SetParent(toggleObj.transform, false);
        
        Image bgImage = background.AddComponent<Image>();
        bgImage.color = new Color(0.2f, 0.2f, 0.2f);
        
        RectTransform bgRect = background.GetComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0, 0.5f);
        bgRect.anchorMax = new Vector2(0, 0.5f);
        bgRect.pivot = new Vector2(0.5f, 0.5f);
        bgRect.sizeDelta = new Vector2(20, 20);
        
        // Create checkmark
        GameObject checkmark = new GameObject("Checkmark");
        checkmark.transform.SetParent(background.transform, false);
        
        Image checkImage = checkmark.AddComponent<Image>();
        checkImage.color = new Color(0.2f, 0.7f, 0.2f);
        
        RectTransform checkRect = checkmark.GetComponent<RectTransform>();
        checkRect.anchorMin = new Vector2(0.1f, 0.1f);
        checkRect.anchorMax = new Vector2(0.9f, 0.9f);
        checkRect.sizeDelta = Vector2.zero;
        
        // Create label
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(toggleObj.transform, false);
        
        TextMeshProUGUI label = labelObj.AddComponent<TextMeshProUGUI>();
        label.text = "Multi Series";
        label.fontSize = 12;
        label.alignment = TextAlignmentOptions.Left;
        
        RectTransform labelRect = labelObj.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0, 0);
        labelRect.anchorMax = new Vector2(1, 1);
        labelRect.offsetMin = new Vector2(25, 0);
        
        // Configure toggle component
        toggle.targetGraphic = bgImage;
        toggle.graphic = checkImage;
        toggle.isOn = false;
        
        return toggleObj;
    }
    
    private static GameObject CreateButton(string name, Transform parent, Vector2 position, string text)
    {
        GameObject buttonObj = new GameObject(name);
        buttonObj.transform.SetParent(parent, false);
        
        RectTransform rectTransform = buttonObj.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(position.x, position.y);
        rectTransform.anchorMax = new Vector2(position.x, position.y);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.sizeDelta = new Vector2(80, 30);
        
        Image image = buttonObj.AddComponent<Image>();
        image.color = new Color(0.2f, 0.4f, 0.8f);
        
        Button button = buttonObj.AddComponent<Button>();
        button.targetGraphic = image;
        
        ColorBlock colors = button.colors;
        colors.highlightedColor = new Color(0.3f, 0.5f, 0.9f);
        colors.pressedColor = new Color(0.1f, 0.3f, 0.7f);
        button.colors = colors;
        
        // Add text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        
        TextMeshProUGUI buttonText = textObj.AddComponent<TextMeshProUGUI>();
        buttonText.text = text;
        buttonText.alignment = TextAlignmentOptions.Center;
        buttonText.fontSize = 14;
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        return buttonObj;
    }
}
#endif