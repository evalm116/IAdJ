using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentNPC : Agent
{ 
    // Este será el steering final que se aplique al personaje.
    [SerializeField] protected Steering steer;
    // Todos los steering que tiene que calcular el agente.
    private List<SteeringBehaviour> listSteerings;


    protected  void Awake()
    {
        this.steer = new Steering();

        // Construye una lista con todos las componenentes del tipo SteeringBehaviour.
        // La llamaremos listSteerings
        listSteerings = new List<SteeringBehaviour>(GetComponents<SteeringBehaviour>());
        // Puedes usar GetComponents<>()
    }


    // Use this for initialization
    void Start()
    {
        this.Velocity = Vector3.zero;
    }

    // Update is called once per frame
    public virtual void Update()
    {
        // En cada frame se actualiza el movimiento
        ApplySteering(Time.deltaTime);

        // En cada frame podría ejecutar otras componentes IA
    }


    private void ApplySteering(float deltaTime)
    {
        // Actualizar las propiedades para Time.deltaTime según NewtonEuler
        
        Velocity += steer.linear * deltaTime;
        Rotation += steer.angular * deltaTime;
        Position += Velocity * deltaTime;
        Orientation += Rotation * deltaTime;

        transform.rotation = Quaternion.identity;
        transform.Rotate(Vector3.up, Orientation);
    }



    public virtual void LateUpdate()
    {
        Steering kinematicFinal = new Steering();

        // Reseteamos el steering final.
        this.steer = new Steering();

        // Recorremos cada steering
        foreach (SteeringBehaviour behavior in listSteerings)
        {
            Steering kinematic = behavior.GetSteering(this);

            kinematicFinal.linear += kinematic.linear;
            kinematicFinal.angular += kinematic.angular;
        }
        //// La cinemática de este SteeringBehaviour se tiene que combinar
        //// con las cinemáticas de los demás SteeringBehaviour.
        //// Debes usar kinematic con el árbitro desesado para combinar todos
        //// los SteeringBehaviour.
        //// Llamaremos kinematicFinal a la aceleraciones finales de esas combinaciones.

        // A continuación debería entrar a funcionar el actuador para comprobar
        // si la propuesta de movimiento es factible:
        // kinematicFinal = Actuador(kinematicFinal, self)


        // El resultado final se guarda para ser aplicado en el siguiente frame.
        this.steer = kinematicFinal;
    }

    public void StopMoving()
    {
        Velocity = Vector3.zero;
        Speed = 0f;
        Rotation = 0f;
    }
}
