using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Steering/InteractiveObject/Agent")]
public class Agent : Bodi
{
    [Tooltip("Radio interior de la IA")]
    [SerializeField] protected float _interiorRadius = 1f;

    [Tooltip("Radio de llegada de la IA")]
    [SerializeField] protected float _arrivalRadius = 3f;

    [Tooltip("Ángulo interior de la IA (grados)")]
    [SerializeField] protected float _interiorAngle = 3.0f; // ángulo sexagesimal.

    [Tooltip("Ángulo exterior de la IA (grados)")]
    [SerializeField] protected float _exteriorAngle = 8.0f; // ángulo sexagesimal.

    [Header("Debug")]
    [SerializeField] protected bool _showGizmos = true;
    [SerializeField] protected Color _innerColor = new Color(1f, 0f, 0f, 0.6f);
    [SerializeField] protected Color _outerColor = new Color(0f, 0f, 1f, 0.6f);

    // PROPIEDADES (garantizar interior < exterior)

    public float InteriorRadius
    {
        get { return _interiorRadius; }
        set
        {
            _interiorRadius = Mathf.Max(0f, value);
            if (_interiorRadius > _arrivalRadius)
                _arrivalRadius = _interiorRadius;
        }
    }

    public float ArrivalRadius
    {
        get { return _arrivalRadius; }
        set
        {
            _arrivalRadius = Mathf.Max(0f, value);
            if (_arrivalRadius < _interiorRadius)
                _interiorRadius = _arrivalRadius;
        }
    }

    public float InteriorAngle
    {
        get { return _interiorAngle; }
        set
        {
            _interiorAngle = Mathf.Clamp(value, 0f, 360f);
            if (_interiorAngle > _exteriorAngle)
                _exteriorAngle = _interiorAngle;
        }
    }

    public float ExteriorAngle
    {
        get { return _exteriorAngle; }
        set
        {
            _exteriorAngle = Mathf.Clamp(value, 0f, 360f);
            if (_exteriorAngle < _interiorAngle)
                _interiorAngle = _exteriorAngle;
        }
    }

    public bool ShowGizmos
    {
        get { return _showGizmos; }
        set { _showGizmos = value; }
    }

    // MÉTODOS FÁBRICA (ejemplo: crear punto de llegada con collider trigger)

    public static Agent CreateArrivalPoint(
        Vector3 position,
        float interiorRadius,
        float arrivalRadius)
    {
        GameObject go = new GameObject("ArrivalPointAgent");
        go.transform.position = position;

        // Collider como trigger
        BoxCollider col = go.AddComponent<BoxCollider>();
        col.isTrigger = true;
        col.size = Vector3.one * (arrivalRadius * 2f);

        Agent agent = go.AddComponent<Agent>();

        // Configuración básica del Bodi (este “agente” no se mueve)
        agent.Mass = 1f;
        agent.MaxSpeed = 0f;
        agent.MaxAcceleration = 0f;
        agent.MaxRotation = 0f;
        agent.MaxAngularAcc = 0f;
        agent.Velocity = Vector3.zero;
        agent.Acceleration = Vector3.zero;

        // Radios sensoriales
        agent.InteriorRadius = interiorRadius;
        agent.ArrivalRadius = arrivalRadius;

        return agent;
    }

    // GIZMOS DE DEPURACIÓN

    protected virtual void OnDrawGizmos()
    {
        if (!_showGizmos) return;

        // Esferas de radios
        Gizmos.color = _innerColor;
        Gizmos.DrawWireSphere(transform.position, _interiorRadius);

        Gizmos.color = _outerColor;
        Gizmos.DrawWireSphere(transform.position, _arrivalRadius);

        // Conos de ángulo interior/exterior (aprox. con líneas)
        Vector3 origin = transform.position;
        Vector3 forward = OrientationToVector(); // de Bodi

        // Ángulos en radianes
        float innerHalf = _interiorAngle * 0.5f * Mathf.Deg2Rad;
        float outerHalf = _exteriorAngle * 0.5f * Mathf.Deg2Rad;

        // Vector base para el cono (frente)
        Vector3 dirInnerLeft = Quaternion.AngleAxis(-_interiorAngle * 0.5f, Vector3.up) * forward;
        Vector3 dirInnerRight = Quaternion.AngleAxis(_interiorAngle * 0.5f, Vector3.up) * forward;
        Vector3 dirOuterLeft = Quaternion.AngleAxis(-_exteriorAngle * 0.5f, Vector3.up) * forward;
        Vector3 dirOuterRight = Quaternion.AngleAxis(_exteriorAngle * 0.5f, Vector3.up) * forward;

        float lineLength = _arrivalRadius;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(origin, origin + dirInnerLeft * lineLength);
        Gizmos.DrawLine(origin, origin + dirInnerRight * lineLength);

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(origin, origin + dirOuterLeft * lineLength);
        Gizmos.DrawLine(origin, origin + dirOuterRight * lineLength);

        // Línea forward para ver hacia dónde mira
        Gizmos.color = Color.green;
        Gizmos.DrawLine(origin, origin + forward * lineLength);
    }
}
