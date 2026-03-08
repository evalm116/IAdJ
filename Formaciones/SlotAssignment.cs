using UnityEngine;

// Estructura para saber qué personaje está en qué hueco
[System.Serializable] //  la lista se vea en el Inspector de Unity
public struct SlotAssignment
{
    public AgentNPC character;
    public int slotNumber;
    public Agent dummyTarget;
}