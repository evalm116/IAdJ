using BBUnity.Conditions;
using Pada1.BBCore;
using Pada1.BBCore.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

using MyUtils = global::Utils;

namespace BBUnity.Conditions
{
    [Condition("IADJ/IsInMeleeChaseRange")]
    [Help("Calcula si un hay algún enemigo lo suficientemente cerca")]
    public class IsInMeleeChaseRange : GOCondition
    {
        [InParam("Chase Distance")]
        [Help("Float with a chase distance considered close enough to pursue")]
        public float chaseDistance;

        [OutParam("Chase Enemy")]
        [Help("Unit within chase range")]
        public Unit chaseEnemy;
        public override bool Check()
        {
            Unit unit = gameObject.GetComponent<Unit>();
            if (unit == null) return false;
            var enemigos = GameManager.Instance.GetEnemyUnits(unit.teamID)
               .Select(enemy => (Vector3.Distance(enemy.GetPosition(), unit.GetPosition()), enemy))
               .OrderBy(pair => pair.Item1).ToList();

            (float distance, Unit enemigo)  = enemigos.FirstOrDefault();            
            if (distance > chaseDistance) return false;
            
            chaseEnemy = enemigo;
            return true;
        }

    }
}
