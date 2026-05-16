using NUnit.Framework.Internal.Commands;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Vector3 RespawnPoint;
    public GameObject objectivesParent;
    public List<Unit> redUnits;
    public List<Unit> blueUnits;
    public Dictionary<BANDO, Unit> units;
    private List<Unit> deadUnits;

    private List<List<Objective>> _objectives;

    private TacticalAI tacticalAIBlue;
    private TacticalAI tacticalAIRed;

    public InfluenceMap RedInfluence;
    public InfluenceMap BlueInfluence;
    public InfluenceMapManager InfluenceManager;

    public static GameManager Instance { get; private set; }
    public Grid GameGrid { get; private set; }

    public List<List<Objective>> Objectives { get => _objectives; private set => _objectives = value; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }

        // Initialize the GameGrid
        GameGrid = GetComponent<Grid>();

            _objectives = GetGroupedComponents<Objective>(objectivesParent);

        // Aseguramos que las unidades son del bando que dicen ser
        foreach (Unit u in redUnits)
        {
            u.teamID = BANDO.Red;
            // TODO: Place Units at random in designated base
        }

        foreach (Unit u in blueUnits)
        {
            u.teamID = BANDO.Blue;
            // TODO: Place Units at random in designated base
        }

        tacticalAIBlue = gameObject.AddComponent<TacticalAI>();
        tacticalAIBlue.teamID = BANDO.Blue;
        tacticalAIBlue.Construct();
        tacticalAIBlue.CurrentStrategy = TacticalAI.Strategy.Defensive;
        tacticalAIBlue.currentAttackState = TacticalAI.AttackState.Gather;

        tacticalAIRed = gameObject.AddComponent<TacticalAI>();
        tacticalAIRed.teamID = BANDO.Red;
        tacticalAIRed.Construct();
        tacticalAIRed.CurrentStrategy = TacticalAI.Strategy.Offensive;
        tacticalAIRed.currentAttackState = TacticalAI.AttackState.Gather;

        SetUpInfluenceMaps();
    }

    private void SetUpInfluenceMaps()
    {
        var influences = gameObject.GetComponents<InfluenceMap>();
        foreach (InfluenceMap f in influences) {
            if (f.bando == BANDO.Blue) {
                BlueInfluence = f;
            } else if (f.bando == BANDO.Red)
            {
                RedInfluence = f;
            }
        }

        if (BlueInfluence == null || RedInfluence == null) {
            Debug.LogError("Mapas de influencia no configurados correctamente en GameManager");
            return;
        }

        var manager = gameObject.GetComponent<InfluenceMapManager>();
        if (manager == null) {
            Debug.LogError("Manager de mapas de influencia no configurado correctamente en GameManager");
            return;
        }
        manager.mapRed = RedInfluence;
        manager.mapBlue = BlueInfluence;

        InfluenceManager = manager;
    }

    public List<List<T>> GetGroupedComponents<T>(GameObject parent) where T : Component
    {
        List<List<T>> masterList = new List<List<T>>();
        foreach (Transform child in parent.transform)
        {
            T component = child.GetComponent<T>();

            // GameObject de un elemento
            if (component != null && child.childCount == 0)
            {
                masterList.Add(new List<T> { component });
            }
            else // GameObject Padre de varios elementos
            {
                // NOTE: Los hijos no se les crea listas multinivel,
                // todos los subniveles se juntan en uno.
                T[] results = child.GetComponentsInChildren<T>(true);
                
                if (results.Length > 0)
                {
                    masterList.Add(new List<T>(results));
                }
            }
            // Si no tiene el componente, ni hijos con el componente, se ignora
        }
        return masterList;
    }

    // Start is called before the first frame update
    void Start()
    {
        deadUnits = new List<Unit>();
    }   

    // Update is called once per frame
    void Update()
    {
        foreach (Unit unit in deadUnits)
        {
            if (unit.CanRespawn)
            {
                unit.Respawn(MyBase(unit.teamID));                
            }
        }
    }

    public void RegisterDeadUnit(Unit unit)
    {
        deadUnits.Add(unit);
    }
    
    /// <summary>
    /// Devuelve las unidades del bando especificado. Devuelve nulo si el bando no está
    /// </summary>
    /// <param name="teamID">Bando del que se quieren las unidades</param>
    /// <returns>Unidades del bando</returns>
    internal List<Unit> GetUnits(BANDO teamID)
    {
        if (teamID == BANDO.Red)
        {
            return redUnits;
        }else if (teamID == BANDO.Blue)
        {
            return blueUnits;
        }
        return null;
    }

    public List<Unit> GetEnemyUnits(BANDO teamID)
    {
        if (teamID == BANDO.Red)
            return GetUnits(BANDO.Blue);
        else if (teamID == BANDO.Blue)
            return GetUnits(BANDO.Red);

        return null;
    }

    public Vector3? GetUnitTarget(Unit unit)
    {
        if (unit.teamID == BANDO.Blue)
        {
            return tacticalAIBlue.GetUnitTarget(unit);
        }
        else if (unit.teamID == BANDO.Red)
        {
            return tacticalAIRed.GetUnitTarget(unit);
        }
        return null;
    }

    public Vector3 GetInitialPosition(Unit u)
    {
        BANDO bANDO = u.teamID;
        if (bANDO == BANDO.Red)
        {
            return Objectives.Last()[0].transform.position;
        }
        else if (bANDO == BANDO.Blue)
        {
            return Objectives[0][0].transform.position;
        }
        return Vector3.zero;
    }

    public TacticalAI.Strategy GetStrategy(Unit u)
    {
        TacticalAI a = (u.teamID == BANDO.Blue) ? tacticalAIBlue : tacticalAIRed;
        return a.CurrentStrategy;
    }

    public Vector3 MyBase (BANDO teamID)
    {
        if (teamID == BANDO.Blue)
        {
            return Objectives[0][0].transform.position;
        }
        else if (teamID == BANDO.Red)
        {
            return Objectives.Last()[0].transform.position;
        }
        return Vector3.zero;
    }

    internal List<float> GetInfluenceArea(List<GridCell> influenceArea)
    {
        var influences = new List<float>();
        var dominanceMap = InfluenceManager.DominanceMap;
        foreach (GridCell cell in influenceArea)
        {
            if (cell == null) continue;
            var value = dominanceMap[cell.gridPosition.x, cell.gridPosition.y];

            influences.Add(value);
        }
        return influences;
    }
}
