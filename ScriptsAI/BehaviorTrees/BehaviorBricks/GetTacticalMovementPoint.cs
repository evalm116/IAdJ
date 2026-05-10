using BBUnity.Conditions;
using Pada1.BBCore;
using Pada1.BBCore.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BBUnity.Conditions
{
    /// <summary>
    /// It is an action to obtain a random position of an area.
    /// </summary>
    [Condition("IADJ/GetTacticalMovementPoint")]
    [Help("Uses Tactical AI to determine a tactical movement point")]
    public class GetTacticalMovementPoint : GOCondition
    {

        [OutParam("Target Point")]
        [Help("The point that the unit will move to")]
        private Vector3 targetPoint;
        // Start is called before the first frame update
        public override bool Check()
        {
            GameManager gameManager = GameManager.Instance;
            Unit unit = gameObject.GetComponent<Unit>();
            Vector3? val = gameManager.GetUnitTarget(unit);
            if (val.HasValue)
            {
                targetPoint = val.Value;
                return true;
            }
                targetPoint = unit.getPosition();
            return false;
        }
    }
}