using UnityEngine;
using System.Collections.Generic;

public class PatternMediaLuna : FormationPattern
{
    // REQUISITO: Límite de unidades (No menos de 6 personajes en total)
    public int maxSlots = 6; // 1 Líder + 5 Subordinados

    public override bool SupportsSlots(int slotCount)
    {
        return slotCount <= maxSlots;
    }

    public override SlotTransform GetSlotLocation(int slotNumber)
    {
        SlotTransform slotInfo = new SlotTransform();

        // REQUISITO: Estructura NO uniforme y más de 3 orientaciones
        switch (slotNumber)
        {
            case 0: // El Líder (Centro)
                slotInfo.position = Vector3.zero;
                slotInfo.orientation = 0f;     // Mira al frente
                break;
            case 1: // Escolta Izquierda (Adelantado)
                slotInfo.position = new Vector3(-2f, 0, 1.5f);
                slotInfo.orientation = -45f;   // Mira en diagonal izquierda
                break;
            case 2: // Escolta Derecha (Adelantado)
                slotInfo.position = new Vector3(2f, 0, 1.5f);
                slotInfo.orientation = 45f;    // Mira en diagonal derecha
                break;
            case 3: // Flanco Izquierdo (Abierto)
                slotInfo.position = new Vector3(-3.5f, 0, -1f);
                slotInfo.orientation = -90f;   // Mira totalmente a la izquierda
                break;
            case 4: // Flanco Derecho (Abierto)
                slotInfo.position = new Vector3(3.5f, 0, -1f);
                slotInfo.orientation = 90f;    // Mira totalmente a la derecha
                break;
            case 5: // Retaguardia (Cerrando el arco por detrás)
                slotInfo.position = new Vector3(0, 0, -2.5f);
                slotInfo.orientation = 180f;   // Mira hacia atrás
                break;
            default:
                slotInfo.position = Vector3.zero;
                slotInfo.orientation = 0f;
                break;
        }

        return slotInfo;
    }

    public override Vector3 GetDriftOffset(List<SlotAssignment> slotAssignments)
    {
        if (slotAssignments.Count == 0) return Vector3.zero;

        Vector3 center = Vector3.zero;
        foreach (SlotAssignment slot in slotAssignments)
        {
            center += GetSlotLocation(slot.slotNumber).position; 
        }
        return center / slotAssignments.Count;
    }
}