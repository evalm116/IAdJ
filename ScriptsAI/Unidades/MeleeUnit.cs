using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MeleeUnit : Unit
{

    protected override int AttackTarget(Unit target)
    {
        int damageDealt;
        Modifier modifier = Modifier.GetInstance();

        double attackPrecision = Math.Floor(UnityEngine.Random.Range(0.0f, 100.0f) * modifier.getPrecisionModifier(this, target)); // TODO: cambiar a calculo terreno

        if (attackPrecision >= 100)
        {
            damageDealt = (int)Math.Floor(this.attack * 1.5);
        }
        else if (attackPrecision >= NeededPrecision)
        {
            damageDealt = (int)Math.Floor(this.attack * modifier.getAttackModifier(this, target));

            if (this.magicAttack)
                damageDealt -= target.magicDefense;
            else
                damageDealt -= target.physicalDefense;

            if (damageDealt < 0)
            {
                Debug.Log($"El ataque de {this.type} ha sido resistido completamente por {target.type}.");
                damageDealt = -2; // Daño resistido
            }
        }
        else
        {
            Debug.Log($"El ataque de {this.type} ha sido esquivado por {target.type}.");
            damageDealt = -1; // fallo
        }

        _lastAttackTime = Time.time; // Actualiza el tiempo del último ataque
        return damageDealt;
    }
}
