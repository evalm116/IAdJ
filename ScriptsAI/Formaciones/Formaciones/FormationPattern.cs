using System.Collections.Generic;
using UnityEngine;


// Interfaz que debe tener toda formación
public abstract class FormationPattern 
{
    // Retorna la posición y orientación local de un hueco concreto
    public abstract Vector3 GetSlotLocation(int slotNumber);
    
    // Comprueba si caben más personajes
    public virtual bool SupportsSlots(int slotCount) { return true; }

    // Calcula el desplazamiento (centro de masas) para evitar derrapes
    public virtual Vector3 GetDriftOffset(List<SlotAssignment> slotAssignments)
    {
        return Vector3.zero;
    }
}