using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UI;

public class InfluenceMapManager : MonoBehaviour
{
    [Header("Referencias")]
    public Grid grid;
    public InfluenceMap mapRed;
    public InfluenceMap mapBlue;

    [Header("Visualización dominancia")]
    public Gradient gradientRed;     // Color cuando domina Red
    public Gradient gradientBlue;    // Color cuando domina Blue

    [Header("Visualización tensión")]
    public Gradient gradientTension;

    [Header("Debug editor")]
    public bool showValuesInEditor = true;

    [Header("Minimapa")]
    public RawImage minimapDominance;
    public RawImage minimapTension;

    // Mapas derivados accesibles desde otros scripts
    public int[,] DominanceMap { get; private set; }
    public int[,] TensionMap { get; private set; }

    private Texture2D texDominance;
    private Texture2D texTension;
    private Color[] colorMapDominance;
    private Color[] colorMapTension;

    private void Awake()
    {
        
    }

    private void Update()
    {
        if (mapRed.influenceValues == null || mapBlue.influenceValues == null)
        {
            Debug.Log("influenceValues todavía null");
            return;
        }
        // Los InfluenceMap individuales ya se recalculan solos en su Update.
        // Aquí solo combinamos.
        CombineMaps();
        UpdateTextures();
    }

    private void CombineMaps()
    {
        if (grid == null || grid.gridArray == null) return;
        if (mapRed.influenceValues == null || mapBlue.influenceValues == null) return;

        int cols = grid.columnas;
        int rows = grid.filas;

        DominanceMap = new int[cols, rows];
        TensionMap = new int[cols, rows];

        for (int x = 0; x < cols; x++)
        {
            for (int z = 0; z < rows; z++)
            {
                int r = mapRed.influenceValues[x, z];
                int b = mapBlue.influenceValues[x, z];

                // Dominancia: [-1 .. 1]
                // -1 = dominio total Blue, +1 = dominio total Red
                DominanceMap[x, z] = r - b;

                // Tensión: [0 .. 1]
                // máxima tensión donde ambos bandos se solapan
                TensionMap[x, z] = r + b;
            }
        }
    }

    private void UpdateTextures()
    {
        UpdateDominanceTexture();
        UpdateTensionTexture();
    }

    private void UpdateDominanceTexture()
    {
        if (minimapDominance == null) return;

        if (texDominance == null)
        {
            texDominance = new Texture2D(grid.columnas, grid.filas, TextureFormat.RGBA32, false);
            texDominance.filterMode = FilterMode.Point;
            colorMapDominance = new Color[grid.columnas * grid.filas];
        }

        for (int x = 0; x < grid.columnas; x++)
        {
            for (int z = 0; z < grid.filas; z++)
            {
                float d = DominanceMap[x, z] / 100f; // [-1..1]
                Color c;

                if (d > 0)
                {
                    c = gradientRed.Evaluate(d);
                    c.a = 1f;
                }
                else if (d < 0)
                {
                    c = gradientBlue.Evaluate(-d);
                    c.a = 1f;
                }
                else
                {
                    c = Color.clear; // solo transparente en empate perfecto (d == 0 exacto)
                }

                int invX = grid.columnas - 1 - x;
                int invZ = grid.filas - 1 - z;
                colorMapDominance[invX + invZ * grid.columnas] = c;
            }
        }

        texDominance.SetPixels(colorMapDominance);
        texDominance.Apply();

        if (minimapDominance != null) minimapDominance.texture = texDominance;
    }

    private void UpdateTensionTexture()
    {
        if (minimapTension == null) return;

        if (texTension == null)
        {
            texTension = new Texture2D(grid.columnas, grid.filas, TextureFormat.RGBA32, false);
            texTension.filterMode = FilterMode.Point;
            colorMapTension = new Color[grid.columnas * grid.filas];
        }

        for (int x = 0; x < grid.columnas; x++)
        {
            for (int z = 0; z < grid.filas; z++)
            {
                float t = TensionMap[x, z] / 200f; // [0..1]
                Color c = gradientTension.Evaluate(t);
                c.a = t > 0f ? 1f : 0f;

                int invX = grid.columnas - 1 - x;
                int invZ = grid.filas - 1 - z;
                colorMapTension[invX + invZ * grid.columnas] = c;
            }
        }

        texTension.SetPixels(colorMapTension);
        texTension.Apply();

        if (minimapTension != null) minimapTension.texture = texTension;
    }

    // ── Gizmos con número encima de cada celda    
    private void OnDrawGizmos()
    {
        if (!showValuesInEditor) return;
        if (DominanceMap == null || TensionMap == null) return;

#if UNITY_EDITOR
        GUIStyle style = new GUIStyle();
        style.alignment = TextAnchor.MiddleCenter;
        style.fontSize = Mathf.Clamp((int)(grid.cellSize * 6), 7, 14);

        for (int x = 0; x < grid.columnas; x++)
        {
            for (int z = 0; z < grid.filas; z++)
            {
                int d = DominanceMap[x, z];
                int t = TensionMap[x, z];

                if (Mathf.Abs(d) < 1 && t < 1) continue;

                Vector3 center = grid.GetCellCenter(x, z) + Vector3.up * 0.1f;

                // Color del texto según dominancia
                style.normal.textColor = d > 0
                    ? Color.red
                    : d < 0 ? Color.cyan : Color.white;

                // Muestra dominancia arriba y tensión abajo
                string label = $"D:{d:+0;-0;0}\nT:{t}/200";
                UnityEditor.Handles.Label(center, label, style);
            }
        }
#endif
    }

    /// <summary>
    /// Consultar dominancia en una posición del mundo. [-1..1]
    /// </summary>
    public float GetDominanceAt(Vector3 worldPosition)
    {
        Vector2Int pos = grid.GetGridPosition(worldPosition);
        if (!grid.PosicionValida(pos)) return 0f;
        return DominanceMap[pos.x, pos.y];
    }

    /// <summary>
    /// Consultar tensión en una posición del mundo. [0..1]
    /// </summary>
    public float GetTensionAt(Vector3 worldPosition)
    {
        Vector2Int pos = grid.GetGridPosition(worldPosition);
        if (!grid.PosicionValida(pos)) return 0f;
        return TensionMap[pos.x, pos.y] / 200f;
    }
}