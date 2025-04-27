using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;

public class DashboardDuplicator : MonoBehaviour
{
    [Header("Dashboard References")]
    public GameObject dashboardPrefab;
    public Transform dashboardContainer;
    public Button createDuplicateButton;
    public int maxDashboards = 9;
    
    [Header("Dashboard Settings")]
    public Vector2 dashboardSize = new Vector2(900f, 500f);
    public float padding = 20f;
    
    // Keep track of all dashboard instances
    private List<GameObject> dashboardInstances = new List<GameObject>();
    private List<API_Importer1> apiImporters = new List<API_Importer1>();
    private List<SensorDataVisualizer> visualizers = new List<SensorDataVisualizer>();
    
    void Start()
    {
        if (createDuplicateButton != null)
        {
            createDuplicateButton.onClick.AddListener(CreateDashboardDuplicate);
        }
        
        // If there's no dashboard instance yet, create the first one
        if (dashboardInstances.Count == 0 && dashboardPrefab != null)
        {
            CreateDashboardDuplicate();
        }
    }
    
    public void CreateDashboardDuplicate()
    {
        if (dashboardInstances.Count >= maxDashboards)
        {
            Debug.LogWarning($"Maximum number of dashboards ({maxDashboards}) reached.");
            return;
        }
        
        if (dashboardPrefab == null)
        {
            Debug.LogError("Dashboard prefab is not assigned!");
            return;
        }
        
        // Create a new dashboard instance
        GameObject newDashboard = Instantiate(dashboardPrefab, dashboardContainer);
        
        // Setup the dashboard layout
        RectTransform rectTransform = newDashboard.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.sizeDelta = dashboardSize;
            
            // Position the dashboard based on current count
            ArrangeDashboards();
        }
        
        // Initialize the dashboard components
        API_Importer1 apiImporter = newDashboard.GetComponentInChildren<API_Importer1>();
        SensorDataVisualizer visualizer = newDashboard.GetComponentInChildren<SensorDataVisualizer>();
        
