using System.Collections.Generic;
using UnityEngine;

// estructura  para enviar 2 datos a la vez
public struct SlotTransform
{
    public Vector3 position;
    public float orientation; // Ángulo en grados
}

// Interfaz que debe tener toda formación
public abstract class FormationPattern 
{
    // Posición + Ángulo
    public abstract SlotTransform GetSlotLocation(int slotNumber);
    
    // Comprueba si caben más personajes
    public virtual bool SupportsSlots(int slotCount) { return true; }

    // Calcula el desplazamiento (centro de masas) para evitar derrapes
    public virtual Vector3 GetDriftOffset(List<SlotAssignment> slotAssignments)
    {
        return Vector3.zero;
    }
}