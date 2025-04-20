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
    
    [Header("Chart Settings")]
    public float chartWidth = 800f;
    public float chartHeight = 400f;
    public float xPadding = 50f;
    public float yPadding = 50f;
    public int maxVisibleDataPoints = 50;
    
    [Header("UI Elements")]
    public RectTransform axisYLabels;
    public RectTransform axisXLabels;
    public TextMeshProUGUI chartTitleText;
    public TextMeshProUGUI minValueText;
    public TextMeshProUGUI maxValueText;
    public Toggle multiSeriesToggle;
    
    [Header("Line Renderers")]
    // Array LineRenderer yang dibuat secara manual di Editor
    public LineRenderer[] parameterLineRenderers;
    
    // Untuk nama parameter yang sesuai dengan LineRenderer
    public string[] parameterNames;
    
    private Dictionary<string, LineRenderer> lineRendererMap = new Dictionary<string, LineRenderer>();
    private Dictionary<string, Color> parameterColors = new Dictionary<string, Color>();
    
    // Untuk navigasi data
    private int dataOffset = 0;
    private bool showMultiSeries = false;
    
    private void Start()
    {
        // Pastikan LineRenderers dan parameterNames memiliki panjang yang sama
        if (parameterLineRenderers.Length != parameterNames.Length)
        {
            Debug.LogError("Jumlah LineRenderer dan parameterNames harus sama!");
            return;
        }
        
        // Buat mapping antara nama parameter dan LineRenderer
        for (int i = 0; i < parameterNames.Length; i++)
        {
            if (parameterLineRenderers[i] != null)
            {
                lineRendererMap[parameterNames[i]] = parameterLineRenderers[i];
                
                // Simpan warna asli LineRenderer
                parameterColors[parameterNames[i]] = parameterLineRenderers[i].startColor;
                
                // Sembunyikan semua LineRenderer di awal
                parameterLineRenderers[i].gameObject.SetActive(false);
            }
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
    }
    
    public void OnDataLoaded(string sensorType)
    {
        // Update dropdown parameter dengan parameter yang tersedia
        UpdateParameterDropdown();
        
        // Sembunyikan semua LineRenderer
        HideAllLineRenderers();
        
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
        
        // Gunakan hanya parameter yang memiliki LineRenderer yang sesuai
        List<string> availableParams = new List<string>();
        List<string> apiParams = apiImporter.GetCurrentSensorParameters();
        
        foreach (string param in apiParams)
        {
            if (lineRendererMap.ContainsKey(param))
            {
                availableParams.Add(param);
            }
        }
        
        if (availableParams.Count > 0)
        {
            parameterDropdown.AddOptions(availableParams);
            parameterDropdown.value = 0;
        }
    }
    
    private void OnParameterChanged(int index)
    {
        if (!showMultiSeries && index >= 0 && parameterDropdown.options.Count > 0)
        {
            string selectedParameter = parameterDropdown.options[index].text;
            
            // Sembunyikan semua LineRenderer
            HideAllLineRenderers();
            
            // Visualisasikan parameter yang dipilih
            VisualizeParameter(selectedParameter);
            
            // Update judul chart
            UpdateChartTitle();
        }
    }
    
    private void OnMultiSeriesToggleChanged(bool isOn)
    {
        showMultiSeries = isOn;
        
        // Sembunyikan semua LineRenderer
        HideAllLineRenderers();
        
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
        List<string> availableParams = new List<string>();
        
        // Filter parameter yang memiliki LineRenderer
        foreach (string param in parameters)
        {
            if (lineRendererMap.ContainsKey(param))
            {
                availableParams.Add(param);
            }
        }
        
        // Hitung min/max global untuk semua parameter
        float min = float.MaxValue;
        float max = float.MinValue;
        
        // Hitung rentang tampilan
        int dataPtsCount = 0;
        if (availableParams.Count > 0)
        {
            List<float> anyData = apiImporter.GetParameterData(availableParams[0]);
            if (anyData != null) dataPtsCount = anyData.Count;
        }
        
        int startIndex = Mathf.Max(0, dataPtsCount - maxVisibleDataPoints - dataOffset);
        int endIndex = Mathf.Min(dataPtsCount - 1 - dataOffset, startIndex + maxVisibleDataPoints);
        
        // Cari min/max global
        foreach (string param in availableParams)
        {
            List<float> paramData = apiImporter.GetParameterData(param);
            if (paramData != null && paramData.Count > 0)
            {
                for (int i = startIndex; i <= endIndex && i < paramData.Count; i++)
                {
                    min = Mathf.Min(min, paramData[i]);
                    max = Mathf.Max(max, paramData[i]);
                }
            }
        }
        
        // Tambahkan padding ke rentang
        float range = max - min;
        min -= range * 0.1f;
        max += range * 0.1f;
        
        // Update label axis
        List<string> timestamps = apiImporter.timestamps;
        UpdateAxisLabels(min, max, timestamps, startIndex, endIndex);
        
        // Visualisasikan setiap parameter
        foreach (string parameter in availableParams)
        {
            VisualizeParameterWithMinMax(parameter, min, max, startIndex, endIndex);
        }
    }
    
    private void VisualizeParameter(string parameter)
    {
        if (!lineRendererMap.ContainsKey(parameter))
        {
            Debug.LogWarning($"Tidak ada LineRenderer untuk parameter: {parameter}");
            return;
        }
        
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
        
        // Jika indeks valid, visualisasikan data
        if (startIndex <= endIndex && startIndex >= 0 && endIndex < data.Count)
        {
            // Hitung min/max untuk skala sumbu Y
            float min = float.MaxValue;
            float max = float.MinValue;
            
            // Cari min/max hanya untuk parameter ini
            for (int i = startIndex; i <= endIndex; i++)
            {
                min = Mathf.Min(min, data[i]);
                max = Mathf.Max(max, data[i]);
            }
            
            // Tambahkan padding ke rentang
            float range = max - min;
            min -= range * 0.1f;
            max += range * 0.1f;
            
            // Update label axis
            UpdateAxisLabels(min, max, timestamps, startIndex, endIndex);
            
            // Visualisasikan data dengan min/max yang dihitung
            VisualizeParameterWithMinMax(parameter, min, max, startIndex, endIndex);
        }
    }
    
    private void VisualizeParameterWithMinMax(string parameter, float min, float max, int startIndex, int endIndex)
    {
        // Ambil data untuk parameter ini
        List<float> data = apiImporter.GetParameterData(parameter);
        
        if (data == null || data.Count == 0 || !lineRendererMap.ContainsKey(parameter))
        {
            return;
        }
        
        // Dapatkan LineRenderer untuk parameter ini
        LineRenderer lineRenderer = lineRendererMap[parameter];
        
        // Aktifkan LineRenderer
        lineRenderer.gameObject.SetActive(true);
        
        // Setup LineRenderer
        lineRenderer.positionCount = endIndex - startIndex + 1;
        
        // Tambahkan titik ke LineRenderer
        for (int i = startIndex; i <= endIndex && i < data.Count; i++)
        {
            // Hitung posisi dalam ruang chart
            float xPos = xPadding + ((i - startIndex) / (float)(endIndex - startIndex)) * (chartWidth - 2 * xPadding);
            float yPos = yPadding + ((data[i] - min) / (max - min)) * (chartHeight - 2 * yPadding);
            
            // Tetapkan titik
            lineRenderer.SetPosition(i - startIndex, new Vector3(xPos, yPos, 0));
        }
    }
    
    private void UpdateAxisLabels(float minValue, float maxValue, List<string> timestamps, int startIndex, int endIndex)
    {
        // Update nilai min/max
        if (minValueText != null)
            minValueText.text = minValue.ToString("F2");
        
        if (maxValueText != null)
            maxValueText.text = maxValue.ToString("F2");
        
        // Update label sumbu Y
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
                label.fontSize = 10;
                label.alignment = TextAlignmentOptions.Center;
                
                // Posisikan label
                RectTransform rt = label.GetComponent<RectTransform>();
                rt.anchoredPosition = new Vector2(-10, yPadding + (chartHeight - 2 * yPadding) * (i / 5f));
            }
        }
        
        // Update label sumbu X
        if (axisXLabels != null)
        {
            // Hapus label yang ada
            foreach (Transform child in axisXLabels)
            {
                Destroy(child.gameObject);
            }
            
            // Buat label baru
            for (int i = 0; i <= 4; i++)
            {
                GameObject labelObj = new GameObject($"XLabel_{i}");
                labelObj.transform.SetParent(axisXLabels);
                
                TextMeshProUGUI label = labelObj.AddComponent<TextMeshProUGUI>();
                int index = startIndex + (int)((endIndex - startIndex) * (i / 4f));
                
                if (index < timestamps.Count)
                {
                    // Format timestamp
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
    }
    
    private void UpdateChartTitle()
    {
        if (chartTitleText != null)
        {
            string sensorType = apiImporter.currentSensor.ToUpper();
            string timeFrame = apiImporter.currentTimeFrame;
            
            if (showMultiSeries)
            {
                chartTitleText.text = $"{sensorType} data - All Parameters - {timeFrame}";
            }
            else if (parameterDropdown.options.Count > 0)
            {
                string parameter = parameterDropdown.options[parameterDropdown.value].text;
                chartTitleText.text = $"{sensorType} data - {parameter} - {timeFrame}";
            }
        }
    }
    
    private void HideAllLineRenderers()
    {
        // Sembunyikan semua LineRenderer
        foreach (var lineRenderer in parameterLineRenderers)
        {
            if (lineRenderer != null)
            {
                lineRenderer.gameObject.SetActive(false);
            }
        }
    }
    
    // Metode navigasi untuk scrolling data
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
        // Sembunyikan semua LineRenderer
        HideAllLineRenderers();
        
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