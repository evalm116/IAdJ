using UnityEngine;

public class SoldadoFSM : MonoBehaviour
{
    public enum Estado
    {
        Reagrupando,
        Acercandose,
        EnPosicion
    }

    [Header("Máquina de Estados")]
    public Estado estadoActual = Estado.Reagrupando;

    [Header("Velocidades")]
    public float velocidadTranquila = 4f; 
    public float velocidadSprint = 12f;   

    [Header("Distancias")]
    public float distanciaParaSprint = 2.5f; 
    public float distanciaLlegadaExacta = 0.5f; 

    private AgentNPC miAgente;
    private Arrive miArrive;
    private Face miFace;
    private Align miAlign; 

    void Start()
    {
        miAgente = GetComponent<AgentNPC>();
        miArrive = GetComponent<Arrive>();
        miFace = GetComponent<Face>();

        // Recuperamos el Align puro que pusiste en el Inspector de Unity
        foreach (Align a in GetComponents<Align>())
        {
            if (a.GetType() == typeof(Align)) miAlign = a;
        }
    }

    void Update()
    {
        if (miArrive == null || miArrive.target == null) return;

        float distanciaAlTarget = Vector3.Distance(transform.position, miArrive.target.Position);

        // --- 1. LÓGICA DE ESTADOS LIMPIA ---
        if (distanciaAlTarget > distanciaParaSprint) estadoActual = Estado.Reagrupando;
        else if (distanciaAlTarget > distanciaLlegadaExacta) estadoActual = Estado.Acercandose;
        else estadoActual = Estado.EnPosicion; 

        // --- 2. EJECUTAR LOS COMPORTAMIENTOS (STEERING) ---
        switch (estadoActual)
        {
            case Estado.Reagrupando:
            case Estado.Acercandose:
                miAgente.MaxSpeed = (estadoActual == Estado.Reagrupando) ? velocidadSprint : velocidadTranquila;
                
                // Al correr, le damos el fantasma al Face y apagamos el Align
                if (miFace != null)
                {
                    miFace.target = miArrive.target;
                    miFace.enabled = true;
                }
                if (miAlign != null) miAlign.enabled = false;
                break;

            case Estado.EnPosicion:
                // AL LLEGAR: Apagamos Face y le QUITAMOS el target para que no nos sabotee
                if (miFace != null)
                {
                    miFace.enabled = false;
                    miFace.target = null;
                }
                // Encendemos Align y nos aseguramos de que tiene el fantasma
                if (miAlign != null)
                {
                    miAlign.enabled = true;
                    miAlign.target = miArrive.target;
                }
                break;
        }
    }
}