using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

using TMPro;

public enum BANDO
{
    None,
    Red,
    Blue
}
public abstract class Unit : MonoBehaviour
{
    // De normal existe un 50% probabilidad de acertar un ataque
    public static int NeededPrecision = 25;
    public GameObject damageTextPrefab;
    public GameObject healingTextPrefab;

    [Tooltip("Tiempo en segundos antes de poder respawnear")]
    public float respawnTime = 3f;

    public enum Type
    {
        Paladin, 
        Cleric, 
        Rogue, 
        Wizard, 
        Ranger
    };

    public AgentNPC agent;

    [Tooltip("Tipo de unidad")]
    public Type type;

    [Tooltip("Equipo al que pertenecen")]
    public BANDO teamID;

    public int maxHealth;
    public int health;

    public int attack;
    public bool magicAttack;

    public int physicalDefense;
    public int magicDefense;
    public float dodge;

    public double range;

    public bool debugMode = false;

    public float attackCooldown = 1f;

    // Variables protegidas de estado interno
    protected float _lastAttackTime = -Mathf.Infinity;
    protected float _deathTime = -Mathf.Infinity;
    public bool IsDead = false;

    // TODO: temporalmente aqui, borrar cuando se haga el comportamiento de las unidades.
    [Tooltip("Where the agent should seek to")]
    public Unit seekTarget = null;

    [Tooltip("Weight assigned to the Seek behaviour in the WeightedSteering")]
    public float weight = 1f;
    Seek seek;

    public System.Action<Unit> OnDeath;
    protected virtual void Start()
    {
        this.agent = GetComponent<AgentNPC>();
        if (this.agent == null)
        {
            Debug.LogError("Unit: AgentNPC component not found on this GameObject.");
            //return;
        }

        InitializeStats();
    }

    void Update()
    {
        // TODO: esto esta aquí hasta que se haga el arbol de comportamiento de las unidades, luego se borra
        autoAttack();

        if (IsDead && CanRespawn)
        {
            Respawn(Vector3.zero);
        }
    }

    protected abstract void CheckGround();


    public Type getType()
    {
        return type;
    }

    public Vector3 GetPosition()
    {
        if (agent == null)
        {
            agent = GetComponent<AgentNPC>();
            if (agent == null)
            {
                Debug.LogWarning("Unit.getPosition called without AgentNPC on object " + gameObject.name);
                return transform.position;
            }
        }
        return agent.Position;
    }

    public virtual void autoAttack()
    {
        foreach (Unit unit in FindObjectsOfType<Unit>())
        {
            if (unit.teamID != this.teamID && isInRange(unit))
            {
                TryAttack(unit);
                return;
            }
        }
    }
    public bool isInRange(Unit target)
    {
        // Debug.Log("target nulo? " + (target == null));
        return Vector3.Distance(this.GetPosition(), target.GetPosition()) <= this.range;
    }

    // Aplica los valores desde una entrada de stats obtenidad de la base de datos
    protected void ApplyStats(StatsEntry stats)
    {
        if (stats == null)
        {
            Debug.LogError("ApplyStats: stats entry is null.");
            return;
        }
        //this.agent.MaxSpeed = stats.maxSpeed; // La velocidad maxima se encuentra en el agente
        this.maxHealth = stats.maxHealth;
        this.health = stats.maxHealth; // iniciar con salud completa
        this.attack = stats.attack;
        this.magicAttack = stats.magicAttack;
        this.physicalDefense = stats.physicalDefense;
        this.magicDefense = stats.magicDefense;
        this.dodge = stats.dodge;
        this.range = stats.range;
        this.attackCooldown = stats.attackCooldown;
    }

    // Inicializa las estad sticas del unit desde la base de datos
    protected void InitializeStats()
    {
        StatsEntry stats = UnitStatsDatabase.getInstance.GetStats(this.type.ToString());
        ApplyStats(stats);
    }

    private DetectorTerreno detectorTerreno;

    public readonly LayerMask capasTerreno = 8; // Asignar en el inspector las capas que representan el terreno
    public TipoTerreno GetTerrainUnderUnit()
    {
        DetectorTerreno.DetectarTerreno(this.transform, 10f, capasTerreno);

        // Fallback: buscar en el Grid
        TipoTerreno gridTerrain = GetTerrainFromGridFallback();
        return gridTerrain;
    }

    private TipoTerreno GetTerrainFromGridFallback()
    {
        // TODO: Arreglar
        Grid grid = FindObjectOfType<Grid>();
        if (grid == null)
        {
            return TipoTerreno.Plain;
        }

        Vector3 unitPos = GetPosition();
        float searchRadius = 5f;
        float closestDistance = float.MaxValue;
        TipoTerreno closestTerrain = TipoTerreno.Plain;

        // Buscar la celda más cercana
        for (int i = 0; i < grid.columnas; i++)
        {
            for (int j = 0; j < grid.filas; j++)
            {
                GridCell cell = grid.GetCellAt(i, j);
                if (cell != null)
                {
                    Vector3 cellCenter = grid.GetCellCenter(i, j);
                    float distance = Vector3.Distance(unitPos, cellCenter);

                    // Si está dentro del radio de búsqueda y es la más cercana
                    if (distance <= searchRadius && distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestTerrain = cell.terrainType;
                    }
                }
            }
        }

        if (closestDistance < float.MaxValue)
        {
            return closestTerrain;
        }

        return TipoTerreno.Plain;
    }