        if (apiImporter != null && visualizer != null)
        {
            // Give each dashboard a unique tag for identification
            int dashboardId = dashboardInstances.Count;
            newDashboard.name = $"Dashboard_{dashboardId}";
            
            // Setup dashboard-specific options if needed
            // For example, you might want to set different default sensors or time frames for each dashboard
            if (dashboardInstances.Count > 0)
            {
                // Copy settings from the first dashboard or set different defaults
                SetupDashboardDefaults(apiImporter, dashboardId);
            }
            
            // Add control buttons to the dashboard
            AddDashboardControls(newDashboard, dashboardId);
            
            // Add to tracking lists
            dashboardInstances.Add(newDashboard);
            apiImporters.Add(apiImporter);
            visualizers.Add(visualizer);
            
            // Fetch data for the new dashboard
            apiImporter.FetchCurrentSensorData();
        }
        else
        {
            Debug.LogError("Dashboard prefab is missing required components!");
            Destroy(newDashboard);
        }
    }
    
    private void SetupDashboardDefaults(API_Importer1 apiImporter, int dashboardId)
    {
        // For odd-numbered dashboards, select a different sensor than the previous one
        if (dashboardId % 2 == 1 && dashboardId > 0)
        {
            // Get current selected sensor index
            int currentSensorIndex = apiImporter.sensorDropdown.value;
            
            // Select a different sensor (cycle through options)
            int newSensorIndex = (currentSensorIndex + 1) % apiImporter.sensorDropdown.options.Count;
            apiImporter.sensorDropdown.value = newSensorIndex;
            
            // This will trigger the OnSensorChanged event in the API_Importer1 class
        }
        
        // For even-numbered dashboards, select a different time frame
        else if (dashboardId % 2 == 0 && dashboardId > 0)
        {
            // Get current month
            int currentMonth = apiImporter.monthDropdown.value;
            
            // Select the previous month
            int newMonth = (currentMonth + 11) % 12; // Go back one month
            apiImporter.monthDropdown.value = newMonth;
            
            // This will trigger the OnTimeFrameChanged event in the API_Importer1 class
        }
    }
    
    private void AddDashboardControls(GameObject dashboard, int dashboardId)
    {
        // Create a control panel for this dashboard
        GameObject controlPanel = new GameObject("ControlPanel");
        controlPanel.transform.SetParent(dashboard.transform, false);
        
        RectTransform panelRect = controlPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(1, 1);
        panelRect.anchorMax = new Vector2(1, 1);
        panelRect.pivot = new Vector2(1, 1);
        panelRect.anchoredPosition = new Vector2(-10, -10);
        panelRect.sizeDelta = new Vector2(100, 30);
        
        // Add close button
        GameObject closeButton = new GameObject("CloseButton");
        closeButton.transform.SetParent(controlPanel.transform, false);
        
        Image buttonImage = closeButton.AddComponent<Image>();
        buttonImage.color = new Color(0.8f, 0.2f, 0.2f, 0.8f);
        
        Button button = closeButton.AddComponent<Button>();
        button.onClick.AddListener(() => RemoveDashboard(dashboardId));
        
        RectTransform buttonRect = closeButton.GetComponent<RectTransform>();
        buttonRect.anchorMin = Vector2.zero;
        buttonRect.anchorMax = Vector2.one;
        buttonRect.sizeDelta = Vector2.zero;
        
        // Add text to the button
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(closeButton.transform, false);
        
        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = "X";
        text.alignment = TextAlignmentOptions.Center;
        text.fontSize = 16;
        text.color = Color.white;
        
        RectTransform textRect = text.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        
        // Add dashboard title
        GameObject titleObj = new GameObject("DashboardTitle");
        titleObj.transform.SetParent(dashboard.transform, false);
        
        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = $"Dashboard {dashboardId + 1}";
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.fontSize = 16;
        titleText.color = Color.white;
        
        RectTransform titleRect = titleText.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 1);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.pivot = new Vector2(0.5f, 1);
        titleRect.anchoredPosition = new Vector2(0, -5);
        titleRect.sizeDelta = new Vector2(0, 30);
    }
    
    public void RemoveDashboard(int dashboardId)
    {
        // Find the dashboard with the matching ID
        for (int i = 0; i < dashboardInstances.Count; i++)
        {
            if (dashboardInstances[i].name == $"Dashboard_{dashboardId}")
            {
                // Destroy the dashboard GameObject
                Destroy(dashboardInstances[i]);
                
                // Remove from our tracking lists
                dashboardInstances.RemoveAt(i);
                apiImporters.RemoveAt(i);
                visualizers.RemoveAt(i);
                
                // Rearrange remaining dashboards
                ArrangeDashboards();
                return;
            }
        }
    }
    
    private void ArrangeDashboards()
    {
        // Determine grid layout based on count
        int rows = Mathf.CeilToInt(Mathf.Sqrt(dashboardInstances.Count));
        int cols = Mathf.CeilToInt((float)dashboardInstances.Count / rows);
        
        // Calculate size for each dashboard
        float availableWidth = ((RectTransform)dashboardContainer).rect.width;
        float availableHeight = ((RectTransform)dashboardContainer).rect.height;
        
        float width = (availableWidth - (padding * (cols + 1))) / cols;
        float height = (availableHeight - (padding * (rows + 1))) / rows;
        
        // Reduce size if too many dashboards
        Vector2 newSize = new Vector2(
            Mathf.Min(dashboardSize.x, width),
            Mathf.Min(dashboardSize.y, height)
        );
        
        // Position each dashboard
        for (int i = 0; i < dashboardInstances.Count; i++)
        {
            RectTransform rectTransform = dashboardInstances[i].GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                // Update size
                rectTransform.sizeDelta = newSize;
                
                // Calculate position
                int row = i / cols;
                int col = i % cols;
                
                float xPos = padding + col * (newSize.x + padding);
                float yPos = padding + row * (newSize.y + padding);
                
                // Set position
                rectTransform.anchorMin = new Vector2(0, 0);
                rectTransform.anchorMax = new Vector2(0, 0);
                rectTransform.pivot = new Vector2(0, 0);
                rectTransform.anchoredPosition = new Vector2(xPos, yPos);
            }
        }
    }
    
    // Method to manually sync data between dashboards
    public void SyncDashboardTimeFrames()
    {
        if (apiImporters.Count <= 1) return;
        
        // Use the first dashboard as reference
        API_Importer1 reference = apiImporters[0];
        string timeFrame = reference.currentTimeFrame;
        
        // Apply to other dashboards
        for (int i = 1; i < apiImporters.Count; i++)
        {
            string[] parts = timeFrame.Split('-');
            if (parts.Length >= 2)
            {
                // Set the year and month dropdowns
                SetDropdownByValue(apiImporters[i].yearDropdown, parts[0]);
                SetDropdownByValue(apiImporters[i].monthDropdown, parts[1]);
                
                // Refresh the data
                apiImporters[i].FetchCurrentSensorData();
            }
        }
    }
    
    // Method to set dropdown by text value
    private void SetDropdownByValue(TMP_Dropdown dropdown, string value)
    {
        if (dropdown != null)
        {
            for (int i = 0; i < dropdown.options.Count; i++)
            {
                if (dropdown.options[i].text == value)
                {
                    dropdown.value = i;
                    return;
                }
            }
        }
    }
}