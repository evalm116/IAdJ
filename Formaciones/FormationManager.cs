using System.Collections.Generic;
using UnityEngine;

public class FormationManager : MonoBehaviour
{
    // Lista de soldados y los huecos que ocupan
    public List<SlotAssignment> slotAssignments = new List<SlotAssignment>();
    
    // El desfase del centro de masas (para evitar derrapar)
    private Vector3 driftOffset;
    
    // El patrón que estamos usando (Línea, V, etc.)
    public FormationPattern pattern;

    // Referencia al mapa 
    public Grid gridManager;


    private void UpdateSlotAssignments()
    {
        for (int i = 0; i < slotAssignments.Count; i++)
        {

            SlotAssignment temp = slotAssignments[i];
            temp.slotNumber = i; 
            slotAssignments[i] = temp; 
        }

        // Actualizamos el drift
        if (pattern != null)
        {
            driftOffset = pattern.GetDriftOffset(slotAssignments);
        }
    }

    public bool AddCharacter(AgentNPC character)
    {
        int occupiedSlots = slotAssignments.Count;

        // Comprobamos si en esta formación cabe uno más
        if (pattern != null && pattern.SupportsSlots(occupiedSlots + 1))
        {
            // Creamos el nuevo assignment y lo metemos en la lista
            SlotAssignment newAssignment = new SlotAssignment();
            newAssignment.character = character;

            // gestión del dummy target
            GameObject tempObj = new GameObject("Dummy_Formacion_" + character.name);
            Agent dummy = tempObj.AddComponent<Agent>();
            newAssignment.dummyTarget = dummy;

            // Buscamos el Arrive del soldado, le asignamos el fantasma y lo encendemos para no crear y destruir componentes cada vez que entran o salen de la formación
            Arrive arriveScript = character.GetComponent<Arrive>();
            if (arriveScript != null) {
                arriveScript.target = dummy;
                arriveScript.enabled = true; 
            }

            slotAssignments.Add(newAssignment);
            
            // Recalculamos los números de hueco de todos
            UpdateSlotAssignments();
            return true;
        }
        return false; // No cabe o no hay patrón
    }


    public bool RemoveCharacter(AgentNPC character)
    {
        // 1. Buscamos en qué hueco está este personaje
        int indexToRemove = -1;     
        for (int i = 0; i < slotAssignments.Count; i++)
        {
            if (slotAssignments[i].character == character)
            {
                indexToRemove = i;
                break; 
            }
        }
        // si lo encontramos, lo quitamos y actualizamos los huecos de los demás
        if (indexToRemove != -1)
        {
            // gestion del Arrive y el dummy target al salir de la formación
            AgentNPC npcToLeave = slotAssignments[indexToRemove].character;
            Arrive arriveScript = npcToLeave.GetComponent<Arrive>();
            
            // Apagamos el Arrive y le quitamos el target
            if (arriveScript != null)
            {
                arriveScript.target = null;
                arriveScript.enabled = false; 
            }

            // Destruimos el fantasma para no dejar basura en la escena
            if (slotAssignments[indexToRemove].dummyTarget != null)
            {
                Destroy(slotAssignments[indexToRemove].dummyTarget.gameObject);
            }
            
            slotAssignments.RemoveAt(indexToRemove); 
            UpdateSlotAssignments(); 

            return true; 
        }
        return false; 
    }

    void Start()
    {
        pattern = new PatternLine(); // O PatternLine()

        // TRUCO TEMPORAL PARA PROBAR:
        // Asumiendo que tienes dos tanques en la escena llamados "Tanque1" y "Tanque2"
        AgentNPC t1 = GameObject.Find("Tanque1").GetComponent<AgentNPC>();
        AgentNPC t2 = GameObject.Find("Tanque2").GetComponent<AgentNPC>();
        AgentNPC t3 = GameObject.Find("Coche").GetComponent<AgentNPC>();
        AgentNPC t4 = GameObject.Find("Persona").GetComponent<AgentNPC>();


        if (t1 != null) AddCharacter(t1);
        if (t2 != null) AddCharacter(t2);
        if (t3 != null) AddCharacter(t3);
        if (t4 != null) AddCharacter(t4);
    }


    void Update()
    {
        // Si no hay patrón, no hacemos matemáticas
        if (pattern == null) return;

        // Recorremos cada soldado en la formación
        for (int i = 0; i < slotAssignments.Count; i++)
        {
            // 1. Pedimos la posición local al patrón (Ej: "2 metros a la derecha")
            Vector3 relativeLoc = pattern.GetSlotLocation(slotAssignments[i].slotNumber);

            // 2. Le restamos el driftOffset para compensar derrapes
            relativeLoc -= driftOffset;

            // 3. TransformPoint: Convierte esa coordenada local a mundo real usando
            //    la rotación y posición del Líder (este GameObject llamado "anchor")
            Vector3 targetPosition = transform.TransformPoint(relativeLoc);

            // 4. Movemos el fantasma (Dummy) a esa coordenada real
            if (slotAssignments[i].dummyTarget != null)
            {
                slotAssignments[i].dummyTarget.Position = targetPosition;
            }

            // CONEXIÓN CON EL GRID 
            if (gridManager != null)
            {
                // Usamos el método para saber qué casilla es (X, Z)
                Vector2Int posGrid = gridManager.GetGridPosition(targetPosition);

                //Comprobamos que la coordenada está dentro de los límites del mapa 
                if (posGrid.x >= 0 && posGrid.x < gridManager.columnas && 
                    posGrid.y >= 0 && posGrid.y < gridManager.filas)
                {
                    // Obtenemos esa celda exacta de la matriz
                    GridCell celda = gridManager.gridArray[posGrid.x, posGrid.y];
                    
                    // Asignar el NPC a la celda
                    celda.isOccupied = true;
                    celda.occupant = slotAssignments[i].character.gameObject;
                }
            }

        }
    }
}