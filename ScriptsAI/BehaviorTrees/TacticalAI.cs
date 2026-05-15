using JetBrains.Annotations;
using NUnit.Framework.Constraints;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class TacticalAI : MonoBehaviour
{
    public enum Strategy
    {
        Defensive,
        Offensive,
        TotalWar
    }

    public enum AttackState
    {
        Gather,
        Attack,
        Retreat,
        DefendContested,
        Patrol
    }

    public readonly Dictionary<Strategy, float> attackPercentages = new Dictionary<Strategy, float>()
    {
         { Strategy.Defensive, 0.2f },
         { Strategy.Offensive, 0.5f },
         { Strategy.TotalWar, 1f }
    };


    public Strategy _currentStrategy;
    public Strategy CurrentStrategy
    {
        get { return _currentStrategy; }
        set
        {
            if (_currentStrategy != value)
            {
                _currentStrategy = value;
                UpdateOrders();
                UpdateGroups(); // Actualizamos los grupos cada vez que cambiamos de estrategia para ajustar el tamaño de los grupos de ataque y defensa
            }
        }
    }
    /*
     * public void ChangeBlueStrategy(Strategy s){
     *         tacticalAIBLUE.CurrentStrategy = s;
     * }
     * */

    public AttackState currentAttackState;

    private int _currentDepth = -1;

    public BANDO teamID;
    List<Unit> units = new List<Unit>();
    int availableUnits = 0;
    List<List<Objective>> objectives;

    int defendIndex = 0;
    int attackIndex = 0;

    Objective myBase;
    Objective enemyBase;

    private float lastUpdate;
    private readonly float UPDATE_INTERVAL = 4f; // Intervalo de actualización en segundos

    List<Objective> defendObjectives;
    List<Objective> attackObjectives;

    public Dictionary<Unit, Objective> ordenes; // NOTE: Unused, esto era una idea antigua

    public (Unit, Objective)[] attackGroup;
    public GameObject gatherAttackPoint; // Punto de reunión para el grupo de ataque antes de atacar
    public (Unit, Objective)[] defendGroup;

    private bool constructed = false;
    // Start is called before the first frame update
    public void Construct()
    {
        GameManager gm = GameManager.Instance;
        objectives = GameManager.Instance.Objectives;
        // Nos suscribimos al evento de captura de objetivo para actualizar el mapa táctico cada vez que se capture un objetivo)
        objectives.ForEach(list => list.ForEach(obj => obj.OnObjectiveCaptured += UpdateTacticalMapOnCaptureObjective));



        // Si eres Rojo, le "das la vuelta al tablero"
        if (teamID == BANDO.Red)
        {
            objectives = objectives.Reverse<List<Objective>>().ToList();
        }

        myBase = objectives.First<List<Objective>>().First<Objective>();
        enemyBase = objectives.Last<List<Objective>>().First<Objective>();

        units = gm.GetUnits(teamID);
        if (units == null)
        {
            Debug.LogError("Bando Mal especificado");
        }
        // Los sanadores van por libre
        availableUnits = units.Where(u => u.type != Unit.Type.Cleric).Count();

        // Crear objetivo dummy para el punto de reunión del grupo de ataque
        gatherAttackPoint = new GameObject("GatherAttackPoint");
        gatherAttackPoint.AddComponent<Objective>();
        gatherAttackPoint.AddComponent<BoxCollider>();
        gatherAttackPoint.GetComponent<BoxCollider>().isTrigger = true;
        gatherAttackPoint.GetComponent<BoxCollider>().size = new Vector3(5, 2, 5); // Tamaño del área de reunión
        gatherAttackPoint.transform.position = new Vector3(15, 1, 0); // NOTE: Posición temporal para pruebas.
        gatherAttackPoint.GetComponent<Objective>().debug = false;
        gatherAttackPoint.GetComponent<Objective>().OnObjectiveEntered += _ => ObjectiveUpdate();

        UpdateTacticalMap();
        lastUpdate = Time.time;

        UpdateGroups();
        constructed = true;
    }

    private void ObjectiveUpdate()
    {
        UpdateAttackGroupOrders();
    }

    private void Start()
    {
        if (!constructed) Construct();
    }


    // Update is called once per frame
    void Update()
    {
        // Limitamos ratio de actualización para no hacer cálculos innecesarios
        if (Time.time - lastUpdate >= UPDATE_INTERVAL)
        {
            bool flowControl = UpdateTacticalMap();
            if (!flowControl)
            {
                return;
            }
            flowControl = UpdateGroups();
            if (!flowControl) return;

            UpdateOrders();

            lastUpdate = Time.time;
        }
    }


    private bool UpdateTacticalMap()
    {
        // TOOD: Hacer para que no se creen nuevas listas cada vez, sino que se actualicen las mismas (para optimizar memoria)

        int depth = GetSecureTerritoryDepth();


        if (depth < 0 || depth >= objectives.Count)
        {
            Debug.LogError($"Error al obtener la profundidad del territorio seguro, profundidad invalida {depth}");
            return false;
        }

        if (depth == _currentDepth)
        {
            return true; // No ha cambiado la profundidad, no es necesario actualizar
        }

        _currentDepth = depth;
        if (depth < objectives.Count - 1 && objectives[depth + 1].Any(obj => obj.teamInControl == teamID))
        {
            defendIndex = depth;
            defendObjectives = new List<Objective>(objectives[defendIndex]);
            var range = objectives[depth + 1].Where(obj => obj.teamInControl == teamID);
            defendObjectives.AddRange(range);

        }
        else
        {
            defendIndex = depth;
            defendObjectives = new List<Objective>(objectives[defendIndex]);
        }
        attackIndex = depth + 1;
        attackObjectives = (attackIndex > objectives.Count) ? new List<Objective>() : new List<Objective>(objectives[attackIndex].Where(obj => obj.teamInControl != teamID));
        return true;
    }

    public void UpdateTacticalMapOnCaptureObjective(Objective objective)
    {
        if (defendObjectives.Contains(objective))
        {
            attackObjectives.Add(objective);
            defendObjectives.Remove(objective);
        }
        else if (attackObjectives.Contains(objective) && objective.teamInControl == teamID)
        {
            defendObjectives.Add(objective);
            attackObjectives.Remove(objective);
        }

        UpdateTacticalMap();
        UpdateOrders();
    }

    // NOTE: IDEA cada estrategia tiene unos grupos de ataque y defensa de diferentes tamaños. Actualizar cuando se cambia de estrategia y en el Start().
    // El grupo de defensa se esparce por los objetivos de defensa, y se puede concentrar en un objetivo que se esté siendo conquistado por el enemigo o esté en disputa.

    // El grupo de ataque se puede :
    // -    concentrar en un objetivo de ataque (el más cercano a la base enemiga, o el que tenga más unidades enemigas, o el que esté más cerca de ser conquistado, etc).
    // -    esparcirse por los objetivos de ataque
    // Asegurarse de que se reunen antes de atacar.

    // Los sanadores van por separado, persiguiendo a las unidades amigas que estén siendo atacadas o estén heridas



    /// <summary>
    /// Crea grupos de ataque y defensa dependiendo de la estrategia actual.
    /// </summary>
    /// <returns>Actualización realizada con éxito o no</returns>
    private bool UpdateGroups()
    {
        int attackGroupSize = Mathf.RoundToInt(availableUnits * attackPercentages[CurrentStrategy]);

        // Si los grupos ya tienen los tamaños designados no hay que actualizarlos
        if (attackGroup != null && attackGroupSize == attackGroup.Length &&
            defendGroup != null && defendGroup.Length == availableUnits - attackGroupSize)
            return true;


        (Unit, Objective)[] newAttackGroup = new (Unit, Objective)[attackGroupSize];
        (Unit, Objective)[] newDefendGroup = new (Unit, Objective)[availableUnits - attackGroupSize];

        if (attackGroup == null && defendGroup == null)
        {
            attackGroup = newAttackGroup;
            defendGroup = newDefendGroup;
            return true;
        }

        int diffAttack = attackGroupSize - attackGroup.Length;
        for (int i = 0; i < newAttackGroup.Length; i++)
        {
            if (i < attackGroup.Length)
            {
                newAttackGroup[i] = attackGroup[i];
            }
            else
            {
                newAttackGroup[i] = (null, null);
            }
        }

        for (int i = 0; i < newDefendGroup.Length; i++)
        {
            if (i < defendGroup.Length)
            {
                newDefendGroup[i] = defendGroup[i];
            }
            else
            {
                newDefendGroup[i] = (null, null);
            }
        }

        if (CurrentStrategy == Strategy.TotalWar)
        {
            int index = 0;
            foreach ((Unit u, Objective o) in attackGroup)
            {
                if (u != null)
                {
                    newAttackGroup[index] = (u, o);
                    index++;
                }
            }

            foreach ((Unit u, Objective o) in defendGroup)
            {
                if (u != null)
                {
                    newAttackGroup[index] = (u, o);
                    index++;
                }
            }
        }
        else if (diffAttack > 0)
        {
            for (int i = 0; i < diffAttack; i++)
            {
                // Pasamos unidades del grupo de defensa al de ataque
                if (defendGroup.Length - 1 - i >= 0 && defendGroup.Length - 1 - i < newDefendGroup.Length)
                {
                    newAttackGroup[attackGroupSize - 1 - i] = defendGroup[defendGroup.Length - 1 - i];
                    newDefendGroup[defendGroup.Length - 1 - i] = (null, null);
                }
            }
        }
        else if (diffAttack < 0)
        {
            for (int i = 0; i < -diffAttack; i++)
            {
                // Pasamos unidades del grupo de ataque al de defensa
                if (attackGroup.Length - 1 - i >= 0)
                {
                    newDefendGroup[newDefendGroup.Length - 1 - i] = attackGroup[attackGroup.Length - 1 - i];
                    newAttackGroup[attackGroup.Length - 1 - i] = (null, null);
                }
            }
        }

        attackGroup = newAttackGroup;
        defendGroup = newDefendGroup;

        return true;
    }

    /// <summary>
    /// Randomización de array utilizando Fisher-Yates shuffle.
    /// Fuente: https://stackoverflow.com/questions/108819/best-way-to-randomize-an-array-with-net
    /// </summary>
    /// <typeparam name="T">Cualquier tipo</typeparam>
    /// <param name="array">Array a mezclar</param>
    public static void Shuffle<T>(T[] array)
    {
        UnityEngine.Random.InitState(System.DateTime.Now.Millisecond);
        int n = array.Length;
        while (n > 1)
        {
            int k = UnityEngine.Random.Range(0, n--);
            T temp = array[n];
            array[n] = array[k];
            array[k] = temp;
        }
    }
    public bool hasOrders(Unit unit)
    {
        return (attackGroup.Any(pair => pair.Item1 == unit) || defendGroup.Any(pair => pair.Item1 == unit));
    }

    public Vector3 GetUnitTarget(Unit unit)
    {
        if (!units.Contains(unit))
        {
            Debug.LogError("Unidad no encontrada en el bando");
            return Vector3.zero;
        }

        // Si la unidad ya tiene órdenes asignadas, se las devolvemos
        Vector3? assignedTarget = FecthAssignedTarget(unit);
        if (assignedTarget.HasValue)
        {
            return assignedTarget.Value;
        }

        // Si no tiene órdenes asignadas, intentamos asignárselas
        if (AssignToGroup(unit))
        {
            UpdateOrders();
            Vector3? reassignedTarget = FecthAssignedTarget(unit);
            if (reassignedTarget.HasValue)
            {
                return reassignedTarget.Value;
            }
            else
            {
                Debug.LogError("Error al asignar órdenes a la unidad");
                return Vector3.zero;
            }
        }

        Debug.LogError("No se han podido asignar órdenes a la unidad, todos los grupos están llenos");
        return Vector3.zero;
    }

    private void UpdateOrders()
    {
        UpdateDefendGroupOrders();
        UpdateAttackGroupOrders();
    }

    /// <summary>
    /// Assigns defend group units to defend objectives, prioritizing units without valid assignments.
    /// Distributes units evenly across all defend objectives.
    /// </summary>
    private void UpdateDefendGroupOrders()
    {
        if (defendObjectives == null || defendObjectives.Count == 0) return;

        // Count actual non-null units in defend group
        List<(int index, Unit unit)> activeUnits = new List<(int, Unit)>();
        for (int i = 0; i < defendGroup.Length; i++)
        {
            if (defendGroup[i].Item1 != null)
            {
                activeUnits.Add((i, defendGroup[i].Item1));
            }
        }

        int unitCount = activeUnits.Count;
        if (unitCount == 0) return;

        // Create list for prioritization: units without objective or with invalid objective
        List<(int index, Unit unit)> priorityUnits = new List<(int, Unit)>();
        List<(int index, Unit unit)> otherUnits = new List<(int, Unit)>();

        foreach (var (index, unit) in activeUnits)
        {
            Objective currentObjective = defendGroup[index].Item2;
            if (currentObjective == null || !defendObjectives.Contains(currentObjective))
            {
                priorityUnits.Add((index, unit));
            }
            else
            {
                otherUnits.Add((index, unit));
            }
        }

        // Combine: priority units first, then others
        var unitsToAssign = new List<(int index, Unit unit)>(priorityUnits);
        unitsToAssign.AddRange(otherUnits);

        // Distribute units evenly across objectives

        // Conseguimos cuantas unidades hay en cada objetivo para intentar equilibrar


        BalanceGroup(defendObjectives, defendGroup, unitsToAssign);
    }

    public Objective currentAttackObjective;
    /// <summary>
    /// Assigns attack group units to attack objectives. The attack has three states;
    /// </summary>
    private void UpdateAttackGroupOrders()
    {
        bool flowControl = UpdateAttackState();
        if (!flowControl)
        {
            Debug.LogWarning("No se han podido actualizar las órdenes del grupo de ataque, probablemente porque no hay objetivos de ataque disponibles.");
            return;
        }


        flowControl = AttackStateAction();
        if (!flowControl)
        {
            Debug.LogWarning("No se han podido asignar órdenes al grupo de ataque, probablemente porque no hay objetivos de ataque disponibles.");
            return;
        }

        /*
                // NOTE: Código para pruebas
                foreach (var (unit, objective) in attackGroup)
                {
                    if (unit == null) continue;
                    // Si la unidad no tiene objetivo o su objetivo ya no es válido, le asignamos el punto de reunión para que se reúna con el grupo antes de atacar
                    if (objective == null)
                    {
                        for (int i = 0; i < attackGroup.Length; i++)
                        {
                            if (attackGroup[i].Item1 == unit)
                            {
                                attackGroup[i] = (unit, gatherAttackPoint.GetComponent<Objective>());
                                break;
                            }
                        }
                    }
                }*/
    }

    private bool UpdateAttackState()
    {
        try
        {
            switch (_currentStrategy)
            {
                case Strategy.Defensive:
                    // NOTE: la estrategia para defensiva estándar tiene que ser patrullar por los objetivos de defensa.
                    // Si el enemigo está conquistado una objetivo de defensa o está en disputa , el grupo de ataque se concentra en ese objetivo.
                    if (defendObjectives.Any(obj => obj.isContested || obj.GetEnemyCountOfTeam(teamID) > 0))
                        currentAttackState = AttackState.DefendContested;
                    else
                        currentAttackState = AttackState.Patrol;
                    break;
                case Strategy.Offensive:
                    // NOTE: la estrategia para ofensiva será primero organizarse en el punto de reunión.
                    // Cuando todas las unidades del grupo de ataque estén reunidas en el punto, se asigna un objetivo de ataque (el más cercano a la base) y se ataca.
                    List<Unit> unitsInGatherPoint = gatherAttackPoint.GetComponent<Objective>().UnitsOfTeam(teamID);
                    switch (currentAttackState)
                    {
                        case AttackState.Gather:
                            // Si el grupo de ataque no está reunido, se mantiene en Gather
                            if (attackGroup.All(pair => pair.Item1 != null && unitsInGatherPoint.Contains(pair.Item1)))
                            {
                                currentAttackState = AttackState.Attack; // Pasamos a atacar cuando todas las unidades del grupo de ataque se han reunido en el punto de reunión
                            }
                            break;
                        case AttackState.Attack:
                            // Si las al menos la mitad tienen poca vida. Las unidades muertas tienen poca vida.
                            if (attackGroup != null &&
                                (attackGroup.Select(pair => pair.Item1).Count(unit => unit != null && unit.IsHealLow()) > attackGroup.Length / 2))
                            {
                                currentAttackState = AttackState.Retreat; // Si el objetivo de ataque ya no es válido, volvemos a reunirnos para elegir un nuevo objetivo
                                                                          // TODO: recolocar gatherpoint a un objetivo de defensa cercano
                            }
                            // Si se conquista el objetivo de ataque
                            else if (currentAttackState == AttackState.Attack && currentAttackObjective != null && currentAttackObjective.teamInControl == teamID)
                            {
                                currentAttackState = AttackState.Gather; // Se vuelven a reunir 
                                                                         // TODO: recolocar gatherpoint
                            }
                            break;
                        case AttackState.Retreat:
                            // Si el grupo de ataque se está retirando, pero al menos la mitad de las unidades están en el punto de reunión, se vuelve a reunir para elegir un nuevo objetivo de ataque
                            if (currentAttackState == AttackState.Retreat && unitsInGatherPoint.Count >= attackGroup.Length / 2)
                            {
                                currentAttackState = AttackState.Gather; // Volvemos al estado de reunión cuando al menos la mitad de las unidades están en el punto de reunión
                                                                         // TODO: recolocar gatherpoint
                            }
                            break;
                        default:
                            currentAttackState = AttackState.Gather; // Por defecto, el grupo de ataque se reúne antes de atacar
                            break;
                    }
                    break;
                case Strategy.TotalWar:
                    // Si la estrategia es Total War, el grupo de ataque siempre está atacando, asignado a un objetivo de ataque válido (si no hay ninguno válido, se asigna el más cercano a la base)
                    if (attackObjectives == null || attackObjectives.Count == 0)
                    {
                        currentAttackState = AttackState.Gather; // Si no hay objetivos de ataque, el grupo de ataque se reúne para esperar a que haya objetivos disponibles
                                                                 //TODO: recolocar gatherpoint a un objetivo de defensa cercano
                    }
                    else if (currentAttackState != AttackState.Attack)
                        currentAttackState = AttackState.Attack; // En total war, el grupo de ataque siempre está atacando
                    break;
            }
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error al actualizar el estado del grupo de ataque: {ex.Message}");
            return false;
        }
    }

    protected bool AttackStateAction()
    {
        switch (currentAttackState)
        {
            case AttackState.Gather:
                // En el estado de Gather, todas las unidades del grupo de ataque se dirigen al punto de reunión para reunirse antes de atacar
                for (int i = 0; i < attackGroup.Length; i++)
                {
                    if (attackGroup[i].Item1 != null && attackGroup[i].Item2 != gatherAttackPoint.GetComponent<Objective>())
                    {
                        attackGroup[i].Item2 = gatherAttackPoint.GetComponent<Objective>();
                    }
                }
                break;
            case AttackState.Attack:
                // En el estado de Attack, asignamos a las unidades a los objetivos de ataque, priorizando las unidades sin objetivo o con objetivo no válido
                if (attackObjectives == null || attackObjectives.Count == 0) return false;
                if (currentAttackObjective == null || !attackObjectives.Contains(currentAttackObjective))
                {
                    // Si no hay objetivo de ataque asignado o el asignado ya no es válido, asignamos el más cercano a la base
                    var emptyObjectives = attackObjectives.Where(x => x.teamInControl == BANDO.None);
                    if (emptyObjectives.Count(_ => true) > 0)
                    {
                        currentAttackObjective = emptyObjectives.OrderBy(obj => Vector3.Distance(obj.transform.position, myBase.transform.position)).FirstOrDefault();
                    }
                    else
                    {
                        currentAttackObjective = attackObjectives.OrderBy(obj => Vector3.Distance(obj.transform.position, myBase.transform.position)).FirstOrDefault();
                    }
                }

                for (int i = 0; i < attackGroup.Length; i++)
                {
                    if (attackGroup[i].Item1 != null)//&& attackGroup[i].Item2 == gatherAttackPoint.GetComponent<Objective>())
                    {
                        attackGroup[i].Item2 = currentAttackObjective;
                    }
                }

                break;
            case AttackState.Retreat:
                // En el estado de Retreat, todas las unidades del grupo de ataque se dirigen al objetivo más cercano dentro del territorio defendido para retirarse y reagruparse
                // TODO: cambiar
                for (int i = 0; i < attackGroup.Length; i++)
                {
                    if (attackGroup[i].Item1 != null)
                    {
                        Unit unit = attackGroup[i].Item1;
                        Objective closestDefendObjective = defendObjectives.OrderBy(obj => Vector3.Distance(unit.agent.Position, obj.transform.position)).FirstOrDefault();
                        if (closestDefendObjective != null)
                        {
                            attackGroup[i] = (unit, closestDefendObjective);
                        }
                    }
                }
                break;
            case AttackState.DefendContested:
                // En el estado de DefendContested, las unidades del grupo de ataque se asignan a defender los objetivos en disputa o que estén siendo conquistados por el enemigo dentro del territorio atacado
                List<Objective> contestedObjectives = defendObjectives.Where(obj => obj.isContested || obj.GetEnemyCountOfTeam(teamID) > 0).OrderBy(obj => Vector3.Distance(obj.transform.position, myBase.transform.position)).ToList();
                for (int i = 0; i < attackGroup.Length; i++)
                {
                    if (attackGroup[i].Item1 != null)
                    {
                        Objective o = contestedObjectives.ElementAtOrDefault(i % contestedObjectives.Count); // Asignamos de forma cíclica a los objetivos en disputa para repartir las unidades entre ellos
                        if (o != null)
                        {
                            attackGroup[i].Item2 = o;
                        }
                    }
                }
                break;
            case AttackState.Patrol:
                // La unidad va a patrullar por los objetivos de defensa.
                for (int i = 0; i < attackGroup.Length; i++)
                {
                    // Si unidad no asignada continuamos
                    if (attackGroup[i].Item1 == null) continue;
                    // Asignamos primer objetivo si unidad no tiene objetivo asignado o si el
                    // objetivo no está en los objetivos de defensa.
                    if (attackGroup[i].Item2 == null || !defendObjectives.Contains(attackGroup[i].Item2))
                    {
                        attackGroup[i].Item2 = defendObjectives[0];
                    }
                    // Actualizamos si la unidad se encuentra dentro del punto
                    else if (attackGroup[i].Item2.GetUnitsInObjective().Contains(attackGroup[i].Item1))
                    {
                        int index = (defendObjectives.IndexOf(attackGroup[i].Item2) + 1) % defendObjectives.Count;
                        attackGroup[i].Item2 = defendObjectives[index];
                    }
                }
                break;
        }

        return true;
    }

    private void BalanceGroup(List<Objective> objectivesSubGroup, (Unit, Objective)[] unitGroup, List<(int index, Unit unit)> unitsToAssign = null)
    {
        List<int> unitsToAssign2 = new List<int>();

        List<int> objectiveUnitCounts = new List<int>(new int[objectivesSubGroup.Count]);
        for (int i = 0; i < unitGroup.Length; i++)
        {
            (Unit unit, Objective objective) = unitGroup[i];
            if (unit == null) continue;
            if (objective == null || !objectivesSubGroup.Contains(objective))
            {
                unitsToAssign2.Add(i);
                continue;
            }
            int objIndex = objectivesSubGroup.IndexOf(objective);
            if (objIndex >= 0)
            {
                objectiveUnitCounts[objIndex]++;
            }
        }

        // TODO: Eliminar estas units to Assign
        if (unitsToAssign2.Count > 0) // Si no se ha pasado lista no hay que asignar
        {
            foreach (var index in unitsToAssign2)
            {
                // Buscamos el objetivo con menos unidades asignadas
                int minCount = objectiveUnitCounts.Min();
                int objectiveIndex = objectiveUnitCounts.IndexOf(minCount);
                // Asignamos la unidad al objetivo
                unitGroup[index].Item2 = objectivesSubGroup[objectiveIndex];
                objectiveUnitCounts[objectiveIndex]++;
            }
        }

        int max = objectiveUnitCounts.Max();
        int min = objectiveUnitCounts.Min();
        // Consideramos que está equilibrado si la diferencia entre el objetivo con más unidades y el que menos tiene es de 1 o menos

        while ((max - min) > 1)
        {
            // Movemos la unidad más rápida del objetivo con más unidades al objetivo con menos unidades para intentar equilibrar
            int maxIndex = objectiveUnitCounts.IndexOf(max);
            int minIndex = objectiveUnitCounts.IndexOf(min);


            Unit fastest = objectivesSubGroup[maxIndex].UnitsOfTeam(teamID).OrderByDescending(u => u.agent.Speed).FirstOrDefault();
            if (fastest == null) break; // No hay unidades en el objetivo, no se puede balancear

            for (int i = 0; i < unitGroup.Length; i++)
            {
                if (unitGroup[i].Item1 == fastest)
                {
                    unitGroup[i] = (fastest, objectivesSubGroup[minIndex]);
                    objectiveUnitCounts[maxIndex]--;
                    objectiveUnitCounts[minIndex]++;
                    break;
                }
            }
            // Comprobamos si ya está balanceado
            max = objectiveUnitCounts.Max();
            min = objectiveUnitCounts.Min();
        }
    }

    private Vector3? FecthAssignedTarget(Unit unit)
    {
        Vector3 position;
        // TODO: Si la unidad está dentro del objetivo asignado, darle una posicióin aleatoria dentro del objetivo para simular wander.
        if (attackGroup.Any(pair => pair.Item1 == unit))
        {
            position = GetTargetPosition(unit, attackGroup);

            return position;
        }
        else if (defendGroup.Any(pair => pair.Item1 == unit))
        {
            position = GetTargetPosition(unit, defendGroup);
            return position;
        }

        return null;

        Vector3 GetTargetPosition(Unit unit, (Unit, Objective)[] group)
        {
            Grid grid = GameManager.Instance.GameGrid;
            Objective obj = group.First(pair => pair.Item1 == unit).Item2;
            //if (zoneCollider.bounds.Contains(targetRB.position))
            if (obj.IsUnitInside(unit))
            {
                int i = 0;
                do
                {
                    position = obj.GetRandomPosition();
                    i++;
                    if (i > 15)
                    {
                        Debug.LogError("No se encontró posición random válida del objetivo dentro del grid. Grid mayormente/completamente fuera del grid o muy mala suerte.");
                        return Vector3.zero;
                    }
                } while (!grid.IsInside(position) || !grid.GetCellAt(position).isWalkable);
            }                
            else
                position = obj.transform.position;

            position.y = unit.agent.Position.y; // Mantenemos la altura de la unidad para que no vuele
            return position;
        }
    }

    private bool AssignToGroup(Unit unit)
    {
        switch (CurrentStrategy)
        {
            case Strategy.Defensive:
                // Primero intentamos asignar al grupo de defensa
                if (AssignToGroup(unit, defendGroup)) return true;
                return AssignToGroup(unit, attackGroup); // Si el grupo de defensa está lleno, asignamos al grupo de ataque                
            case Strategy.Offensive:
                // Lo contrario que el anterior
                if (AssignToGroup(unit, attackGroup)) return true;
                return AssignToGroup(unit, defendGroup);
            case Strategy.TotalWar:
                return AssignToGroup(unit, attackGroup); // En total war, todas las unidades van al grupo de ataque
        }
        return false;
    }

    private bool AssignToGroup(Unit unit, (Unit, Objective)[] group)
    {
        for (int i = 0; i < group.Length; i++)
        {
            if (group[i].Item1 == null)
            {
                group[i] = (unit, null);
                return true;
            }
        }
        return false;
    }

    public int GetSecureTerritoryDepth()
    {
        int depth = -1;
        foreach (List<Objective> territory in objectives)
        {
            if (territory.All(obj => obj.teamInControl == teamID))
            {
                depth++;
            }
            else
            {
                break;
            }
        }
        return depth;
    }

}
