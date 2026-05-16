using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InfluenceMap : MonoBehaviour
{
    [Header("Referencias")]
    public Grid grid;

    [Header("Configuración")]
    [Range(1, 15)]
    public int influenceRadius = 4;
    public BANDO bando;

    [Range(0, max: 100)]
    public int decayPerSecond = 20;

    [Header("Visualización")]
    public bool showInfluenceGizmos = true;
    public Gradient influenceGradient; // Configúralo en Inspector: negro→rojo→amarillo

    [Header("Minimapa")]
    public RawImage minimapImage;

    // El mapa de influencia: valor [0..1] por celda
    public int[,] influenceValues;



    private Texture2D heatTexture;


    private Color[] colorMap;

    private void Update()
    {
        ApplyDecay();
        RecalculateInfluence();
        UpdateHeatTexture();
    }

    private void ApplyDecay()
    {
        if (influenceValues == null) return;

        int decayThisFrame = Mathf.RoundToInt(decayPerSecond * Time.deltaTime);
        if (decayThisFrame <= 0) return;

        for (int x = 0; x < grid.columnas; x++)
            for (int z = 0; z < grid.filas; z++)
                influenceValues[x, z] = Mathf.Max(0, influenceValues[x, z] - decayThisFrame);
    }

    public void RecalculateInfluence()
    {
        if (grid == null || grid.gridArray == null) return;

        if (influenceValues == null)
            influenceValues = new int[grid.columnas, grid.filas];

        foreach (Unit unit in FindObjectsByType<Unit>(FindObjectsSortMode.None))
        {
            if (unit.teamID != bando) continue;
            if (!unit.gameObject.activeSelf) continue;

            GridCell npcCell = grid.GetCellAt(unit.GetPosition());
            if (npcCell == null) continue;

            int cx = npcCell.gridPosition.x;
            int cz = npcCell.gridPosition.y;

            for (int x = cx - influenceRadius; x <= cx + influenceRadius; x++)
            {
                for (int z = cz - influenceRadius; z <= cz + influenceRadius; z++)
                {
                    if (!grid.PosicionValida(x, z)) continue;

                    int dist = Mathf.Abs(x - cx) + Mathf.Abs(z - cz);
                    if (dist > influenceRadius) continue;

                    int influence = Mathf.RoundToInt((1f - (float)dist / influenceRadius) * 100);

                    // Toma el máximo entre lo que ya había y la influencia nueva
                    if (influence > influenceValues[x, z])
                        influenceValues[x, z] = influence;
                }
            }
        }
    }

    /// <summary>
    /// Obtener la influencia normalizada [0..1] en una posición del mundo.
    /// </summary>
    public float GetInfluenceAt(Vector3 worldPosition)
    {
        Vector2Int pos = grid.GetGridPosition(worldPosition);
        if (!grid.PosicionValida(pos)) return 0f;
        return influenceValues[pos.x, pos.y] / 100f;
    }

    public void UpdateHeatTexture()
    {
        if (heatTexture == null)
        {
            heatTexture = new Texture2D(grid.columnas, grid.filas, TextureFormat.RGBA32, false);
            colorMap = new Color[grid.columnas * grid.filas]; // ← array reutilizable
        }

        heatTexture.filterMode = FilterMode.Point;

        for (int x = 0; x < grid.columnas; x++)
        {
            for (int z = 0; z < grid.filas; z++)
            {
                float v = influenceValues[x, z] / 100f;
                Color c = influenceGradient.Evaluate(v);
                c.a = v > 0f ? 1f : 0f; // opaco si hay influencia, transparente si no

                int invX = grid.columnas - 1 - x;
                int invZ = grid.filas - 1 - z;
                colorMap[invX + invZ * grid.columnas] = c;
            }
        }

        heatTexture.SetPixels(colorMap);
        heatTexture.Apply();

        if (minimapImage != null)
            minimapImage.texture = heatTexture;
    }

    private void OnDrawGizmos()
    {
        if (!showInfluenceGizmos || grid == null || influenceValues == null) return;

        for (int x = 0; x < grid.columnas; x++)
        {
            for (int z = 0; z < grid.filas; z++)
            {
                int rawVal = influenceValues[x, z];
                if (rawVal <= 0) continue;

                float val = rawVal / 100f; // normalizado entre 0-1

                Vector3 center = grid.GetCellCenter(x, z);

                Color col = influenceGradient != null
                    ? influenceGradient.Evaluate(val)
                    : new Color(1f, val * 0.5f, 0f, val * 0.8f);

                Gizmos.color = col;
                float size = grid.cellSize * val * 0.85f;
                Gizmos.DrawCube(center + Vector3.up * 0.05f, new Vector3(size, 0.02f, size));

#if UNITY_EDITOR
                GUIStyle style = new GUIStyle();
                style.alignment = TextAnchor.MiddleCenter;
                style.fontSize = Mathf.Clamp((int)(grid.cellSize * 5), 7, 13);
                style.normal.textColor = col;
                UnityEditor.Handles.Label(center + Vector3.up * 0.15f, rawVal.ToString(), style);
#endif
            }
        }
    }
}