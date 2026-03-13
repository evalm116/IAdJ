using UnityEngine;

public class SoldadoFSM : MonoBehaviour
{
    public enum Estado
    {
        EnFormacion, // Cerca de su sitio (Va despacio, no derrapa)
        Reagrupando  // Lejos de su sitio (Mete el turbo para alcanzar la V)
    }

    [Header("Máquina de Estados")]
    public Estado estadoActual = Estado.EnFormacion;

    [Header("Velocidades")]
    public float velocidadTranquila = 4f; // IGUAL a la del Líder gris
    public float velocidadSprint = 12f;   // El turbo para las curvas y cuando se queda atrás

    [Header("Distancias de Cambio")]
    public float distanciaParaSprint = 2.5f; // Si se aleja más de 2.5m, arranca a correr
    public float distanciaParaFrenar = 1.0f; // Si se acerca a 1m, echa el freno de mano

    // Referencias internas
    private AgentNPC miAgente;
    private Arrive miArrive;

    void Start()
    {
        miAgente = GetComponent<AgentNPC>();
        miArrive = GetComponent<Arrive>();
    }

    void Update()
    {
        // Si no tiene target (aún no se ha unido a la formación), no hace nada
        if (miArrive == null || miArrive.target == null) return;

        // 1. Calculamos a qué distancia está de su "fantasma" (su hueco en la V)
        float distanciaAlTarget = Vector3.Distance(transform.position, miArrive.target.Position);

        // 2. Lógica de cambio de estado
        if (estadoActual == Estado.EnFormacion && distanciaAlTarget > distanciaParaSprint)
        {
            estadoActual = Estado.Reagrupando;
        }
        else if (estadoActual == Estado.Reagrupando && distanciaAlTarget <= distanciaParaFrenar)
        {
            estadoActual = Estado.EnFormacion;
        }

        // 3. Aplicar el comportamiento del estado (El truco anti-derrapes)
        switch (estadoActual)
        {
            case Estado.Reagrupando:
                miAgente.MaxSpeed = velocidadSprint;
                break;

            case Estado.EnFormacion:
                // Al cortarle la velocidad máxima de golpe, evitamos que se pase de largo y derrape
                miAgente.MaxSpeed = velocidadTranquila;
                break;
        }
    }
}