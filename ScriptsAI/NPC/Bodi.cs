using UnityEngine;


public enum Range 
{
    NegPiToPi,    // Para ángulos entre -180 y 180
    ZeroToTwoPi   // Para ángulos entre 0 y 360
}

public class Bodi : MonoBehaviour
{

    [SerializeField] protected float _mass = 1;
    [SerializeField] protected float _maxSpeed = 1;
    [SerializeField] protected float _maxRotation = 1;
    [SerializeField] protected float _maxAcceleration = 1;
    [SerializeField] protected float _maxAngularAcc = 1;
    [SerializeField] protected float _maxForce = 1;

    protected Vector3 _acceleration; // aceleración lineal
    protected float _angularAcc;  // aceleración angular
    [SerializeField] protected Vector3 _velocity; // velocidad lineal
    protected float _rotation;  // velocidad angular
    protected float _speed;  // velocidad escalar
    protected float _orientation;  // 'posición' angular
    // Se usará transform.position como 'posición' lineal

    /// Un ejemplo de cómo construir una propiedad en C#
    /// <summary>
    /// Mass for the NPC
    /// </summary>
    public float Mass
    {
        get { return _mass; }
        set { _mass = Mathf.Max(0, value); }
    }

    // CONSTRUYE LAS PROPIEDADES SIGUENTES. PUEDES CAMBIAR LOS NOMBRE A TU GUSTO
    // Lo importante es controlar el set
    public float MaxForce
    {
        get { return _maxForce; }
        set { _maxForce = Mathf.Max(0, value); }
    }

    public float MaxSpeed
    {
        get { return _maxSpeed; }
        set { _maxSpeed = Mathf.Max(0, value); }
    }

    public Vector3 Velocity
    {
        get { return _velocity; }
        set { _velocity = Vector3.ClampMagnitude(value, MaxSpeed); }
    }

    public float MaxRotation
    {
        get { return _maxRotation; }
        set { _maxRotation = Mathf.Max(0, value); }
    }

    public float Rotation
    {
        get { return _rotation; }
        set { _rotation = Mathf.Clamp(value, -_maxRotation, _maxRotation); }
    }

    public float MaxAcceleration
    {
        get { return _maxAcceleration; }
        set { _maxAcceleration = Mathf.Max(0, value); }
    }

    public Vector3 Acceleration
    {
        get { return _acceleration; }
        set { _acceleration = Vector3.ClampMagnitude(value, _maxAcceleration); }
    }

    public float MaxAngularAcc
    {
        get { return _maxAngularAcc; }
        set { _maxAngularAcc = Mathf.Max(0, value); }
    }

    public float AngularAcc
    {
        get { return _angularAcc; }
        set { _angularAcc = Mathf.Clamp(value, 0, _maxAngularAcc); }
    }

    public Vector3 Position
    {
        get { return transform.position; }
        set { transform.position = value; }
    }

    public float Orientation
    {
        get { return _orientation; }
        set { _orientation = value; }
    }
    
    public float Speed
    {
        get { return _speed; }
        set { _speed = Mathf.Clamp(value, 0, _maxSpeed); }
    }

   
    public static float MapToRange(float rotation, Range r)
    {
        if (r == Range.NegPiToPi)
        {
            while (rotation > 180) rotation -= 360;
            while (rotation < -180) rotation += 360;
        }
        else if (r == Range.ZeroToTwoPi)
        {
            while (rotation < 0) rotation += 360;
            while (rotation >= 360) rotation -= 360;
        }
        return rotation;
    }

    public float Heading()
    {
        if (Velocity.sqrMagnitude < Mathf.Epsilon) return 0f;
        float angle = Mathf.Atan2(Velocity.x, Velocity.z) * Mathf.Rad2Deg;
        return angle; 
    }


    public float MapToRange(Range r)
    {
        return MapToRange(Orientation, r);
    }

    public float PositionToAngle()
    {
        Vector3 p = Position;
        float angle = Mathf.Atan2(p.x, p.z) * Mathf.Rad2Deg;
        return angle;
    }

    public Vector3 OrientationToVector()
    {
        float rad = Orientation * Mathf.Deg2Rad;
        return new Vector3(Mathf.Sin(rad), 0f, Mathf.Cos(rad));
    }

    public Vector3 VectorHeading()
    {
        return OrientationToVector();
    }

    public float GetMiniminAngleTo(Vector3 direction)
    {
        if (direction.sqrMagnitude < Mathf.Epsilon) return 0f;

        float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        float delta = targetAngle - Orientation;

        delta = (delta + 180f) % 360f;
        if (delta < 0f) delta += 360f;
        delta -= 180f;

        return delta;
    }

    public void ResetOrientation()
    {
        Orientation = 0f;
        Rotation = 0f;
        AngularAcc = 0f;
        transform.rotation = Quaternion.identity;
    }

    public float PredictNearestApproachTime(Bodi other, float timeInit, float timeEnd)
    {
        Vector3 relPos0 = other.Position - this.Position;
        Vector3 relVel = other.Velocity - this.Velocity;

        float relSpeedSq = relVel.sqrMagnitude;
        if (relSpeedSq < Mathf.Epsilon)
        {
            return timeInit;
        }

        float tStar = -Vector3.Dot(relPos0, relVel) / relSpeedSq;

        tStar = Mathf.Clamp(tStar, timeInit, timeEnd);
        return tStar;
    }

    public float PredictNearestApproachDistance3(Bodi other, float timeInit, float timeEnd)
    {
        float t = PredictNearestApproachTime(other, timeInit, timeEnd);

        Vector3 thisFuture = this.Position + this.Velocity * t;
        Vector3 otherFuture = other.Position + other.Velocity * t;

        return Vector3.Distance(thisFuture, otherFuture);
    }
}
