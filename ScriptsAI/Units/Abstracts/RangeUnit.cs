using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class RangeUnit : Unit
{
    public GameObject projectilePrefab;
    protected override int AttackTarget(Unit target)
    {
        GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        // Projectile projComp = projectile.GetComponentInChildren<Projectile>();
        Projectile projComp = projectile.GetComponent<Projectile>();
        projComp.SetShooter(this);
        projComp.SetTarget(target);
        _lastAttackTime = Time.time; // Actualiza el tiempo del último ataque

        return 0; // El daño se aplica al impactar el proyectil
    }
        
    public override bool TryAttack(Unit target)
    {
        if (target == null || !CanAttack || !isInRange(target))
        {
            return false;
        }

        AttackTarget(target);

        return true;
    }
}
