using Pada1.BBCore;
using Pada1.BBCore.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace BBUnity.Actions
{
    [Action("IADJ/GoToSafeZone")]
    [Help("Uses pathFinding to go to a specified point")]
    public class GetToSafeZone : GOAction
    {
        private PathFindingTactical pathFollowing;
        private Unit unit;
        private bool inizalizationError;

        public override void OnStart()
        {
            try
            {


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


                Unit closestCleric = GameManager.Instance.GetUnits(unit.teamID)
                    .Where(u => u.type == Unit.Type.Cleric)
                    .OrderBy(c => Vector3.Distance(unit.agent.Position, c.agent.Position))
                    .FirstOrDefault(c => !c.IsDead);

                Vector3 destination = (closestCleric != null) ? closestCleric.GetPosition() : GameManager.Instance.MyBase(unit.teamID);
                pathFollowing.ObjectivePosition = destination;
            }
            catch (Exception ex)
            {
                inizalizationError = true;
                Debug.LogError("Error inicializando GetToSafeZone en el BehaviorTree" + ex.ToString());
            }
        }

        public override TaskStatus OnUpdate()
        {
            return (inizalizationError) ? TaskStatus.FAILED : TaskStatus.COMPLETED;
        }
    }
}
