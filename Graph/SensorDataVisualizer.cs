using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class SensorDataVisualizer : MonoBehaviour
{
    [Header("References")]
    public API_Importer1 apiImporter;
    public TMP_Dropdown parameterDropdown;
    public Transform chartContainer;
    public GameObject lineRendererPrefab; // Akan dibuat di script terpisah
    
    [Header("Chart Settings")]
    public float chartWidth = 800f;
    public float chartHeight = 400f;
    public float xPadding = 50f;
    public float yPadding = 50f;
    public int maxVisibleDataPoints = 50;
    public Material[] lineMaterials; // Akan diinisialisasi di script terpisah
    
    [Header("UI Elements")]
    public RectTransform axisYLabels;   // Container untuk label sumbu Y
    public RectTransform axisXLabels;   // Container untuk label sumbu X
    public TextMeshProUGUI chartTitleText;
    public TextMeshProUGUI minValueText;
    public TextMeshProUGUI maxValueText;
    public Toggle multiSeriesToggle;
    
    // Untuk menyimpan LineRenderer yang aktif
    private List<LineRenderer> activeRenderers = new List<LineRenderer>();
    private List<string> activeParameters = new List<string>();
    private Dictionary<string, Color> parameterColors = new Dictionary<string, Color>();
    
    // Untuk navigasi data
    private int dataOffset = 0;
    private bool showMultiSeries = false;
    
    private void Start()
    {
        // Memastikan prefab LineRenderer sudah dibuat
        if (lineRendererPrefab == null)
        {
            Debug.LogError("lineRendererPrefab belum ditetapkan! Buat prefab terlebih dahulu atau gunakan LineRendererPrefabCreator.");
            return;
        }
        
        // Mendaftarkan event untuk API_Importer
        if (apiImporter != null)
        {
            apiImporter.OnDataLoaded += OnDataLoaded;
        }
        else
        {
            Debug.LogError("API_Importer tidak ditemukan!");
            return;
        }
        
        // Setup parameter dropdown
        if (parameterDropdown != null)
        {
            parameterDropdown.onValueChanged.AddListener(OnParameterChanged);
        }
        
        // Setup toggle untuk multi-series
        if (multiSeriesToggle != null)
        {
            multiSeriesToggle.onValueChanged.AddListener(OnMultiSeriesToggleChanged);
            showMultiSeries = multiSeriesToggle.isOn;
        }
        
        // Inisialisasi warna untuk parameter
        AssignParameterColors();
    }
    
    private void AssignParameterColors()
    {
        // Daftar warna untuk parameter berbeda
        Color[] colors = new Color[] 
        {
            Color.red,
            Color.blue,
            Color.green,
            Color.yellow,
            Color.cyan,
            Color.magenta,
            new Color(1.0f, 0.5f, 0.0f), // Orange
            new Color(0.5f, 0.0f, 0.5f), // Purple
            new Color(0.0f, 0.5f, 0.5f), // Teal
            new Color(0.5f, 0.5f, 0.0f)  // Olive
        };
        
        // Pre-assign warna untuk semua parameter yang mungkin
        string[] allPossibleParams = new string[] 
        {
            "humidity", "temperature", "eco2", "tvoc", "flowrate", "totalVolume",
            "irradiance", "power_irr", "voltage", "current", "power", "energy",
            "suhu", "v_tds", "tds", "ec", "v_ph", "ph"
        };
        
        for (int i = 0; i < allPossibleParams.Length; i++)
        {
            parameterColors[allPossibleParams[i]] = colors[i % colors.Length];
        }
    }
    
    public void OnDataLoaded(string sensorType)
    {
        // Update dropdown parameter dengan parameter yang tersedia
        UpdateParameterDropdown();
        
        // Hapus chart yang ada
        ClearChart();
        
        // Visualisasikan parameter pertama atau semua jika multi-series aktif
        if (showMultiSeries)
        {
            VisualizeAllParameters();
        }
        else if (parameterDropdown.options.Count > 0)
        {
            string selectedParameter = parameterDropdown.options[parameterDropdown.value].text;
            VisualizeParameter(selectedParameter);
        }
        
        // Update judul chart
        UpdateChartTitle();
    }
    
    private void UpdateParameterDropdown()
    {
        if (parameterDropdown == null) return;
        
        parameterDropdown.ClearOptions();
        
        List<string> parameters = apiImporter.GetCurrentSensorParameters();
        if (parameters != null && parameters.Count > 0)
        {
            parameterDropdown.AddOptions(parameters);
            parameterDropdown.value = 0;
        }
    }
    
    private void OnParameterChanged(int index)
    {
        if (!showMultiSeries && index >= 0 && parameterDropdown.options.Count > 0)
        {
            string selectedParameter = parameterDropdown.options[index].text;
            
            // Hapus chart sebelumnya
            ClearChart();
            
            // Buat chart baru
            VisualizeParameter(selectedParameter);
            
            // Update judul chart
            UpdateChartTitle();
        }
    }
    
    private void OnMultiSeriesToggleChanged(bool isOn)
    {
        showMultiSeries = isOn;
        
        // Hapus chart sebelumnya
        ClearChart();
        
        // Visualisasikan berdasarkan mode
        if (showMultiSeries)
        {
            VisualizeAllParameters();
        }
        else
        {
            // Visualisasikan hanya parameter yang dipilih
            if (parameterDropdown.options.Count > 0)
            {
                string selectedParameter = parameterDropdown.options[parameterDropdown.value].text;
                VisualizeParameter(selectedParameter);
            }
        }
        
        // Update judul chart
        UpdateChartTitle();
    }
    
    private void VisualizeAllParameters()
    {
        List<string> parameters = apiImporter.GetCurrentSensorParameters();
        activeParameters.Clear();
        
        foreach (string parameter in parameters)
        {
            VisualizeParameter(parameter);
            activeParameters.Add(parameter);
        }
    }
    
    private void VisualizeParameter(string parameter)
    {
        // Ambil data untuk parameter ini
        List<float> data = apiImporter.GetParameterData(parameter);
        List<string> timestamps = apiImporter.timestamps;
        
        if (data == null || data.Count == 0 || timestamps == null || timestamps.Count == 0)
        {
            Debug.LogWarning($"Tidak ada data untuk parameter: {parameter}");
            return;
        }
        
        // Hitung rentang tampilan
        int startIndex = Mathf.Max(0, data.Count - maxVisibleDataPoints - dataOffset);
        int endIndex = Mathf.Min(data.Count - 1 - dataOffset, startIndex + maxVisibleDataPoints);
        
        // Jika indeks valid, buat LineRenderer
        if (startIndex <= endIndex && startIndex >= 0 && endIndex < data.Count)
        {
            // Hitung min/max untuk skala sumbu Y
            float min = float.MaxValue;
            float max = float.MinValue;
            
            // Jika menampilkan beberapa seri, cari min/max global
            if (showMultiSeries)
            {
                foreach (string param in apiImporter.GetCurrentSensorParameters())
                {
                    List<float> paramData = apiImporter.GetParameterData(param);
                    if (paramData != null && paramData.Count > 0)
                    {
                        for (int i = startIndex; i <= endIndex; i++)
                        {
                            if (i < paramData.Count)
                            {
                                min = Mathf.Min(min, paramData[i]);
                                max = Mathf.Max(max, paramData[i]);
                            }
                        }
                    }
                }
            }
            else
            {
                // Cari min/max hanya untuk parameter ini
                for (int i = startIndex; i <= endIndex; i++)
                {
                    min = Mathf.Min(min, data[i]);
                    max = Mathf.Max(max, data[i]);
                }
            }
            
            // Tambahkan padding ke rentang
            float range = max - min;
            min -= range * 0.1f;
            max += range * 0.1f;
            
            // Buat LineRenderer baru
            GameObject lineObj = Instantiate(lineRendererPrefab, chartContainer);
            lineObj.SetActive(true);
            LineRenderer lineRenderer = lineObj.GetComponent<LineRenderer>();
            
            if (lineRenderer != null)
            {
                // Setup LineRenderer
                lineRenderer.positionCount = endIndex - startIndex + 1;
                lineRenderer.startWidth = 2f;
                lineRenderer.endWidth = 2f;
                
                // Tetapkan warna berdasarkan parameter
                if (parameterColors.ContainsKey(parameter))
                {
                    lineRenderer.startColor = parameterColors[parameter];
                    lineRenderer.endColor = parameterColors[parameter];
                }
                
                // Tetapkan material jika tersedia
                if (lineMaterials != null && lineMaterials.Length > 0)
                {
                    int materialIndex = System.Array.IndexOf(apiImporter.GetCurrentSensorParameters().ToArray(), parameter) % lineMaterials.Length;
                    lineRenderer.material = lineMaterials[materialIndex];
                }
                
                // Tambahkan titik ke LineRenderer
                for (int i = startIndex; i <= endIndex; i++)
                {
                    // Hitung posisi dalam ruang chart
                    float xPos = xPadding + ((i - startIndex) / (float)(endIndex - startIndex)) * (chartWidth - 2 * xPadding);
                    float yPos = yPadding + ((data[i] - min) / (max - min)) * (chartHeight - 2 * yPadding);
                    
                    // Tetapkan titik
                    lineRenderer.SetPosition(i - startIndex, new Vector3(xPos, yPos, 0));
                }
                
                // Tambahkan ke daftar renderer aktif
                activeRenderers.Add(lineRenderer);
                
                // Update label sumbu
                UpdateAxisLabels(min, max, timestamps, startIndex, endIndex);
            }
        }
    }
    
    private void UpdateAxisLabels(float minValue, float maxValue, List<string> timestamps, int startIndex, int endIndex)
    {
        // Update nilai min/max
        if (minValueText != null)
            minValueText.text = minValue.ToString("F2");
        
        if (maxValueText != null)
            maxValueText.text = maxValue.ToString("F2");
        
        // Update label sumbu Y (buat 5 label)
        if (axisYLabels != null)
        {
            // Hapus label yang ada
            foreach (Transform child in axisYLabels)
            {
                Destroy(child.gameObject);
            }
            
            // Buat label baru
            for (int i = 0; i <= 5; i++)
            {
                GameObject labelObj = new GameObject($"YLabel_{i}");
                labelObj.transform.SetParent(axisYLabels);
                
                TextMeshProUGUI label = labelObj.AddComponent<TextMeshProUGUI>();
                float value = minValue + (maxValue - minValue) * (i / 5f);
                label.text = value.ToString("F2");
                label.fontSize = 12;
                label.alignment = TextAlignmentOptions.Right;
                
                // Posisikan label
                RectTransform rt = label.GetComponent<RectTransform>();
                rt.anchoredPosition = new Vector2(-10, yPadding + (chartHeight - 2 * yPadding) * (i / 5f));
            }
        }
        
        // Update label sumbu X (tampilkan beberapa timestamp)
        if (axisXLabels != null)
        {
            // Hapus label yang ada
            foreach (Transform child in axisXLabels)
            {
                Destroy(child.gameObject);
            }
            
            // Buat label baru (5 label dengan jarak sama)
            for (int i = 0; i <= 4; i++)
            {
                GameObject labelObj = new GameObject($"XLabel_{i}");
                labelObj.transform.SetParent(axisXLabels);
                
                TextMeshProUGUI label = labelObj.AddComponent<TextMeshProUGUI>();
                int index = startIndex + (int)((endIndex - startIndex) * (i / 4f));
                
                // Format timestamp (asumsi dalam format standar)
                string timestamp = timestamps[index];
                string formattedTime = System.DateTime.Parse(timestamp).ToString("MM/dd HH:mm");
                
                label.text = formattedTime;
                label.fontSize = 10;
                label.alignment = TextAlignmentOptions.Center;
                
                // Posisikan label
                RectTransform rt = label.GetComponent<RectTransform>();
                rt.anchoredPosition = new Vector2(xPadding + (chartWidth - 2 * xPadding) * (i / 4f), -10);
                rt.sizeDelta = new Vector2(100, 20);
            }
        }
    }
    
    private void UpdateChartTitle()
    {
        if (chartTitleText != null)
        {
            string sensorType = apiImporter.currentSensor.ToUpper();
            string timeFrame = apiImporter.currentTimeFrame;
            
            if (showMultiSeries)
            {
                chartTitleText.text = $"{sensorType} Data - {timeFrame} - All Parameters";
            }
            else if (parameterDropdown.options.Count > 0)
            {
                string parameter = parameterDropdown.options[parameterDropdown.value].text;
                chartTitleText.text = $"{sensorType} - {parameter} - {timeFrame}";
            }
        }
    }
    
    private void ClearChart()
    {
        // Hapus semua line renderer aktif
        foreach (LineRenderer renderer in activeRenderers)
        {
            if (renderer != null)
            {
                Destroy(renderer.gameObject);
            }
        }
        
        activeRenderers.Clear();
    }
    
    // Metode navigasi untuk scrolling melalui data jika ada lebih banyak dari yang dapat ditampilkan
    public void ShowNextDataSet()
    {
        if (dataOffset > 0)
        {
            dataOffset -= maxVisibleDataPoints / 2;
            if (dataOffset < 0) dataOffset = 0;
            
            // Refresh visualisasi
            RefreshVisualization();
        }
    }
    
    public void ShowPreviousDataSet()
    {
        List<float> data = null;
        
        if (parameterDropdown.options.Count > 0)
        {
            string parameter = parameterDropdown.options[parameterDropdown.value].text;
            data = apiImporter.GetParameterData(parameter);
        }
        
        if (data != null && data.Count > maxVisibleDataPoints)
        {
            int maxOffset = data.Count - maxVisibleDataPoints;
            dataOffset += maxVisibleDataPoints / 2;
            if (dataOffset > maxOffset) dataOffset = maxOffset;
            
            // Refresh visualisasi
            RefreshVisualization();
        }
    }
    
    private void RefreshVisualization()
    {
        // Hapus chart sebelumnya
        ClearChart();
        
        // Visualisasikan berdasarkan mode
        if (showMultiSeries)
        {
            VisualizeAllParameters();
        }
        else
        {
            // Visualisasikan hanya parameter yang dipilih
            if (parameterDropdown.options.Count > 0)
            {
                string selectedParameter = parameterDropdown.options[parameterDropdown.value].text;
                VisualizeParameter(selectedParameter);
            }
        }
    }
}