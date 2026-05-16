using Pada1.BBCore;
using Pada1.BBCore.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BBUnity.Actions
{
    [Action("IADJ/PursueEnemyRange")]
    [Help("Uses pathFinding to go to pursue enemy as if the unit is a melee unit")]
    public class PursueEnemyRange : GOAction
    {
        [InParam("Oportunity Enemy")]
        [Help("Unit within oportunity range")]
        public Unit oportunityEnemy;

        private bool succesfulstart = false;
        public override void OnStart()
        {
            GameManager gameManager = GameManager.Instance;
            Unit unit = gameObject.GetComponent<Unit>();
            if (unit == null)
            {
                succesfulstart = false;
                return;
            }

            PathFindingTactical pathFollowing = gameObject.GetComponent<PathFindingTactical>();

            if (oportunityEnemy == null || pathFollowing == null)
            {
                succesfulstart = false;
                return;
            }

            Vector3 enemyPosition = oportunityEnemy.GetPosition();

            pathFollowing.Objective.position = enemyPosition;
            pathFollowing.SetUpObjective();

          

            Flee flee = gameObject.GetComponent<Flee>();

            if (flee == null)
            {
                flee = gameObject.AddComponent<Flee>();                
            }

            unit.agent.InteriorRadius = (float)unit.range / 2f;
            unit.agent.ArrivalRadius = (float) unit.range - 0.1f;
            flee.target = oportunityEnemy.agent;
            flee.weight = 2;

            succesfulstart = (unit.agent.EmptySteeringList()
                && unit.agent.addSteering(pathFollowing)
                && unit.agent.addSteering(flee));
            
        }

        public override TaskStatus OnUpdate()
        {
            return (succesfulstart)? TaskStatus.COMPLETED : TaskStatus.FAILED;
        }
    }
}
