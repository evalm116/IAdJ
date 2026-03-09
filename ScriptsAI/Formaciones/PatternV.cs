using UnityEngine;
using System.Collections.Generic;

public class PatternV : FormationPattern
{
    public float separationX = 2f; // Separación lateral
    public float separationZ = 2f; // Separación hacia atrás

    public override Vector3 GetSlotLocation(int slotNumber)
    {
        if (slotNumber == 0) return Vector3.zero;

        // Separación lateral 
        float xPos = ((slotNumber + 1) / 2) * separationX;
        if (slotNumber % 2 == 0) xPos = -xPos;

        // Separación hacia atrás (cuanto mayor el slot, más atrás va)
        float zPos = -(((slotNumber + 1) / 2) * separationZ); 

        return new Vector3(xPos, 0, zPos);
    }

    public override Vector3 GetDriftOffset(List<SlotAssignment> slotAssignments)
    {
        if (slotAssignments.Count == 0) return Vector3.zero;

        Vector3 center = Vector3.zero;
        foreach (SlotAssignment slot in slotAssignments)
        {
            center += GetSlotLocation(slot.slotNumber);
        }
        return center / slotAssignments.Count;
    }
}