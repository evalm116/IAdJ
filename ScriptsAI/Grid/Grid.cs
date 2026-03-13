using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
public class Grid : MonoBehaviour
{
    public int columnas;
    public int filas;
    public float cellSize;
    public Transform padreObstaculos;

    public GridCell[,] gridArray;

    [Header("Debug")]
    public PathfindingManager debugHeuristics;

    private void Awake()
    {
        gridArray = new GridCell[columnas, filas];
        float radioComprobacion = cellSize / 2.1f;


        // Calcular que celdas y si son transitables
        for (int x = 0; x < columnas; x++)
        {
            for (int z = 0; z < filas; z++)
            {
                // Crear celda en x,z
                // Por defecto transitable en constructor
                gridArray[x, z] = new GridCell(new Vector2Int(x, z)); 
                Vector3 centroCelda = GetCellCenter(x, z);

                // Obtenemos TODOS los colisionadores que tocan el centro de esta celda
                Collider[] collidersDetectados = Physics.OverlapSphere(centroCelda, radioComprobacion);

                // El colisionador solo bloquea si es del padreObstaculos
                foreach (Collider col in collidersDetectados)
                {                    
                    if (col.transform.parent != null && col.transform.parent == padreObstaculos)
                    {
                        gridArray[x, z].isWalkable = false;
                        break; // Como ya sabemos que está bloqueada, dejamos de comprobar el resto
                    }
                }
            }
        }
    }
    /// <summary>
    /// Obtener posición del centro de una celda dada su posición en el grid.
    /// </summary>
    /// <param name="x">Posición x en el grid</param>
    /// <param name="z">Posición z en el grid</param>
    /// <returns>Posición del centro de la celda en el mundo 3D</returns>
    public Vector3 GetCellCenter(int x, int z)
    {
        float xPos = transform.position.x + (x * cellSize) + (cellSize / 2);
        float zPos = transform.position.z + (z * cellSize) + (cellSize / 2);
        return new Vector3(xPos, transform.position.y, zPos);
    }

    /// <summary>
    /// Dada una posición en el mundo 3D, obtener la posición correspondiente en el grid (x,z).
    /// </summary>
    /// <param name="worldPosition">Posición en el mundo 3D.</param>
    /// <returns>Posición de celda que lo contiene en el grid.</returns>
    public Vector2Int GetGridPosition(Vector3 worldPosition)
    {
        int x = Mathf.FloorToInt((worldPosition.x - transform.position.x) / cellSize);
        int z = Mathf.FloorToInt((worldPosition.z - transform.position.z) / cellSize);
        return new Vector2Int(x, z);
    }


    //liderPosicion, posicion de la celda que se considera la posicion del lider
    //Dada una posicion (x,z) del grid retornar la posicion del mundo considerando la posicion del lider
    public Vector3 GetWorldPositionWithLeader(int x, int z, Vector2Int liderPosicion)
    {
        int offsetX = x - liderPosicion.x;
        int offsetZ = z - liderPosicion.y;

        float xPos = transform.position.x + (offsetX * cellSize) + (cellSize / 2);
        float zPos = transform.position.z + (offsetZ * cellSize) + (cellSize / 2);
        return new Vector3(xPos, transform.position.y, zPos);
    }

    
    /// <summary>
    /// Dada una celda, retorna una lista de las celdas vecinas transitables (arriba, abajo, izquierda, derecha).
    /// </summary>
    /// <param name="cell">Celda origen.</param>
    /// <returns>Celdas vecinas transitables de la celda de entrada.</returns>
    public List<GridCell> GetNeighbors(GridCell cell)
    {
        List<GridCell> neighbors = new List<GridCell>();
        int x = cell.gridPosition.x;
        int z = cell.gridPosition.y; // Ojo: en Vector2Int 'y' representa tu 'z' del mundo

        // Arriba
        if (z + 1 < filas && gridArray[x, z + 1].isWalkable) neighbors.Add(gridArray[x, z + 1]);
        // Abajo
        if (z - 1 >= 0 && gridArray[x, z - 1].isWalkable) neighbors.Add(gridArray[x, z - 1]);
        // Derecha
        if (x + 1 < columnas && gridArray[x + 1, z].isWalkable) neighbors.Add(gridArray[x + 1, z]);
        // Izquierda
        if (x - 1 >= 0 && gridArray[x - 1, z].isWalkable) neighbors.Add(gridArray[x - 1, z]);

        return neighbors;
    }
    private void OnDrawGizmos()
    {
        if (gridArray == null) return;

        for (int x = 0; x < columnas; x++)
        {
            for (int z = 0; z < filas; z++)
            {
                Gizmos.color = Color.gray;
                Vector3 cellCenter = GetCellCenter(x, z);
                Gizmos.DrawWireCube(cellCenter, new Vector3(cellSize, 0f, cellSize));

                // 
                /*Gizmos.color = gridArray != null && !gridArray[x, z].isWalkable ? Color.red : Color.green;
                Gizmos.DrawSphere(cellCenter, cellSize / 4);*/

                // Dibujamos una esfera roja si la celda no es transitable, 
                if (!gridArray[x, z].isWalkable)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawSphere(cellCenter, cellSize / 4);
                }
                else // Si es transitable dibujamos su heurística encima de la celda
                {
                    // #if necesario para evitar errores de Handles en builds, aunque no es necesario para Gizmos
                    if (debugHeuristics != null)
                    {
#if UNITY_EDITOR
                        int learned = (int)debugHeuristics.gridHeuristics[x, z];
                        string label = learned < 0f ? "-" : learned.ToString();

                        // Offset arriba para que no se solape con el suelo
                        Vector3 labelPos = cellCenter + Vector3.up * (cellSize * 0.02f);

                        GUIStyle style = new GUIStyle();
                        style.alignment = TextAnchor.MiddleCenter;
                        style.normal.textColor = Color.white;
                        style.fontSize = Mathf.Clamp((int)(cellSize * 6), 8, 16);

                        Handles.Label(labelPos, label, style);
#endif
                    }
                }
            }
        }
    }


    public bool PosicionValida(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < columnas && pos.y >= 0 && pos.y < filas;
    }
}
