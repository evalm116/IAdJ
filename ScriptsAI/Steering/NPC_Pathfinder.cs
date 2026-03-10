using System.Runtime;
using UnityEngine;

public class NPC_Pathfinder : Seek
{
    [Header("Referencias")]
    public PathfindingManager pathManager;
    public Grid gameGrid;
    public Transform objetivo; // El objeto al que queremos llegar 

    [Header("Ajustes de Movimiento")]
    public float velocidad = 3f;

    // La celda a la que el NPC se está dirigiendo en este momento
    private GridCell celdaDestinoActual;

    void Start()
    {
        if (target == null)
        {
            GameObject dummy = new GameObject("DummyPathTarget");
            target = dummy.AddComponent<Agent>();
        }
        PedirSiguientePaso();
    }

    public override Steering GetSteering(AgentNPC character)
    {
        // Si no tenemos objetivo o celda a la que ir, no hacemos nada
        if (objetivo == null || celdaDestinoActual == null) return new Steering();

        // 1. Obtenemos la posición física (Vector3) del centro de la celda a la que vamos
        int x = celdaDestinoActual.gridPosition.x;
        int z = celdaDestinoActual.gridPosition.y;
        Vector3 puntoDestino = gameGrid.GetCellCenter(x, z);

        // 2. STEERING BÁSICO (Seek): Nos movemos hacia ese punto
        target.Position = Vector3.MoveTowards(transform.position, puntoDestino, velocidad * Time.deltaTime);

        // 3. COMPROBACIÓN DE LLEGADA
        if (Vector3.Distance(transform.position, puntoDestino) < 0.1f)
        {
            // Hemos llegado a la casilla, ¡toca pensar el siguiente paso!
            PedirSiguientePaso();
        }

        return base.GetSteering(character);
    }

    private void PedirSiguientePaso()
    {
        if (objetivo == null) return;

        // A. Calculamos en qué celda (X, Z) estamos nosotros ahora mismo
        Vector2Int miPosicionGrid = gameGrid.GetGridPosition(transform.position);

        // B. Calculamos en qué celda (X, Z) está nuestra meta final
        Vector2Int metaPosicionGrid = gameGrid.GetGridPosition(objetivo.position);

        // OJO: Nos aseguramos de no salirnos de los límites del array
        if (!PosicionValida(miPosicionGrid) || !PosicionValida(metaPosicionGrid))
        {
            Debug.LogWarning("El NPC o el Objetivo están fuera del Grid.");
            return;
        }

        // C. Cogemos las celdas reales del array
        GridCell miCelda = gameGrid.gridArray[miPosicionGrid.x, miPosicionGrid.y];
        GridCell celdaMeta = gameGrid.gridArray[metaPosicionGrid.x, metaPosicionGrid.y];

        celdaDestinoActual = pathManager.GetNextStepLRTA(miCelda, celdaMeta);
    }

    private bool PosicionValida(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < gameGrid.columnas && pos.y >= 0 && pos.y < gameGrid.filas;
    }
}