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

        int x = celdaDestinoActual.gridPosition.x;
        int z = celdaDestinoActual.gridPosition.y;
        Vector3 puntoDestino = gameGrid.GetCellCenter(x, z);

        if (Vector3.Distance(objetivo.position, puntoDestino) <= 0.15f)
        {
            character.StopMoving();
            return new Steering(); // Ya hemos llegado al objetivo final
        }


        target.Position = puntoDestino;

        // Se llega al punto destino, se pide el siguiente paso
        if (Vector3.Distance(transform.position, puntoDestino) < 0.1f)
        {
            PedirSiguientePaso();
        }

        return base.GetSteering(character);
    }

    private void PedirSiguientePaso()
    {
        if (objetivo == null) return;

        // A. Calculamos en qué celda (X, Z) estamos nosotros ahora mismo
        Vector2Int miPosicionGrid = gameGrid.GetGridPosition(transform.position);


        if (!PosicionValida(miPosicionGrid))
        {
            Debug.LogWarning("El NPC están fuera del Grid.");
            return;
        }

        // C. Cogemos las celdas reales del array
        GridCell miCelda = gameGrid.gridArray[miPosicionGrid.x, miPosicionGrid.y];

        if (pathManager.GoalCell == null)
        {
            // B. Calculamos en qué celda (X, Z) está nuestra meta final
            Vector2Int metaPosicionGrid = gameGrid.GetGridPosition(objetivo.position);

            if (!PosicionValida(metaPosicionGrid))
            {
                Debug.LogWarning("El Objetivo están fuera del Grid.");
                return;
            }

            GridCell celdaMeta = gameGrid.gridArray[metaPosicionGrid.x, metaPosicionGrid.y];
            pathManager.GoalCell = celdaMeta;
        }
        //celdaDestinoActual = pathManager.GetNextStepLRTA(miCelda, celdaMeta);
        celdaDestinoActual = pathManager.FindPath(miCelda, 1);
    }

    private bool PosicionValida(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < gameGrid.columnas && pos.y >= 0 && pos.y < gameGrid.filas;
    }
}