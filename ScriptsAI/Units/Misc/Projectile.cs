using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class Projectile : AgentNPC
{
    public Unit shooter;
    public Unit target;

    private Seek _seekBehaviour;
    private Face _faceBehaviour;
    private bool _steeringsAdded = false;

    private readonly float speed = 10f;
    private readonly float acceleration = 5000f;

    protected override void Start()
    {
        base.Start();
        this.MaxSpeed = this.speed;
        this.MaxAcceleration = this.acceleration;
        this.MaxRotation = this.acceleration;
        this.MaxAngularAcc = this.acceleration;
        AddSteeringBehaviours();

        var direction = target.agent.Position - shooter.agent.Position;
        if (direction.magnitude != 0)
            this.Orientation = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
    }

    // Update is called once per frame
    public  override void Update()
    {
        base.Update();
        // Si el target se ha asignado después del Start, instamos las steering behaviours aquí
        // AddSteeringBehaviours();
        // Comprobamos impacto con el target
        if (target != null && Vector3.Distance(this.transform.position, target.transform.position) < 0.5f)
        {
            int damage = AttackTarget();
            target.TakeDamage(damage);
            Destroy(this.gameObject); // Destruye el proyectil al impactar
        }
    }

    public void SetTarget(Unit targetUnit)
    {
        target = targetUnit;
        AddSteeringBehaviours();
    }

    public void SetShooter(Unit shooterUnit)
    {
        shooter = shooterUnit;
    }


    private void AddSteeringBehaviours()
    {
        // Para el proyectil, añadimos el Seek y el Face (si no están ya añadidos)
        if (_steeringsAdded) return;
        if (target == null) return;
        if (target.agent == null) return;

        // Añadimos el Seek
        if (_seekBehaviour == null)
        {
            _seekBehaviour = gameObject.AddComponent<Seek>();
            _seekBehaviour.target = target.agent;
            if (this.listSteerings != null)
            {
                listSteerings.Add(_seekBehaviour);
            }
        }
        
        // Añadimos el Face
        if (_faceBehaviour == null)
        {
            _faceBehaviour = gameObject.AddComponent<Face>();
            _faceBehaviour.target = target.agent;
            if (listSteerings != null)
            {
                listSteerings.Add(_faceBehaviour);
            }
        }
        _steeringsAdded = true;
    }

    /// <summary>
    /// Attacks the target unit and returns the damage dealt. Modified copy of method from Melee unit.
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    protected int AttackTarget()
    {
        if (target == null || shooter == null) return 0;

        int damageDealt;
        Modifier modifier = Modifier.GetInstance();

        double attackPrecision = Math.Floor(UnityEngine.Random.Range(0.0f, 100.0f) * modifier.getPrecisionModifier(shooter, target)); // TODO: cambiar a calculo terreno

        if (attackPrecision >= 100)
        {
            damageDealt = (int)Math.Floor(shooter.attack * 1.5);
        }
        else if (attackPrecision >= Unit.NeededPrecision)
        {
            damageDealt = (int)Math.Floor(shooter.attack * modifier.getAttackModifier(shooter, target));

            if (shooter.magicAttack)
                damageDealt -= target.magicDefense;
            else
                damageDealt -= target.physicalDefense;

            if (damageDealt < 0)
            {
                Debug.Log($"El ataque de {shooter.type} ha sido resistido completamente por {target.type}.");
                damageDealt = -2; // Daño resistido
            }
        }
        else
        {
            Debug.Log($"El ataque de {shooter.type} ha sido esquivado por {target.type}.");
            damageDealt = -1; // fallo
        }

        return damageDealt;
    }
}
