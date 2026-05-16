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
    [Condition("IADJ/AttackCloseEnemyDefensive")]
    [Help("Detects is an ene")]

    public class AttackCloseEnemyDefensive : GOCondition
    {
        [InParam("Chase Distance")]
        [Help("Float with a chase distance considered close enough to pursue")]
        public float chaseDistance;

        [OutParam("Chase Enemy")]
        [Help("Unit within chase range")]
        public Unit chaseEnemy;

        [InParam("Influence Check Radius")]
        [Help("Radius of the influence check")]
        public int influenceCheckRadius;

        [InParam("Influence Chase Average")]
        [Help("Radius of the influence check")]
        [Range(0, max: 100)]
        public float chaseAverage;

        public override bool Check()
        {
            Unit unit = gameObject.GetComponent<Unit>();
            if (unit == null) return false;

            var enemigos = GameManager.Instance.GetEnemyUnits(unit.teamID)
                .Select(enemy => (Vector3.Distance(enemy.GetPosition(), unit.GetPosition()), enemy))
                .OrderBy(pair => pair.Item1).ToList();

            foreach ((float distance, Unit enemigo) in enemigos)
            {
                if (distance > chaseDistance) return false;
                
                GridCell currentCell = GameManager.Instance.GameGrid.GetCellAt(enemigo.GetPosition());
                if (currentCell == null) continue;

                var influenceArea = MyUtils.GetEspacioBusqueda(currentCell, influenceCheckRadius);
                var influences = GameManager.Instance.GetInfluenceArea(influenceArea);

                if (influences == null) return false;

                // La misma condición pero cambia según si eres 
                var a = influences.Average();
                if ((unit.teamID == BANDO.Red && influences.Average() > -1 * chaseAverage) || 
                    (unit.teamID == BANDO.Blue && influences.Average() < chaseAverage))
                {
                    chaseEnemy = enemigo;
                    return true;
                }
            }

            return false;
        }

    }

}
