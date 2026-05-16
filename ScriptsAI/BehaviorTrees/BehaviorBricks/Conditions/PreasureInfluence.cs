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
    [Condition("IADJ/PreasureInfluence")]
    [Help("Calcula presión media a su alrededor y decide si ha pasado el umbral de presión o no")]
    public class PreasureInfluence : GOCondition
    {
        [InParam("Influence Preasure Max")]
        [Help("Radius of the influence check")]
        [Range(0, max: 100)]
        public float preasureMax;

        [InParam("Influence Check Radius")]
        [Help("Radius of the influence check")]
        public int influenceCheckRadius;
        public override bool Check()
        {

            Unit unit = gameObject.GetComponent<Unit>();
            if (unit == null) return false;

            GridCell currentCell = GameManager.Instance.GameGrid.GetCellAt(unit.GetPosition());
            if (currentCell == null) return false;

            var influenceArea = MyUtils.GetEspacioBusqueda(currentCell, influenceCheckRadius);
            var influences = GameManager.Instance.GetInfluenceArea(influenceArea);
            var a = influences.Average();
            if ((unit.teamID == BANDO.Red && influences.Average() > -1 * preasureMax) ||
                (unit.teamID == BANDO.Blue && influences.Average() < preasureMax))
            {
                return true;
            }

            return false;
        }
    }
}