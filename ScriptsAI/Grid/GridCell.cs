using UnityEngine;

[System.Serializable]
public class GridCell
{
    public Vector2Int gridPosition;
    public bool isOccupied;
    public GameObject occupant;
    public float cost;
    public TipoTerreno terrainType;

    public bool isWalkable;


    // Constructor
    public GridCell(Vector2Int pos, TipoTerreno terreno)
    {
        gridPosition = pos;
        isOccupied = false;
        isWalkable = true; // Por defecto asumimos que el suelo está libre
        occupant = null;
        terrainType = terreno;
        cost = 1f;
    }
}