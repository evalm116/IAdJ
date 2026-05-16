using Pada1.BBCore;
using Pada1.BBCore.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BBUnity.Actions
{
    [Action("IADJ/PursueEnemyMelee")]
    [Help("Uses pathFinding to go to pursue enemy as if the unit is a melee unit")]
    public class PursueEnemyMelee : GOAction
    {
        [InParam("Oportunity Enemy")]
        [Help("Unit within oportunity range")]
        public Unit oportunityEnemy;

        private bool failedOnStart;
        public override void OnStart()
        {
            GameManager gameManager = GameManager.Instance;
            Unit unit = gameObject.GetComponent<Unit>();
            if (unit == null)
            {
                failedOnStart = true;
                return;
            }

            PathFindingTactical pathFollowing = gameObject.GetComponent<PathFindingTactical>();

            if (oportunityEnemy == null || pathFollowing == null)
            {
                failedOnStart = true;
                return;
            }

            Vector3 enemyPosition = oportunityEnemy.GetPosition();

            pathFollowing.Objective.position = enemyPosition;
            pathFollowing.SetUpObjective();

            if (!unit.agent.EmptySteeringList() || !unit.agent.addSteering(pathFollowing))
            {
                failedOnStart = true;
                return;
            }

            failedOnStart = false;
        }

        public override TaskStatus OnUpdate()
        {
            return (failedOnStart)? TaskStatus.FAILED : TaskStatus.COMPLETED;
        }
    }
}
