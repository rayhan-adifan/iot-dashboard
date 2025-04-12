using UnityEngine;

public class LineRendererPrefabCreator : MonoBehaviour
{
    public Material defaultLineMaterial;
    public SensorDataVisualizer visualizer;
    
    void Awake()
    {
        // Referensi visualizer jika belum ditetapkan
        if (visualizer == null)
        {
            visualizer = GetComponent<SensorDataVisualizer>();
        }
        
        // Buat prefab LineRenderer saat runtime
        GameObject lineRendererPrefab = new GameObject("LineRendererPrefab");
        LineRenderer lineRenderer = lineRendererPrefab.AddComponent<LineRenderer>();
        
        // Konfigurasi LineRenderer
        lineRenderer.startWidth = 0.02f; // Kurangi width agar lebih sesuai
        lineRenderer.endWidth = 0.02f;
        lineRenderer.positionCount = 0;
        lineRenderer.useWorldSpace = false; // Benar, gunakan local space
        
        // Pastikan LineRenderer berada di layer UI
        lineRenderer.sortingLayerName = "UI";
        lineRenderer.sortingOrder = 5; // Atur order in layer lebih tinggi agar terlihat di atas background
        
        // Gunakan shader yang lebih cocok untuk UI
        if (defaultLineMaterial != null)
        {
            lineRenderer.material = defaultLineMaterial;
        }
        else
        {
            // Gunakan shader UI-Default atau UI/Default
            Material lineMaterial = new Material(Shader.Find("UI/Default"));
            if (lineMaterial == null)
                lineMaterial = new Material(Shader.Find("Sprites/Default"));
            
            lineMaterial.color = Color.white;
            lineRenderer.material = lineMaterial;
        }
        
        // Tambahkan Canvas Renderer jika berada dalam Canvas
        if (GetComponentInParent<Canvas>() != null)
        {
            lineRendererPrefab.AddComponent<CanvasRenderer>();
        }
        
        // Simpan sebagai prefab dan sembunyikan
        lineRendererPrefab.SetActive(false);
        DontDestroyOnLoad(lineRendererPrefab);
        
        // Tetapkan sebagai referensi ke SensorDataVisualizer
        if (visualizer != null)
        {
            visualizer.lineRendererPrefab = lineRendererPrefab;
        }
        else
        {
            Debug.LogError("SensorDataVisualizer tidak ditemukan!");
        }
    }
}