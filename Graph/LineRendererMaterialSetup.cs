using UnityEngine;

public class LineRendererMaterialSetup : MonoBehaviour
{
    [SerializeField] private SensorDataVisualizer visualizer;
    
    // Array of line materials
    [SerializeField] private Material[] lineMaterials;
    
    void Start()
    {
        // Create materials at runtime if not provided
        if (lineMaterials == null || lineMaterials.Length == 0)
        {
            CreateLineMaterials();
        }
        
        // Assign materials to visualizer
        visualizer.lineMaterials = lineMaterials;
    }
    
    private void CreateLineMaterials()
    {
        // Create a set of materials with different colors
        lineMaterials = new Material[10];
        
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
        
        for (int i = 0; i < lineMaterials.Length; i++)
        {
            Material mat = new Material(Shader.Find("Sprites/Default"));
            mat.color = colors[i % colors.Length];
            lineMaterials[i] = mat;
        }
    }
}