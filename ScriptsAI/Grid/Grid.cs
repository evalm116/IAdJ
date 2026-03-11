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

    private void Awake()
    {
        gridArray = new GridCell[columnas, filas];
        float radioComprobacion = cellSize / 2.1f;

        for (int x = 0; x < columnas; x++)
        {
            for (int z = 0; z < filas; z++)
            {
                // 1. Creamos la celda por defecto transitable
                gridArray[x, z] = new GridCell(new Vector2Int(x, z));
                Vector3 centroCelda = GetCellCenter(x, z);

                // 2. Obtenemos TODOS los colisionadores que tocan el centro de esta celda
                Collider[] collidersDetectados = Physics.OverlapSphere(centroCelda, radioComprobacion);

                // 3. Revisamos uno a uno lo que hemos tocado
                foreach (Collider col in collidersDetectados)
                {
                    // Comprobamos si el objeto que hemos tocado es hijo del Empty "obstaculos"
                    // (También comprobamos que el padre no sea nulo para evitar errores)
                    if (col.transform.parent != null && col.transform.parent == padreObstaculos)
                    {
                        gridArray[x, z].isWalkable = false; // ¡Es un muro!
                        break; // Como ya sabemos que está bloqueada, dejamos de comprobar el resto
                    }
                }
            }
        }
    }
    public Vector3 GetCellCenter(int x, int z)
    {
        float xPos = transform.position.x + (x * cellSize) + (cellSize / 2);
        float zPos = transform.position.z + (z * cellSize) + (cellSize / 2);
        return new Vector3(xPos, transform.position.y, zPos);
    }

    //dada una posicion (i,j) del grid retornar la posicion del mundo
    public Vector3 GetWorldPosition(int x, int z)
    {
        float xPos = transform.position.x + (x * cellSize);
        float zPos = transform.position.z + (z * cellSize);
        return new Vector3(xPos, transform.position.y, zPos);
    }

    //dada una posicion del mundo retornar la posicion del grid
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

    // Método para obtener los vecinos de una celda (arriba, abajo, izquierda, derecha)
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

        for (int x = 0; x < columnas; x++)
        {
            for (int z = 0; z < filas; z++)
            {
                Gizmos.color = Color.gray;
                Vector3 cellCenter = GetCellCenter(x, z);
                Gizmos.DrawWireCube(cellCenter, new Vector3(cellSize, 0f, cellSize));
                /*Gizmos.color = gridArray != null && !gridArray[x, z].isWalkable ? Color.red : Color.green;
                Gizmos.DrawSphere(cellCenter, cellSize / 4);*/

                // If gridArray is not initialized (e.g. in edit-time before Awake) skip accessing it
                if (gridArray == null || x < 0 || x >= gridArray.GetLength(0) || z < 0 || z >= gridArray.GetLength(1))
                {
                    continue;
                }

                // For non-walkable cells keep the red sphere to indicate obstacles
                if (!gridArray[x, z].isWalkable)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawSphere(cellCenter, cellSize / 4);
                }
                else
                {
#if UNITY_EDITOR
                    // For walkable cells draw the learnedHeuristic value as a label above the cell
                    int learned = (int)gridArray[x, z].learnedHeuristic;
                    string label = learned < 0f ? "-" : learned.ToString();

                    // Small upward offset so the label sits above the cell
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

    internal void ResetHeuristics()
    {
        for (int x = 0; x < columnas; x++)
        {
            for (int z = 0; z < filas; z++)
            {
                gridArray[x, z].learnedHeuristic = -1.0f;
            }
        }
    }
}
