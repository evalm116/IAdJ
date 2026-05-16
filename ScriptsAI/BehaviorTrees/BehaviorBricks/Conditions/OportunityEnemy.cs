using BBUnity.Conditions;
using Pada1.BBCore;
using Pada1.BBCore.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BBUnity.Conditions
{
    [Condition("IADJ/OportunityEnemy")]
    [Help("Detects is an ene")]
    public class OportunityEnemy : GOCondition
    {

        [OutParam("Oportunity Enemy")]
        [Help("Unit within oportunity range")]
        public Unit oportunityEnemy;

        [InParam("Oportunity Distance")]
        [Help("Float with a close distance considered close enough to follow")]
        public float oportunityDistance;
        public override bool Check()
        {
            Unit unit = gameObject.GetComponent<Unit>();
            if (unit == null) return false;

            (float distance, Unit bestCandidate) = GameManager.Instance.GetEnemyUnits(unit.teamID)                
                .Where(enemy => enemy.IsHealLow() && !enemy.IsDead)
                .Select(enemy => (Vector3.Distance(enemy.GetPosition(), unit.GetPosition()), enemy))
                .OrderBy( pair => pair.Item1).FirstOrDefault();

            if (bestCandidate == null || distance < oportunityDistance) return false;
            
            oportunityEnemy = bestCandidate;
            return true;
        }
    }
}
