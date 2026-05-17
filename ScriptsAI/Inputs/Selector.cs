using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Selector : MonoBehaviour
{
    // Patrón Singleton para que el script 'PathFindingTactical' pueda saber 
    // desde cualquier sitio si una unidad está seleccionada o no.
    public static Selector Instance { get; private set; }

    [Header("Configuración de Selección")]
    [Tooltip("Color de debug para resaltar el material del personaje seleccionado.")]
    public Color selectedColor = Color.green;

    [Header("Filtros de Capas (Layers)")]
    private int groundLayer;
    private int unitLayerMask;

    // Lista dinámica para guardar las unidades que controlamos actualmente.
    private List<AgentNPC> selectedUnits = new List<AgentNPC>();

    // Diccionario para recordar el color original del monigote antes de pintarlo de verde.
    private Dictionary<AgentNPC, Color> originalColors = new Dictionary<AgentNPC, Color>();

    // Referencia al script Grid de tu proyecto para validar las casillas.
    private Grid gameGrid;

    private void Awake()
    {
        // Inicializamos el Singleton
        Instance = this;

        // ¿POR QUÉ HACEMOS ESTO?: Recuperamos las capas configuradas en Unity por código.
        // Evita que si se te olvida arrastrar la capa en el inspector, el script deje de funcionar.
        int groundLayerIndex = LayerMask.NameToLayer("GroundLayer");
        if (groundLayerIndex != -1) groundLayer = 1 << groundLayerIndex;
        else Debug.LogError("¡Falta la capa 'GroundLayer' en los ajustes de Unity!");

        int unitLayerIndex = LayerMask.NameToLayer("UnitLayerMask");
        if (unitLayerIndex != -1) unitLayerMask = 1 << unitLayerIndex;
        else Debug.LogError("¡Falta la capa 'UnitLayerMask' en los ajustes de Unity!");
    }

    private void Start()
    {
        // Buscamos el Grid en la escena. Lo necesitamos para consultar el mapa táctico.
        gameGrid = FindObjectOfType<Grid>();
        if (gameGrid == null)
        {
            Debug.LogError("[SELECTOR] No se ha encontrado el script Grid en la escena. ¡Es vital para validar el movimiento!");
        }
    }

    void Update()
    {
        // Clic Izquierdo: Selección de unidades
        if (Input.GetMouseButtonDown(0))
        {
            HandleSelection();
        }

        // Clic Derecho: Dar orden manual de movimiento táctico
        if (Input.GetMouseButtonDown(1))
        {
            HandleMovement();
        }
    }


    /// Gestiona la selección visual y lógica de las unidades mediante Raycast.
    private void HandleSelection()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, unitLayerMask))
        {
            AgentNPC unit = hit.collider.GetComponentInParent<AgentNPC>();

            if (unit != null)
            {
                // Si no mantenemos pulsado Shift Izquierdo, limpiamos la selección previa
                if (!Input.GetKey(KeyCode.LeftShift))
                {
                    DeselectAllUnits();
                }

                SelectUnit(unit);
                return;
            }
        }

        // Clic al vacío sin Shift: Deseleccionar todo
        if (!Input.GetKey(KeyCode.LeftShift))
        {
            DeselectAllUnits();
        }
    }

    /// Intercepta el clic derecho, valida el terreno y sobrescribe el comportamiento autónomo.
    private void HandleMovement()
    {
        // ¿POR QUÉ? Si no tienes a nadie seleccionado, el clic derecho no debe calcular nada (ahorro de rendimiento).
        if (selectedUnits.Count == 0) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, groundLayer))
        {
            Vector3 targetPos = hit.point;

            // 1. COMPROBACIÓN EXIGIDA: ¿El punto está dentro de los límites físicos del Grid?
            if (gameGrid.IsInside(targetPos))
            {
                // 2. COMPROBACIÓN EXIGIDA: Convertimos la posición 3D en celda lógica y miramos si es accesible
                GridCell cell = gameGrid.GetCellAt(targetPos);

                if (cell != null && cell.isWalkable)
                {
                    // Si la celda es transitable, mandamos a todas las unidades seleccionadas hacia allí
                    foreach (AgentNPC unit in selectedUnits)
                    {
                        PathFindingTactical pft = unit.GetComponent<PathFindingTactical>();
                        BehaviorExecutor behaviorTree = unit.GetComponent<BehaviorExecutor>();
                        if (pft != null)
                        {
                            pft.isUnderManualControl = true; // Encendemos el flag manual
                            pft.ObjectivePosition = targetPos; // Asignamos las coordenadas de destino
                            //pft.ForceRepath(); // Forzamos al A* Táctico a recalcular la ruta en este frame
                            unit.EmptySteeringList();
                            unit.addSteering(pft);
                            // Si el personaje tiene un árbol de IA activo,
                            // le quitamos las pilas temporalmente congelando su toma de decisiones.
                            if (behaviorTree != null)
                            {
                                behaviorTree.paused = true;
                                behaviorTree.enabled = false;
                            }
                        }
                    }
                }
                else
                {
                    // Si el usuario pincha en un muro, castillo o río infranqueable, lo ignoramos de forma segura
                    Debug.LogWarning("[SELECTOR] Movimiento denegado: La celda de destino es un obstáculo infranqueable.");
                }
            }
        }
    }

    private void SelectUnit(AgentNPC unit)
    {
        if (selectedUnits.Contains(unit)) return;

        selectedUnits.Add(unit);

        // Cambiamos el color de la malla del monigote para dar feedback visual al profesor
        Renderer rend = unit.GetComponentInChildren<Renderer>();
        if (rend != null)
        {
            if (!originalColors.ContainsKey(unit))
            {
                originalColors[unit] = rend.material.color;
            }
            rend.material.color = selectedColor;
        }
    }

    private void DeselectAllUnits()
    {
        foreach (AgentNPC unit in selectedUnits)
        {
            // 1. Restaurar el color original del monigote
            Renderer rend = unit.GetComponentInChildren<Renderer>();
            if (rend != null && originalColors.ContainsKey(unit))
            {
                rend.material.color = originalColors[unit];
            }


            // DEVOLVER A LA IA AL DESELECCIONAR
            PathFindingTactical pft = unit.GetComponent<PathFindingTactical>();
            BehaviorExecutor behaviorTree = unit.GetComponent<BehaviorExecutor>();

            if (pft != null)
            {
                pft.isUnderManualControl = false; // Apagamos el flag manual
            }

            if (behaviorTree != null)
            {
                // Le devolvemos las pilas al árbol de comportamiento inmediatamente
                behaviorTree.paused = false;
                behaviorTree.enabled = true;
            }
        }

        // Limpiamos las listas para la siguiente selección
        selectedUnits.Clear();
        originalColors.Clear();
    }

    // Método público auxiliar para que el OnDrawGizmos sepa si debe pintar el camino en VERDE o GRIS
    public bool IsUnitSelected(AgentNPC unit)
    {
        return selectedUnits.Contains(unit);
    }
}