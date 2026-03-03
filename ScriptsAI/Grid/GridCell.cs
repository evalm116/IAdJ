using UnityEngine;

[System.Serializable] // Fundamental para que Unity nos deje ver los datos en el Inspector si queremos depurar
public class GridCell
{
    public Vector2Int gridPosition; // Coordenadas (X, Z) de esta celda en la matriz
    public bool isOccupied;         // ¿Hay algún NPC asignado a esta casilla?
    public GameObject occupant;     // Si está ocupada, ¿quién es el NPC exacto que la ocupa?
    public float cost;              // Coste de movimiento (útil si más adelante metes zonas de barro o asfalto)

    // Constructor: Se ejecuta cuando el Grid crea esta celda por primera vez
    public GridCell(Vector2Int pos)
    {
        gridPosition = pos;
        isOccupied = false;
        occupant = null;
        cost = 1f; // Por defecto, el coste de pisar la celda es normal (1)
    }
}