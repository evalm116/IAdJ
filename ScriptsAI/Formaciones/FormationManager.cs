using System.Collections.Generic;
using UnityEngine;

public class FormationManager : MonoBehaviour
{
    // --- VARIABLES DE LA FORMACIÓN ---
    public List<SlotAssignment> slotAssignments = new List<SlotAssignment>();
    private Vector3 driftOffset;
    public FormationPattern pattern;
    public LayerMask capaObstaculos; 
    public float distanciaOffsetPared = 1.5f; 

    [Header("Tiempos de Formación")]
    public float tiempoParaFormar = 0.8f; 
    public float tiempoParaRomper = 0.5f; 

    private float tiempoQuieto = 0f;
    private float tiempoMoviendose = 0f;
    private bool enFormacion = true; 

    // --- VARIABLES DEL LÍDER (WANDER Y MANUAL) ---
    [Header("Tiempos del Líder")]
    public float tiempoCaminandoWander = 5f; 
    public float tiempoParadoWander = 5f; 
    public float tiempoInactivoParaWander = 5f; 

    private float cronometroWander = 0f;
    private bool liderDePaseo = true;
    
    private bool modoManual = false; 
    private float cronometroManual = 0f;

    private AgentNPC agenteLider;
    private Wander wanderLider;
    private Arrive arriveLider;
    private Face faceLider;
    private Agent destinoOriginal;


    // =========================================================
    // FUNCIONES BÁSICAS DE GESTIÓN DE LA FORMACIÓN
    // =========================================================
    public bool IsInFormation(AgentNPC character)
    {
        foreach (SlotAssignment slot in slotAssignments)
        {
            if (slot.character == character) return true;
        }
        return false;
    }

    private void UpdateSlotAssignments()
    {
        for (int i = 0; i < slotAssignments.Count; i++)
        {
            SlotAssignment temp = slotAssignments[i];
            temp.slotNumber = i; 
            slotAssignments[i] = temp; 
        }

        if (pattern != null) driftOffset = pattern.GetDriftOffset(slotAssignments);
    }

    public bool AddCharacter(AgentNPC character)
    {
        int occupiedSlots = slotAssignments.Count;

        if (pattern != null && pattern.SupportsSlots(occupiedSlots + 1))
        {
            SlotAssignment newAssignment = new SlotAssignment();
            newAssignment.character = character;

            GameObject tempObj = new GameObject("Dummy_Formacion_" + character.name);
            Agent dummy = tempObj.AddComponent<Agent>();
            newAssignment.dummyTarget = dummy;

            Arrive arriveScript = character.GetComponent<Arrive>();
            if (arriveScript != null) { arriveScript.target = dummy; arriveScript.enabled = true; }
            
            Face faceScript = character.GetComponent<Face>();
            if (faceScript != null) { faceScript.target = dummy; faceScript.enabled = true; }

            foreach (Align a in character.GetComponents<Align>())
            {
                if (a.GetType() == typeof(Align)) { a.target = dummy; a.enabled = false; }
            }

            slotAssignments.Add(newAssignment);
            UpdateSlotAssignments();
            return true;
        }
        return false; 
    }

