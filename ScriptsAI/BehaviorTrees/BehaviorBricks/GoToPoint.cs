using Pada1.BBCore;
using Pada1.BBCore.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BBUnity.Actions
{
    /// <summary>
    /// It is an action to obtain a random position of an area.
    /// </summary>
    [Action("IADJ/GoToPoint")]
    [Help("Uses pathFinding to go to a specified point")]
    public class GoToPoint : GOAction
    {

        private PathFindingTactical pathFollowing;

        private Unit unit;
        private bool isDead;

        [InParam("Target Point")]
        [Help("The point that the unit will move to")]
        private Vector3 targetPoint;
        // Start is called before the first frame update
        public override void OnStart()
        {
            isDead = false;
            unit = gameObject.GetComponent<Unit>();
            pathFollowing = gameObject.GetComponent<PathFindingTactical>();
            pathFollowing.Objective.position = targetPoint;
            pathFollowing.SetUpObjective();

               unit.OnUnitDisabled += HandleUnitDeath;
        }

  

        // Se pone esta función por un error que me dio
        public void OnEnable()
        {
            OnStart();
        }

        // Update is called once per frame
        public override TaskStatus OnUpdate()
        {
            if (pathFollowing.Finished)
            {
                return TaskStatus.COMPLETED;
            }
            
            if (isDead)
            {
                return TaskStatus.COMPLETED;
            }

            return TaskStatus.RUNNING;
        }

        public void HandleUnitDeath(Unit unit)
        {
            isDead = true;
        }
    }
}
