using UnityEngine;

public enum TipoTerreno
{
    Road,
    Plain,
    Desert,
    Forest,
    Mountain,
    RedBase,
    BlueBase,
    River
}

public class TerrenoInfo : MonoBehaviour
{
    [SerializeField] private TipoTerreno _terrainType;
    public TipoTerreno TerrainType => _terrainType;
}