    protected abstract int AttackTarget(Unit target);

    // Comprueba si el ataque est  en cooldown o no
    public bool CanAttack => Time.time - _lastAttackTime >= attackCooldown;

    public virtual bool TryAttack(Unit target)
    {
        if (target == null || !CanAttack || !isInRange(target) || target.teamID == this.teamID)
        {
            return false;
        }

        int damage = AttackTarget(target);
        target.TakeDamage(damage);

        return damage < 0;
    }

    public void TakeDamage(int damage)
    {
        if (IsDead) return; // No recibe da o si muerto
        GameObject DamageTextInstance = Instantiate(damageTextPrefab, this.transform);
        //DamageTextInstance.transform.localPosition = new Vector3(0, 2.5f, 0); // Ajusta la posici n del texto de da o sobre la 
        if (damage == -1)
        {
            DamageTextInstance.transform.GetChild(0).GetComponent<TextMeshPro>().text = "Dodge";
            Debug.Log($"{this.type} ha esquivado el ataque.");
            return; // Ataque esquivado
        }

        if (damage == -2)
        {
            DamageTextInstance.transform.GetChild(0).GetComponent<TextMeshPro>().text = "Resist";
            Debug.Log($"{this.type} ha resistido el ataque.");
            return; // Ataque resistido
        }

        DamageTextInstance.transform.GetChild(0).GetComponent<TextMeshPro>().text = damage.ToString();
        this.health -= damage;

        // TODO: Update health bar UI here ',:|

        Debug.Log($"{this.type} ha recibido {damage} puntos de da o. Salud restante: {this.health}/{this.maxHealth}");
        if (this.health <= 0)
        {
            Die();
        }

    }

    public void GetHeal(int healing)
    {
        if (IsDead) return; // No recibe healing si  muerto

        GameObject HealingTextInstance = Instantiate(damageTextPrefab, this.transform);
        HealingTextInstance.transform.GetChild(0).GetComponent<TextMeshPro>().text = healing.ToString();
        HealingTextInstance.transform.GetChild(0).GetComponent<TextMeshPro>().color = Color.green;
        if (this.health + healing > this.maxHealth)
        {
            healing = this.maxHealth - this.health; // No sobrepasa la salud m xima
        }
        else
        {
            this.health += healing;
        }
    }



    /// <summary>
    /// Maneja la muerte de la unidad
    /// </summary>
    private void Die()
    {        
        IsDead = true;
        _deathTime = Time.time;
        // TODO: Esto lo debe hacer el árbol de comportamiento
        FindObjectsByType<GameManager>(FindObjectsSortMode.None)[0].RegisterDeadUnit(this);
        
        var a = gameObject.GetComponent<PathFindingTactical>();
        if (a != null) a.ForceRepath();

        OnDeath?.Invoke(this); // TODO: pasar el objetivo que mató a la unidad
        gameObject.SetActive(false);
    }


    /// <summary>
    /// Puede respawnear si está muerto y pasó el tiempo suficiente
    /// </summary>
    public bool CanRespawn => IsDead && (Time.time - _deathTime >= respawnTime);

    /// <summary>
    /// Respawnea la unidad en su punto de respawn asignado
    /// </summary>
    public void Respawn(Vector3 respawnPoint)
    {
        if (!IsDead)
        {
            Debug.LogWarning($"{this.type} está vivo, no puede respawnear.");
            return;
        }

        this.health = this.maxHealth;

        // Usar siempre el respawn point del StrategicManager si existe
        if (respawnPoint != null)
        {   
            this.agent.Position = new Vector3(respawnPoint.x, agent.Position.y, respawnPoint.z);
        }
        else
        {
            Debug.LogWarning($"{this.type} no tiene respawnPoint asignado.");
        }

        gameObject.SetActive(true);
        IsDead = false;

        
        Debug.Log($"{this.type} ({this.teamID}) respawneado en {this.agent.Position}.");
    }


    public void OnDrawGizmos()
    {
        if (!debugMode) return;
        // Dibuja el rango de ataque como una esfera alrededor de la unidad
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, (float)range);
    }

    public void setRespawnTime(float time)
    {
        this.respawnTime = time;
    }

    public float getRespawnTime()
    {
        return this.respawnTime;
    }

    // Evento que se dispara cuando la unidad es deshabilitada (muere)
    // Necesario para los objetivos
    public System.Action<Unit> OnUnitDisabled;

    void OnDisable()
    {
        OnUnitDisabled?.Invoke(this);
    }

    internal bool IsHealLow()
    {
        return health < maxHealth / 3;
    }

    internal bool IsFullHealth()
    {
        return health >= maxHealth;
    }
}
