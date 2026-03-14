using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(AgentNPC))]
public class SteeringBehaviour : MonoBehaviour
{

    protected string nameSteering = "no steering";

    public string NameSteering
    {
        set { nameSteering = value; }
        get { return nameSteering; }
    }

    // Target sobre el que se calcula el comportamiento
    public Agent target;
    public float weight = 1.0f;


    /// <summary>
    /// Cada SteerinBehaviour retornará un Steering=(vector, escalar)
    /// acorde a su propósito: llegar, huir, pasear, ...
    /// Sobreescribie siempre este método en todas las clases hijas.
    /// </summary>
    /// <param name="agent"></param>
    /// <returns></returns>
    public virtual Steering GetSteering(AgentNPC agent)
    {
        return null;
    }


    protected virtual void OnGUI()
    {
        // Para la depuración te puede interesar que se muestre el nombre
        // del steeringbehaviour sobre el personaje.
        // Te puede ser util Rect() y GUI.TextField()
        // https://docs.unity3d.com/ScriptReference/GUI.TextField.html
    }

    protected float GetArriveDistance(AgentNPC character)
    {
        if (character.Velocity.magnitude > 0)
        {
            return 0.04f * character.Velocity.magnitude;
        }
        // Si está parado devolvemos distancia fija
        return 0.1f;
    }
}
