using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI; // For UI components
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro; // For TextMeshPro components

// Data structure classes for each sensor type
[System.Serializable]
public class DHT21Data
{
    public int id;
    public string hum;
    public string temp;
    public string updated_at;
}

[System.Serializable]
public class CCS811Data
{
    public int id;
    public float eco2;
    public float tvoc;
    public string updated_at;
}

[System.Serializable]
public class FlowrateData
{
    public int id;
    public float flowrate;
    public float totalVolume;
    public string updated_at;
}

[System.Serializable]
public class IrradianceData
{
    public int id;
    public float irradiance;
    public float power_irr;
    public string updated_at;
}

[System.Serializable]
public class PZEM017Data
{
    public int id;
    public float voltage;
    public float current;
    public float power;
    public float energy;
    public string updated_at;
}

[System.Serializable]
public class TDSData
{
    public int id;
    public float suhu;
    public float v_tds;
    public float tds;
    public float ec;
    public string updated_at;
}

[System.Serializable]
public class PHData
{
    public int id;
    public float v_ph;
    public float ph;
    public string updated_at;
}

// Generic response wrapper
[System.Serializable]
public class GenericResponse<T>
{
    public List<T> data;
}

public class API_Importer1 : MonoBehaviour
{
    [Header("API Settings")]
    public string baseApiUrl = "https://dash.generasienergi.my.id/dashboard-iot/api/";
    public string apiKey = "425870c7d25c78907c8dc2a02fe520";
    
    [Header("Sensor Selection")]
    public TMP_Dropdown sensorDropdown;
    public string currentSensor = "dht21";
    
    [Header("Time Settings")]
    public TMP_Dropdown yearDropdown;
    public TMP_Dropdown monthDropdown;
    public string currentTimeFrame = "2024-10";

    [Header("UI References")]
    public TMP_Text statusText;
    public Button refreshButton;
    
    // Dictionary to store all sensor data
    private Dictionary<string, List<object>> allSensorData = new Dictionary<string, List<object>>();
    
    // Dictionary to store display values for charts
    public Dictionary<string, List<float>> chartValues = new Dictionary<string, List<float>>();
    public List<string> timestamps = new List<string>();
    
    // Available sensors
    private readonly Dictionary<string, System.Type> sensorTypes = new Dictionary<string, System.Type>()
    {
        { "dht21", typeof(DHT21Data) },
        { "ccs811", typeof(CCS811Data) },
        { "flowrate", typeof(FlowrateData) },
        { "irradiance", typeof(IrradianceData) },
        { "pzem017", typeof(PZEM017Data) },
        { "tds", typeof(TDSData) },
        { "ph", typeof(PHData) }
    };

    public delegate void DataLoadedHandler(string sensorType);
    public event DataLoadedHandler OnDataLoaded;

    void Start()
    {
        InitializeUI();
        FetchCurrentSensorData();
    }
    
    void InitializeUI()
    {
        // Setup sensor dropdown
        if (sensorDropdown != null)
        {
            sensorDropdown.ClearOptions();
            List<string> options = new List<string>(sensorTypes.Keys);
            sensorDropdown.AddOptions(options);
            sensorDropdown.value = options.IndexOf(currentSensor);
            sensorDropdown.onValueChanged.AddListener(delegate { OnSensorChanged(); });
        }
        
        // Setup year dropdown
        if (yearDropdown != null)
        {
            yearDropdown.ClearOptions();
            List<string> years = new List<string>();
            int currentYear = DateTime.Now.Year;
            for (int i = currentYear - 2; i <= currentYear; i++)
            {
                years.Add(i.ToString());
            }
            yearDropdown.AddOptions(years);
            yearDropdown.value = years.IndexOf(currentTimeFrame.Split('-')[0]);
            yearDropdown.onValueChanged.AddListener(delegate { OnTimeFrameChanged(); });
        }
        
        // Setup month dropdown
        if (monthDropdown != null)
        {
            monthDropdown.ClearOptions();
            List<string> months = new List<string>();
            for (int i = 1; i <= 12; i++)
            {
                months.Add(i.ToString("00"));
            }
            monthDropdown.AddOptions(months);
            
            // Set to current month in the time frame
            string currentMonth = currentTimeFrame.Split('-')[1];
            monthDropdown.value = months.IndexOf(currentMonth);
            monthDropdown.onValueChanged.AddListener(delegate { OnTimeFrameChanged(); });
        }
        
        // Setup refresh button
        if (refreshButton != null)
        {
            refreshButton.onClick.AddListener(FetchCurrentSensorData);
        }
        
        // Initialize status text
        if (statusText != null)
        {
            statusText.text = "Ready to fetch data...";
        }
    }
    