    public bool RemoveCharacter(AgentNPC character)
    {
        if (character == this.GetComponent<AgentNPC>()) return false;

        int indexToRemove = -1;     
        for (int i = 0; i < slotAssignments.Count; i++)
        {
            if (slotAssignments[i].character == character) { indexToRemove = i; break; }
        }
        
        if (indexToRemove != -1)
        {
            AgentNPC npcToLeave = slotAssignments[indexToRemove].character;
            Arrive arriveScript = npcToLeave.GetComponent<Arrive>();
            if (arriveScript != null) { arriveScript.target = null; arriveScript.enabled = false; }
            
            Face faceScript = npcToLeave.GetComponent<Face>();
            if (faceScript != null) { faceScript.target = null; faceScript.enabled = false; }

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


    // =========================================================
    // START Y UPDATE (LA LÓGICA DE MOVIMIENTO)
    // =========================================================
    void Start()
    {
        pattern = new PatternMediaLuna(); 

        agenteLider = GetComponent<AgentNPC>();
        arriveLider = GetComponent<Arrive>();

        // Filtramos exactamente qué script es Face puro y cuál es Wander
        Face[] todosLosFaces = GetComponents<Face>();
        foreach (Face f in todosLosFaces)
        {
            if (f.GetType() == typeof(Face)) faceLider = f;
            if (f.GetType() == typeof(Wander)) wanderLider = (Wander)f;
        }

        if (agenteLider != null)
        {
            SlotAssignment leaderSlot = new SlotAssignment();
            leaderSlot.character = agenteLider;
            leaderSlot.slotNumber = 0;
            slotAssignments.Add(leaderSlot);
        }

        // Guardamos el Destino original del clic
        if (arriveLider != null) destinoOriginal = arriveLider.target;

        // Empezamos forzando el modo Wander
        ApagarManual();
        if (wanderLider != null) wanderLider.isWandering = true;
    }

    void Update()
    {
        if (pattern == null) return;

        // --------------------------------------------------------
        // 1. INTERRUPCIÓN MANUAL (Clic Derecho)
        // --------------------------------------------------------
        if (Input.GetMouseButtonDown(1)) 
        {
            modoManual = true;
            cronometroManual = 0f; 
            
            // Apagamos el interruptor del Wander y activamos el ratón
            if (wanderLider != null) wanderLider.isWandering = false;
            ActivarManual();
        }

        // --------------------------------------------------------
        // 2. CONTROL DEL MOVIMIENTO DEL LÍDER
        // --------------------------------------------------------
        if (modoManual)
        {
            if (arriveLider != null && arriveLider.target != null)
            {
                float distanciaAlDestino = Vector3.Distance(transform.position, arriveLider.target.Position);

                // Si ha llegado a la marca del ratón
                if (distanciaAlDestino < 1.0f) 
                {
                    cronometroManual += Time.deltaTime;
                    
                    // Si pasan 5 segundos quieto... vuelve el Wander
                    if (cronometroManual >= tiempoInactivoParaWander)
                    {
                        modoManual = false;
                        liderDePaseo = true; 
                        cronometroWander = 0f;

                        ApagarManual();
                        if (wanderLider != null) wanderLider.isWandering = true;
                    }
                }
                else
                {
                    cronometroManual = 0f; 
                }
            }
        }
        else
        {
            // --- MODO DEMO AUTOMÁTICA (Alterna entre Wander 5s y Parado 5s) ---
            cronometroWander += Time.deltaTime;

            if (liderDePaseo && cronometroWander >= tiempoCaminandoWander)
            {
                // Toca pararse: Apagamos el interruptor del Wander
                if (wanderLider != null) wanderLider.isWandering = false;

                agenteLider.Velocity = Vector3.zero; 
                agenteLider.Rotation = 0f; 
                
                liderDePaseo = false;
                cronometroWander = 0f;
            }
            else if (!liderDePaseo && cronometroWander >= tiempoParadoWander)
            {
                // Toca caminar: Encendemos el interruptor
                if (wanderLider != null) wanderLider.isWandering = true;
                
                liderDePaseo = true;
                cronometroWander = 0f;
            }
        }

        // --------------------------------------------------------
        // 3. LÓGICA DE LA FORMACIÓN (Los soldados le siguen)
        // --------------------------------------------------------
        float velocidadLider = agenteLider.Velocity.magnitude;

        if (velocidadLider > 0.1f)
        {
            tiempoQuieto = 0f; 
            tiempoMoviendose += Time.deltaTime; 

            if (tiempoMoviendose >= tiempoParaRomper) enFormacion = false; 
        }
        else
        {
            tiempoMoviendose = 0f; 
            tiempoQuieto += Time.deltaTime;

            if (tiempoQuieto >= tiempoParaFormar) enFormacion = true; 
        }

        if (enFormacion)
        {
            for (int i = 0; i < slotAssignments.Count; i++)
            {
                if (slotAssignments[i].dummyTarget == null) continue;

                SlotTransform slotData = pattern.GetSlotLocation(slotAssignments[i].slotNumber);
                Vector3 relativeLoc = slotData.position;
                
                float orientacionFinal = slotData.orientation + transform.eulerAngles.y;
                slotAssignments[i].dummyTarget.Orientation = orientacionFinal;
                slotAssignments[i].dummyTarget.transform.rotation = Quaternion.Euler(0, orientacionFinal, 0);

                Vector3 targetPosition = transform.TransformPoint(relativeLoc);
                Vector3 direccionAlTarget = targetPosition - transform.position;
                float distanciaAlTarget = direccionAlTarget.magnitude;

                if (Physics.Raycast(transform.position, direccionAlTarget.normalized, out RaycastHit hit, distanciaAlTarget, capaObstaculos))
                {
                    targetPosition = hit.point + (hit.normal * distanciaOffsetPared);
                }

                slotAssignments[i].dummyTarget.Position = targetPosition;
                slotAssignments[i].dummyTarget.transform.position = targetPosition;
            }
        }
        else
        {
            Vector3 puntoPeloton = transform.position - (transform.forward * 2f);

            for (int i = 0; i < slotAssignments.Count; i++)
            {
                if (slotAssignments[i].dummyTarget == null) continue;

                slotAssignments[i].dummyTarget.Position = puntoPeloton;
                slotAssignments[i].dummyTarget.transform.position = puntoPeloton;
                slotAssignments[i].dummyTarget.Orientation = transform.eulerAngles.y;
            }
        }
    }

    // =========================================================
    // FUNCIONES DE CONTROL MANUAL
    // =========================================================
    private void ActivarManual()
    {
        // Les damos el "Destino" y les damos fuerza (Weight 1)
        if (arriveLider != null) { arriveLider.enabled = true; arriveLider.weight = 1f; arriveLider.target = destinoOriginal; }
        if (faceLider != null) { faceLider.enabled = true; faceLider.weight = 1f; faceLider.target = destinoOriginal; }
    }

    private void ApagarManual()
    {
        // Les robamos el Target y los matamos a Weight 0
        if (arriveLider != null) { arriveLider.enabled = false; arriveLider.weight = 0f; arriveLider.target = null; }
        if (faceLider != null) { faceLider.enabled = false; faceLider.weight = 0f; faceLider.target = null; }
    }
}