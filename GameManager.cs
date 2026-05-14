using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public Grid GameGrid;  // Asignar en el Inspector
    public List<List<Objective>> Objectives;  // Asignar en el Inspector

    public Vector3 RespawnPointRed;
    public Vector3 RespawnPointBlue;

    private List<Unit> deadUnits;

    void Start()
    {
        deadUnits = new List<Unit>();
    }

    void Update()
    {
        foreach (Unit unit in deadUnits.ToList())
        {
            if (unit.CanRespawn)
            {
                Vector3 point = unit.teamID == BANDO.Red ? RespawnPointRed : RespawnPointBlue;
                unit.Respawn(point);
                deadUnits.Remove(unit);
            }
        }
    }

    public void RegisterDeadUnit(Unit unit)
    {
        if (!deadUnits.Contains(unit))
            deadUnits.Add(unit);
    }

    public List<Unit> GetUnits()
    {
        return new List<Unit>(FindObjectsByType<Unit>(FindObjectsSortMode.None));
    }

    public List<Unit> GetUnits(BANDO bando)
    {
        return GetUnits().Where(u => u.teamID == bando).ToList();
    }

    public Vector3? GetUnitTarget(Unit unit)
    {
        Unit closest = null;
        float minDist = float.MaxValue;
        foreach (Unit u in GetUnits())
        {
            if (u.teamID == unit.teamID || !u.gameObject.activeSelf) continue;
            float d = Vector3.Distance(unit.getPosition(), u.getPosition());
            if (d < minDist) { minDist = d; closest = u; }
        }
        return closest != null ? closest.getPosition() : (Vector3?)null;
    }

    public Vector3 GetInitialPosition(Unit unit)
    {
        if (unit == null) return Vector3.zero;
        return unit.teamID == BANDO.Red ? RespawnPointRed : RespawnPointBlue;
    }
}