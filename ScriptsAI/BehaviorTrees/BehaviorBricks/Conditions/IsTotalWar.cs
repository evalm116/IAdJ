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
    [Condition("IADJ/IsTotalWar")]
    [Help("Calcula el mejor candidato a sanar, si no hay falla")]
    public class IsTotalWar : GOCondition
    {
        public override bool Check()
        {
            Unit unit = gameObject.GetComponent<Unit>();
            if (unit == null) return false;

            return GameManager.Instance.GetStrategy(unit) == TacticalAI.Strategy.TotalWar;
        }
    }
}
