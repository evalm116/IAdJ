using Pada1.BBCore;
using Pada1.BBCore.Tasks;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace BBUnity.Actions
{
    /// <summary>
    /// It is an action to obtain a random position of an area.
    /// </summary>
    [Action("IADJ/Patrol")]
    [Help("Patrulla por puntos defensivos de su IA Tactica")]
    public class Patrol : GOAction
    {

        private PathFindingTactical pathFollowing;

        [InParam("PrevObjective")]
        [Help("Objetivo previo")]
        private Objective PrevObjective;
        // Start is called before the first frame update

        bool failedOnStart = false;
        public override void OnStart()
        {
            Unit unit;
            GameManager gameManager = GameManager.Instance;
            unit = gameObject.GetComponent<Unit>();
            pathFollowing = gameObject.GetComponent<PathFindingTactical>();

            if (pathFollowing == null)
            {
                pathFollowing = gameObject.AddComponent<PathFindingTactical>();
            }

            if (pathFollowing.PathManager == null)
            {
                pathFollowing.PathManager = gameObject.AddComponent<AStarTactical>();
            }

            Objective obj = GameManager.Instance.GetNextPatrolObjective(unit, PrevObjective);
            if (obj == null)
            {
                failedOnStart = true;
                return;
            }

            if (obj == PrevObjective)
                return;

            pathFollowing.ObjectivePosition = obj.transform.position;
            PrevObjective = obj;

            unit.agent.EmptySteeringList();
            unit.agent.addSteering(pathFollowing);
            failedOnStart = false;
        }

        // Update is called once per frame
        public override TaskStatus OnUpdate()
        {
            if (failedOnStart)
            {
                return TaskStatus.FAILED;
            }
            return TaskStatus.COMPLETED;
        }

    }
}
