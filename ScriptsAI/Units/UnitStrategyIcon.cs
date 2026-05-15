using UnityEngine;

public class UnitStrategyIcon : MonoBehaviour
{
    [Header("Iconos (asignar en Inspector)")]
    public GameObject iconOffensive;
    public GameObject iconDefensive;
    public GameObject iconTotalWar;
    public GameObject iconHealer;

    private Unit unit;
    private TacticalAI.Strategy lastStrategy;

    void Start()
    {
        unit = GetComponent<Unit>();
        UpdateIcon();
    }

    void Update()
    {
        // Solo actualiza si la estrategia ha cambiado
        TacticalAI.Strategy current = GameManager.Instance.GetStrategy(unit);
        if (current != lastStrategy)
        {
            lastStrategy = current;
            UpdateIcon();
        }
    }

    private void UpdateIcon()
    {
        //siempre muestra el icono de cleric independientemente de la estrategia
        if (unit.type == Unit.Type.Cleric)
        {
            SetIcon(iconHealer);
            return;
        }

        TacticalAI.Strategy strategy = GameManager.Instance.GetStrategy(unit);
        switch (strategy)
        {
            case TacticalAI.Strategy.Offensive:
                SetIcon(iconOffensive);
                break;
            case TacticalAI.Strategy.Defensive:
                SetIcon(iconDefensive);
                break;
            case TacticalAI.Strategy.TotalWar:
                SetIcon(iconTotalWar);
                break;
        }
    }

    private void SetIcon(GameObject activeIcon)
    {
        iconOffensive?.SetActive(false);
        iconDefensive?.SetActive(false);
        iconTotalWar?.SetActive(false);
        iconHealer?.SetActive(false);
        activeIcon?.SetActive(true);
    }
}