using System.Runtime;
using UnityEngine;

public class PathfindingNPCSeek : Seek
{
    [Header("Referencias")]
    public LRTASeek pathManager;
    public Grid gameGrid;
    public Transform objetivo; // El objeto al que queremos llegar 
    private Vector2Int _celdaObjetivo;

    [Header("Ajustes de Movimiento")]
    public int radioEspacioBusqueda = 1;

    // La celda a la que el NPC se está dirigiendo en este momento
    private GridCell celdaDestinoActual;
    

    void Start()
    {
        if (target == null)
        {
            GameObject dummy = new GameObject("DummyPathTarget");
            target = dummy.AddComponent<Agent>();
        }
        if (gameGrid != null)
        {
            _celdaObjetivo = gameGrid.GetGridPosition(objetivo.position);
            if (!gameGrid.PosicionValida(_celdaObjetivo))
            {
                Debug.LogWarning("El Objetivo están fuera del Grid.");
                return;
            }
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

        Vector2Int miPosicionGrid = gameGrid.GetGridPosition(character.Position); 
        if (miPosicionGrid == _celdaObjetivo)        
        {
            character.StopMoving();
            return new Steering(); // Ya hemos llegado al objetivo final
        }


        target.Position = puntoDestino;
        //target.Position = Vector3.MoveTowards(transform.position, puntoDestino, character.MaxSpeed * Time.deltaTime); // Movemos el target suavemente hacia el punto destino

        // Se llega al punto destino, se pide el siguiente paso
        if (Vector3.Distance(target.Position, character.Position) < GetArriveDistance(character))
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


        if (!gameGrid.PosicionValida(miPosicionGrid))
        {
            Debug.LogWarning("El NPC están fuera del Grid.");
            return;
        }

        // C. Cogemos las celdas reales del array
        GridCell miCelda = gameGrid.gridArray[miPosicionGrid.x, miPosicionGrid.y];

        if (pathManager.GoalCell == null)
        {
            if (!gameGrid.PosicionValida(_celdaObjetivo))
            {
                Debug.LogWarning("El Objetivo están fuera del Grid.");
                return;
            }

            GridCell celdaMeta = gameGrid.gridArray[_celdaObjetivo.x, _celdaObjetivo.y];
            pathManager.GoalCell = celdaMeta;
        }
        
        celdaDestinoActual = pathManager.FindPath(miCelda, radioEspacioBusqueda);      
            
    }
}