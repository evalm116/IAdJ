using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Vector3 RespawnPoint;

    private List<Unit> deadUnits;
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
                unit.Respawn(RespawnPoint);
        }
    }

    public void RegisterDeadUnit(Unit unit)
    {
        deadUnits.Add(unit);
    }
}
