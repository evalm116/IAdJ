using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cleric : Unit
{
    // deprecated
    public override void autoAttack()
    {
        // Igual que el metodo original, pero solo "ataca" unidades del mismo equipo
        foreach (Unit unit in FindObjectsOfType<Unit>())
        {
            if (unit.gameObject != this.gameObject && unit.teamID == this.teamID && isInRange(unit) && unit.health < unit.maxHealth)
            {
                TryAttack(unit);
                return;
            }
        }
    }
    public override bool TryAttack(Unit target)
    {
        if (target == null || !CanAttack || !isInRange(target))
        {
            return false;
        }

        if (target.teamID != this.teamID || target.health >= target.maxHealth)
            return false; // No puede sanar unidades enemigas o unidades con salud completa

        // Se mantiene por no dejar AttackTarget vacio
        int healing = AttackTarget(target);
        target.GetHeal(healing);

        return true;
    }

    protected override int AttackTarget(Unit target)
    {
        _lastAttackTime = Time.time;
        // En el healer, el "ataque" es en realidad una sanaci�n        
        return this.attack;
    }

    protected override void CheckGround()
    {
        throw new System.NotImplementedException();
    }

    private void Awake()
    {
        this.type = Type.Cleric;
    }

}
