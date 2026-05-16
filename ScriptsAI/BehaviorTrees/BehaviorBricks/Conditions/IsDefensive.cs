using BBUnity.Conditions;
using Pada1.BBCore;
using Pada1.BBCore.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using MyUtils = global::Utils;

namespace BBUnity.Conditions
{
    [Condition("IADJ/IsDefensive")]
    [Help("Devuelve si el estado de la IA tactica de la unidad actual es defensivo o no")]
    public class IsDefensive : GOCondition
    {
        public override bool Check()
        {
            Unit unit = gameObject.GetComponent<Unit>();
            if (unit == null) return false;

            return GameManager.Instance.GetStrategy(unit) == TacticalAI.Strategy.Defensive;
        }
    }
}