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
    [Condition("IADJ/IsUnitHealthLow")]
    [Help("Determines if the unit has low health or not")]
    public class IsUnitHealthLow : GOCondition
    {
        public override bool Check()
        {
            Unit u = gameObject.GetComponent<Unit>();
            if (u == null) {
                Debug.LogError("No se pudo encontrar la unidad asignada ");
                return false;
            }

            if (u.IsHealLow())
            {
                return true;
            }
            return false;
        }
    }
}