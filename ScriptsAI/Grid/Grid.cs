using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    public int columnas;
    public int filas;
    public float cellSize;

    //public Vector2Int liderPosicion;

    public GridCell[,] gridArray; // Declaramos la matriz 2D usando tu nueva clase

    private void Awake()
    {
        // Inicializamos el tamaño de la matriz
        gridArray = new GridCell[columnas, filas];

        for (int x = 0; x < columnas; x++)
        {
            for (int z = 0; z < filas; z++)
            {
                // Rellenamos cada hueco con un nuevo objeto GridCell
                gridArray[x, z] = new GridCell(new Vector2Int(x, z));
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
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.gray;
        for (int x = 0; x < columnas; x++)
        {
            for (int z = 0; z < filas; z++)
            {
                Vector3 cellCenter = GetCellCenter(x, z);

                Gizmos.DrawWireCube(cellCenter, new Vector3(cellSize, 0f, cellSize));
            }
        }
    }
}
