using UnityEngine;
using System.Collections.Generic;

public class PatternV : FormationPattern
{
    public float separationX = 1f; 
    public float separationZ = 1f; 
    
    // REQUISITO: Límite de unidades
    public int maxSlots = 6; // 1 líder + 5 soldados

    // REQUISITO: Bloquear si intentan entrar más del límite
    public override bool SupportsSlots(int slotCount)
    {
        return slotCount <= maxSlots;
    }

    public override SlotTransform GetSlotLocation(int slotNumber)
    {
        SlotTransform slotInfo = new SlotTransform();

        // 1. EL LÍDER (Hueco 0)
        if (slotNumber == 0) 
        {
            slotInfo.position = Vector3.zero;
            slotInfo.orientation = 0f; // Mira hacia el frente (0º)
            return slotInfo;
        }

        // --- MATEMÁTICAS DE LA POSICIÓN ---
        float xPos = ((slotNumber + 1) / 2) * separationX;
        if (slotNumber % 2 == 0) xPos = -xPos;

        float zPos = -(((slotNumber + 1) / 2) * separationZ); 
        slotInfo.position = new Vector3(xPos, 0, zPos);

        // --- MATEMÁTICAS DE LA ORIENTACIÓN (Mínimo 3 distintas) ---
        if (slotNumber == maxSlots - 1)
        {
            // El último soldado de la formación hace de retaguardia. Mira hacia atrás.
            slotInfo.orientation = 180f; 
        }
        else if (slotNumber % 2 != 0) 
        {
            // Los impares (Ala derecha): vigilan el flanco derecho
            slotInfo.orientation = 45f;
        }
        else 
        {
            // Los pares (Ala izquierda): vigilan el flanco izquierdo
            slotInfo.orientation = -45f;
        }

        return slotInfo;
    }

    public override Vector3 GetDriftOffset(List<SlotAssignment> slotAssignments)
    {
        if (slotAssignments.Count == 0) return Vector3.zero;

        Vector3 center = Vector3.zero;
        foreach (SlotAssignment slot in slotAssignments)
        {
            // Como ahora GetSlotLocation devuelve un SlotTransform, pedimos la .position
            center += GetSlotLocation(slot.slotNumber).position; 
        }
        return center / slotAssignments.Count;
    }
}