using UnityEngine;
using System.Collections.Generic;

public class PatternLine : FormationPattern
{
    public float separation = 2f; // Distancia entre soldados

    public override Vector3 GetSlotLocation(int slotNumber)
    {
        if (slotNumber == 0) return Vector3.zero; // El líder va en el centro

        // Calculamos la posición X
        float xPos = ((slotNumber + 1) / 2) * separation;
        
        // Si el número es par, lo ponemos a la izquierda (negativo)
        if (slotNumber % 2 == 0) xPos = -xPos; 

        return new Vector3(xPos, 0, 0); 
    }

    public override Vector3 GetDriftOffset(List<SlotAssignment> slotAssignments)
    {
        if (slotAssignments.Count == 0) return Vector3.zero;

        Vector3 center = Vector3.zero;
        foreach (SlotAssignment slot in slotAssignments)
        {
            center += GetSlotLocation(slot.slotNumber);
        }
        return center / slotAssignments.Count; // Media de las posiciones
    }
}