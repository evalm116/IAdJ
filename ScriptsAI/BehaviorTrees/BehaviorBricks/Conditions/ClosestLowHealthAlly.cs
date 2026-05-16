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
    [Condition("IADJ/ClosestLowHealthAlly")]
    [Help("Calcula el mejor candidato a sanar, si no hay falla")]
    public class ClosestLowHealthAlly : GOCondition
    {
        [OutParam("Next Patient")]
        [Help("Ally that the healer is going to approach next")]
        public Unit nextPatient;

        public override bool Check()
        {
            Unit unit = gameObject.GetComponent<Unit>();
            if (unit == null) return false;

            // Aliado más cercano con poca vida
            Unit bestCandidate = GameManager.Instance.GetUnits(unit.teamID)
                .Where(ally => ally.IsHealLow() && !ally.IsDead)
                .Where(ally => ally != unit)
                .OrderBy(ally => Vector3.Distance(ally.GetPosition(), unit.GetPosition()))
                .FirstOrDefault();

            // Aliado más cercano sin la vida completa, solo si no hay aliados con poca vida
            if (bestCandidate == null)
                bestCandidate = GameManager.Instance.GetUnits(unit.teamID)
                .Where(ally => !ally.IsFullHealth() && !ally.IsDead)
                .Where (ally => ally != unit)
                .OrderBy(ally => Vector3.Distance(ally.GetPosition(), unit.GetPosition()))
                .FirstOrDefault();

            if (bestCandidate == null) return false;
            
            nextPatient = bestCandidate;
            return true;
        }
    }
}