    void OnSensorChanged()
    {
        if (sensorDropdown != null)
        {
            currentSensor = sensorDropdown.options[sensorDropdown.value].text;
            FetchCurrentSensorData();
        }
    }
    
    void OnTimeFrameChanged()
    {
        if (yearDropdown != null && monthDropdown != null)
        {
            string year = yearDropdown.options[yearDropdown.value].text;
            string month = monthDropdown.options[monthDropdown.value].text;
            currentTimeFrame = $"{year}-{month}";
            FetchCurrentSensorData();
        }
    }
    
    public void FetchCurrentSensorData()
    {
        StartCoroutine(GetDataFromAPI(currentSensor, currentTimeFrame));
    }

    IEnumerator GetDataFromAPI(string sensorType, string timeFrame)
    {
        string url = $"{baseApiUrl}{sensorType}/{apiKey}/{timeFrame}";
        
        if (statusText != null)
        {
            statusText.text = $"Fetching {sensorType} data...";
        }
        
        Debug.Log($"Fetching data from: {url}");

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.SetRequestHeader("Accept", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string json = request.downloadHandler.text;
                ProcessSensorData(sensorType, json);
            }
            else
            {
                Debug.LogError($"API Request Failed: {request.error}");
                if (statusText != null)
                {
                    statusText.text = $"Error: Failed to fetch {sensorType} data";
                }
            }
        }
    }
    
    private void ProcessSensorData(string sensorType, string json)
    {
        // Clear previous data
        allSensorData[sensorType] = new List<object>();
        chartValues.Clear();
        timestamps.Clear();
        
        try
        {
            switch (sensorType)
            {
                case "dht21":
                    ProcessDHT21Data(json);
                    break;
                case "ccs811":
                    ProcessCCS811Data(json);
                    break;
                case "flowrate":
                    ProcessFlowrateData(json);
                    break;
                case "irradiance":
                    ProcessIrradianceData(json);
                    break;
                case "pzem017":
                    ProcessPZEM017Data(json);
                    break;
                case "tds":
                    ProcessTDSData(json);
                    break;
                case "ph":
                    ProcessPHData(json);
                    break;
                default:
                    Debug.LogError($"Unknown sensor type: {sensorType}");
                    return;
            }
            
            int dataCount = timestamps.Count;
            if (statusText != null)
            {
                statusText.text = $"Loaded {dataCount} {sensorType} data points from {currentTimeFrame}";
            }
            
            // Notify any listeners that data has been loaded
            OnDataLoaded?.Invoke(sensorType);
            
        }
        catch (Exception e)
        {
            Debug.LogError($"Error processing {sensorType} data: {e.Message}");
            if (statusText != null)
            {
                statusText.text = $"Error processing {sensorType} data";
            }
        }
    }
    
    // Process methods for each sensor type
    private void ProcessDHT21Data(string json)
    {
        GenericResponse<DHT21Data> response = JsonUtility.FromJson<GenericResponse<DHT21Data>>(json);
        
        if (response != null && response.data != null)
        {
            // Initialize chart value lists
            chartValues["humidity"] = new List<float>();
            chartValues["temperature"] = new List<float>();
            
            foreach (DHT21Data entry in response.data)
            {
                allSensorData["dht21"].Add(entry);
                
                // Add display values for charts
                chartValues["humidity"].Add(float.Parse(entry.hum));
                chartValues["temperature"].Add(float.Parse(entry.temp));
                timestamps.Add(entry.updated_at);
            }
        }
    }
    
    private void ProcessCCS811Data(string json)
    {
        GenericResponse<CCS811Data> response = JsonUtility.FromJson<GenericResponse<CCS811Data>>(json);
        
        if (response != null && response.data != null)
        {
            // Initialize chart value lists
            chartValues["eco2"] = new List<float>();
            chartValues["tvoc"] = new List<float>();
            
            foreach (CCS811Data entry in response.data)
            {
                allSensorData["ccs811"].Add(entry);
                
                // Add display values for charts
                chartValues["eco2"].Add(entry.eco2);
                chartValues["tvoc"].Add(entry.tvoc);
                timestamps.Add(entry.updated_at);
            }
        }
    }
    
    private void ProcessFlowrateData(string json)
    {
        GenericResponse<FlowrateData> response = JsonUtility.FromJson<GenericResponse<FlowrateData>>(json);
        
        if (response != null && response.data != null)
        {
            // Initialize chart value lists
            chartValues["flowrate"] = new List<float>();
            chartValues["totalVolume"] = new List<float>();
            
            foreach (FlowrateData entry in response.data)
            {
                allSensorData["flowrate"].Add(entry);
                
                // Add display values for charts
                chartValues["flowrate"].Add(entry.flowrate);
                chartValues["totalVolume"].Add(entry.totalVolume);
                timestamps.Add(entry.updated_at);
            }
        }
    }
    
    private void ProcessIrradianceData(string json)
    {
        GenericResponse<IrradianceData> response = JsonUtility.FromJson<GenericResponse<IrradianceData>>(json);
        
        if (response != null && response.data != null)
        {
            // Initialize chart value lists
            chartValues["irradiance"] = new List<float>();
            chartValues["power_irr"] = new List<float>();
            
            foreach (IrradianceData entry in response.data)
            {
                allSensorData["irradiance"].Add(entry);
                
                // Add display values for charts
                chartValues["irradiance"].Add(entry.irradiance);
                chartValues["power_irr"].Add(entry.power_irr);
                timestamps.Add(entry.updated_at);
            }
        }
    }
    
    private void ProcessPZEM017Data(string json)
    {
        GenericResponse<PZEM017Data> response = JsonUtility.FromJson<GenericResponse<PZEM017Data>>(json);
        
        if (response != null && response.data != null)
        {
            // Initialize chart value lists
            chartValues["voltage"] = new List<float>();
            chartValues["current"] = new List<float>();
            chartValues["power"] = new List<float>();
            chartValues["energy"] = new List<float>();
            
            foreach (PZEM017Data entry in response.data)
            {
                allSensorData["pzem017"].Add(entry);
                
                // Add display values for charts
                chartValues["voltage"].Add(entry.voltage);
                chartValues["current"].Add(entry.current);
                chartValues["power"].Add(entry.power);
                chartValues["energy"].Add(entry.energy);
                timestamps.Add(entry.updated_at);
            }
        }
    }
    
    private void ProcessTDSData(string json)
    {
        GenericResponse<TDSData> response = JsonUtility.FromJson<GenericResponse<TDSData>>(json);
        
        if (response != null && response.data != null)
        {
            // Initialize chart value lists
            chartValues["suhu"] = new List<float>();
            chartValues["v_tds"] = new List<float>();
            chartValues["tds"] = new List<float>();
            chartValues["ec"] = new List<float>();
            
            foreach (TDSData entry in response.data)
            {
                allSensorData["tds"].Add(entry);
                
                // Add display values for charts
                chartValues["suhu"].Add(entry.suhu);
                chartValues["v_tds"].Add(entry.v_tds);
                chartValues["tds"].Add(entry.tds);
                chartValues["ec"].Add(entry.ec);
                timestamps.Add(entry.updated_at);
            }
        }
    }
    
    private void ProcessPHData(string json)
    {
        GenericResponse<PHData> response = JsonUtility.FromJson<GenericResponse<PHData>>(json);
        
        if (response != null && response.data != null)
        {
            // Initialize chart value lists
            chartValues["v_ph"] = new List<float>();
            chartValues["ph"] = new List<float>();
            
            foreach (PHData entry in response.data)
            {
                allSensorData["ph"].Add(entry);
                
                // Add display values for charts
                chartValues["v_ph"].Add(entry.v_ph);
                chartValues["ph"].Add(entry.ph);
                timestamps.Add(entry.updated_at);
            }
        }
    }
    
    // Helper method to get available parameters for the current sensor
    public List<string> GetCurrentSensorParameters()
    {
        return new List<string>(chartValues.Keys);
    }
    
    // Helper method to get the dataset for a specific parameter
    public List<float> GetParameterData(string parameter)
    {
        if (chartValues.ContainsKey(parameter))
        {
            return chartValues[parameter];
        }
        return new List<float>();
    }
}