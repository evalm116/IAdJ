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
    [Action("IADJ/GoToTacticalPoint")]
    [Help("Uses pathFinding to go to a specified point")]
    public class GoToTacticalPoint : GOAction
    {

        private PathFindingTactical pathFollowing;

        private Unit unit;
        private bool isDead;

        [InParam("Target Point")]
        [Help("The point that the unit will move to")]
        private Vector3 targetPoint;
        // Start is called before the first frame update

        TaskStatus startStatus = TaskStatus.RUNNING;
        public override void OnStart()
        {
            isDead = false;
            GameManager gameManager = GameManager.Instance;
            unit = gameObject.GetComponent<Unit>();
            pathFollowing = gameObject.GetComponent<PathFindingTactical>();                       
            
            Vector3? val = gameManager.GetUnitTarget(unit);
            if (!val.HasValue)
            {
                startStatus = TaskStatus.FAILED;
                return;
            }

            pathFollowing.Objective.position = val.Value;
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
            if (startStatus == TaskStatus.FAILED)
            {
                return TaskStatus.FAILED;
            }

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
