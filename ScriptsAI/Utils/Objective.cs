using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Objective : MonoBehaviour
{
    public List<Unit> unitsInObjective;
    public BANDO teamInControl = BANDO.None;
    public bool isContested;

    [Header("Debug")]
    public bool debug;
    private Collider col;

    private float conquestStartTime;
    private readonly float CONQUEST_TIME = 5f;

    private void Start()
    {
        unitsInObjective = new List<Unit>();
        col = GetComponent<Collider>();
        conquestStartTime = -1;
    }
        
    void OnTriggerEnter(Collider other)
    {
        Unit unit = other.gameObject.GetComponent<Unit>();

        // TODO: Eliminar cuando terminemos pruebas
        Debug.Log($"Unit {unit.type} has reached the objective! Team: {unit.teamID}");

        if (!unitsInObjective.Contains(unit))
        {
            unit.OnUnitDisabled += HandleUnitExit; // Acción en caso de muerte dentro del objetivo
            unitsInObjective.Add(unit);
            checkContested();
            OnObjectiveEntered?.Invoke(this);
        }
        else
        {
            Debug.LogWarning($"Unit {unit.type} and {unit.teamID} entered objective but was already registered as inside.");
        }        
    }

    /// <summary>
    /// Comprueba si un objetivo está siendo disputado o no. 
    /// Actualiza la variable isContested.
    /// </summary>
    private void checkContested()
    {        
        if (unitsInObjective.Count == 0)
        {
            isContested = false;
        }
        else
        {
            Unit first = unitsInObjective.First();
            isContested = unitsInObjective.Any(u => u.teamID != first.teamID);
            if (isContested) conquestStartTime = -1;
        }
    }

    void OnTriggerStay(Collider other)
    {
        Unit unit = other.gameObject.GetComponent<Unit>();

        // TODO: Eliminar cuando terminemos pruebas
        Debug.Log($"Unit {unit.type} and {unit.teamID} is inside obejctive");

        if (!isContested && conquestStartTime == -1f && unit.teamID != teamInControl)
        {
            conquestStartTime = Time.time;
        }
        else if (conquestStartTime > 0 && Time.time - conquestStartTime > CONQUEST_TIME)
        {
            teamInControl = unit.teamID;
            conquestStartTime = -1f;
            OnObjectiveCaptured?.Invoke(this);
        }
    }

    void OnTriggerExit(Collider other)
    {
        Unit unit = other.gameObject.GetComponent<Unit>();

        // TODO: Eliminar cuando terminemos pruebas
        Debug.Log($"Unit {unit.type} and {unit.teamID} left obejctive");
        HandleUnitExit(unit);
    }

    private void HandleUnitExit(Unit unit)
    {
        if (unit == null) return;

        if (unitsInObjective.Contains(unit))
        {
            unit.OnUnitDisabled -= HandleUnitExit; // Unsubscribe
            unitsInObjective.Remove(unit);
            checkContested();

            if (unitsInObjective.Count == 0 && conquestStartTime > 0)
                conquestStartTime = -1f;
        }
        else {
            Debug.LogWarning($"Unit {unit.type} and {unit.teamID} tried to leave objective but was not registered as inside.");
        }
    }

    private void OnDrawGizmos()
    {
        if (debug) DrawObjectiveGizmo(false);
    }

    private void OnDrawGizmosSelected()
    {
        if (debug) DrawObjectiveGizmo(true);
    }

    private void DrawObjectiveGizmo(bool selected = false)
    {
        if (col == null) col = GetComponent<Collider>();
        if (col == null) return;

        // 1. Set the color based on control state
        if (isContested )
        {
            Gizmos.color = Color.yellow;
        } else if (conquestStartTime > 0)
        {
            Gizmos.color = Color.magenta;
        }
        else
        {
            switch (teamInControl)
            {
                case BANDO.Red:
                    Gizmos.color = Color.red;
                    break;
                case BANDO.Blue:
                    Gizmos.color = Color.blue;
                    break;
                case BANDO.None:
                    Gizmos.color = Color.white;
                    break;
            }
        }

        // 2. Adjust transparency: solid wireframe, transparent fill
        Color fill = Gizmos.color;
        fill.a = selected ? 0.5f : 0.3f;
        Gizmos.color = fill;

        // 3. Match the Gizmo's matrix to the Transform
        // This handles position, rotation, and scale automatically
        Gizmos.matrix = transform.localToWorldMatrix;

        // 4. Draw the shape based on the collider type
        if (col is BoxCollider box)
        {
            Gizmos.DrawCube(box.center, box.size);
            Gizmos.DrawWireCube(box.center, box.size);
        }
        else if (col is SphereCollider sphere)
        {
            Gizmos.DrawSphere(sphere.center, sphere.radius);
            Gizmos.DrawWireSphere(sphere.center, sphere.radius);
        }
    }

    public bool IsUnitInside(Unit unit)
    {
        return unitsInObjective.Contains(unit);
    }

    internal Vector3 GetRandomPosition()
    {
        var a = new Vector3(
            UnityEngine.Random.Range(col.bounds.min.x, col.bounds.max.x),
            0,
            UnityEngine.Random.Range(col.bounds.min.z, col.bounds.max.z)
        );
        return a;
    }

    public List<Unit> UnitsOfTeam(BANDO teamID)
    {
        return unitsInObjective.Where(u => u.teamID == teamID).ToList();
    }

    public List<Unit> GetUnitsInObjective()
    {
        return new List<Unit>(unitsInObjective);
    }

    public int GetUnitCountOfTeam(BANDO teamID)
    {
        return unitsInObjective.Count(u => u.teamID == teamID);
    }

    public int GetEnemyCountOfTeam(BANDO teamID)
    {
        return unitsInObjective.Count(u => u.teamID != teamID);
    }

    public System.Action<Objective> OnObjectiveCaptured;
    public System.Action<Objective> OnObjectiveEntered;


}
