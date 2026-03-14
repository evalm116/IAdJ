using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LRTASeek : MonoBehaviour
{
    public enum HeuristicType
    {
        Manhattan,
        Euclidean,
        Chebyshev
    }

    public HeuristicType heuristicType = HeuristicType.Manhattan;

    public Grid grid;
    public float[,] gridHeuristics;
    private GridCell _goalCell;
    private bool _caminoValido = true;
    public GridCell GoalCell
    {
        get => _goalCell;
        set
        {
            _goalCell = value;
            // Reiniciar las heurísticas aprendidas de todas las celdas al cambiar la meta
            ResetHeuristics();
        }
    }

    private void Start()
    {
        if (grid == null)
        {
            Debug.LogError("Grid no está asignada en el PathfindingManager.");
            return;
        }
        gridHeuristics = new float[grid.columnas, grid.filas];
        ResetHeuristics();
    }

    /// <summary>
    /// Calcula el siguiente paso a seguir desde la celda de inicio utilizando el algoritmo LRTA*.
    /// En esta versión solo se calcula el siguiente paso, no toda la ruta.  Algoritmo según el libro 
    /// "Edelkamp, S., & Schrödl, S. (2012). Heuristic search : theory and applications Stefan Edelkamp,
    /// Stefan Schrödl. (1st edition). Morgan Kaufmann." Página 471 algoritmo 11.1.
    /// </summary>
    /// <param name="startCell">Posición actual del personaje</param>
    /// <returns>La siguiente celda a alcanzar</returns>
    public GridCell FindPath(GridCell startCell, int radioEspacioBusqueda)
    {
        GridCell currentCell = startCell;
        HashSet<GridCell> espacioBusqueda;
        if (currentCell != GoalCell && _caminoValido)
        {
            espacioBusqueda = GetEspacioBusqueda(currentCell, radioEspacioBusqueda);

            //algoritmo 11.2 (calcula la heuristica de las celdas del espacio de búsqueda)
            ValueUpdateStep(GoalCell, espacioBusqueda);

            do
            {
                currentCell = GetNextStep(currentCell);
                if (currentCell == null)
                {
                    Debug.LogWarning("No se encontró un camino válido.");
                    _caminoValido = false;
                    return null;
                }
            }
            while (espacioBusqueda.Contains(currentCell));
        }

        return currentCell;
    }


    /// <summary>
    /// Calcula el espacio de búsqueda a partir de la celda actual. Consideramos que el personaje solo se mueve en vertical y horizontal.
    /// </summary>
    /// <param name="currentCell">Celda origen</param>
    /// <param name="tamanoEspacio">Radio del espacio de búsqueda</param>
    /// <returns></returns>
    public HashSet<GridCell> GetEspacioBusqueda(GridCell currentCell, int tamanoEspacio)
    {
        HashSet<GridCell> espacioBusqueda = new HashSet<GridCell>
        {
            // Agregar la celda actual al espacio de búsqueda
            currentCell
        };

        // Agregar los vecinos de la celda actual al espacio de búsqueda
        List<GridCell> neighbors = grid.GetNeighbors(currentCell);
        foreach (GridCell neighbor in neighbors)
        {
            if (!espacioBusqueda.Contains(neighbor) && neighbor != GoalCell)
            {
                espacioBusqueda.Add(neighbor);
                if (tamanoEspacio > 1)
                {
                    espacioBusqueda.UnionWith(GetEspacioBusqueda(neighbor, tamanoEspacio - 1));
                }
            }
        }

        return espacioBusqueda;
    }

    /// <summary>
    /// Calculamos la heurística de las celdas del espacio de búsqueda utilizando el algoritmo de actualización de valores 
    /// (Value Update Step). Según el libro "Edelkamp, S., & Schrödl, S. (2012). Heuristic search : theory and applications 
    /// / Stefan Edelkamp, Stefan Schrödl. (1st edition). Morgan Kaufmann." Página 472 algoritmo 11.2.
    /// </summary>
    /// <param name="goalCell"></param>
    /// <param name="espacioBusqueda"></param>
    public void ValueUpdateStep(GridCell goalCell, HashSet<GridCell> espacioBusqueda)
    {
        List<(GridCell, float)> oldValues = new List<(GridCell, float)>();
        List<GridCell> infinitos = new List<GridCell>();

        // Inicializamos heurística a infinito guardando la anterior
        foreach (GridCell cell in espacioBusqueda)
        {
            oldValues.Add((cell, GetCellHeuristicSafe(cell)));
            gridHeuristics[cell.gridPosition.x, cell.gridPosition.y] = float.MaxValue;
            // cell.learnedHeuristic = float.MaxValue;
            infinitos.Add(cell);
        }

        while (infinitos.Any())
        {
            GridCell actual;
            float new_value;
            (actual, new_value) = ArgMinMax(infinitos, oldValues, goalCell);
            infinitos.Remove(actual);
            gridHeuristics[actual.gridPosition.x, actual.gridPosition.y] = new_value;
            //actual.learnedHeuristic = new_value;

            //if (actual.learnedHeuristic == float.MaxValue)
            if (gridHeuristics[actual.gridPosition.x, actual.gridPosition.y] == float.MaxValue)
            {
                Debug.LogWarning("No se encontró un camino válido.");
                return;
            }
        }
    }

    private (GridCell, float) ArgMinMax(List<GridCell> infinitos, List<(GridCell, float)> oldValues, GridCell goalCell)
    {
        // arg min_{u∈Slss | h(u) =∞} max{temp(u), min_{a∈A(u)}{w(u,a) + h(Succ(u,a))} }
        // Calcula la heurística mínimo entre las celdas cuya heurística es actualmente infinita,
        // la heurística de cada celda se actualiza con el máximo entre su valor anterior y el
        // mínimo de las heurísticas de sus vecinos más el costo de llegar a ellos.

        float minValue = float.MaxValue;
        GridCell minCell = null;

        foreach (GridCell cell in infinitos)
        {
            float oldValue = oldValues.FirstOrDefault(x => x.Item1 == cell).Item2;

            // min_{a∈A(u)}{w(u,a) + h(Succ(u,a))}
            // obtenemos vecino más barato
            float bestCost = float.MaxValue;
            grid.GetNeighbors(cell).ForEach(neighbor =>
            {
                // Nota: En pimera parte esto no es necesario porque todos
                // los costos son 1, pero en la segunda necesitamos calcular
                // el costo según el terreno. Así que lo dejo.
                float currentCost = cell.cost + GetCellHeuristicSafe(neighbor);
                if (currentCost < bestCost)
                {
                    bestCost = currentCost;
                }
            });

            // Máximo entre el valor anterior y el mejor costo calculado
            // max{ temp(u), min_{ a∈A(u)} { w(u, a) + h(Succ(u, a))} }
            float value = Mathf.Max(oldValue, bestCost);

            // Mínimo entre los calculados
            // arg min_{u∈Slss | h(u) =∞}
            if (value < minValue)
            {
                minValue = value;
                minCell = cell;
            }
        }

        // Devuelve tanto la celda como el valor mínimo porque o si no
        // tendríamos que volver a calcularlo después.
        return (minCell, minValue);
    }

    /// <summary>
    /// Optiene el siguiente paso a seguir desde la celda actual, elegiendo la celda vecina
    /// con menor coste heurístico.
    /// </summary>
    /// <param name="currentCell">Celda actual</param>
    /// <returns>Vecino de menor costo.</returns>
    public GridCell GetNextStep(GridCell currentCell)
    {
        // Argmin(u,A) = min{w(u,a) + h(Succ(u,a))}
        // Donde A es el conjunto de acciones disponibles en el estado u, w(u,a)
        // es el costo de la acción a desde el estado u, y h(Succ(u,a)) es la heurística
        // del estado sucesor después de aplicar la acción a.

        GridCell bestNextCell = null;
        float minF = float.MaxValue;
        foreach (GridCell neighbor in grid.GetNeighbors(currentCell))
        {
            // w(u,a) + h(Succ(u,a))
            float f = currentCell.cost + GetCellHeuristicSafe(neighbor);

            // Con esto buscamos el f mínimo 
            if (f < minF)
            {
                minF = f;
                bestNextCell = neighbor;
            }
        }

        return bestNextCell;
    }

    /// <summary>
    /// Obtiene la heurística de una celda, si el valor está sin inicializar se calcula.
    /// </summary>
    /// <param name="cell">Celda de la que obtener la heurística.</param>
    /// <returns>Heurísitca de la celda.</returns>
    public float GetCellHeuristicSafe(GridCell cell)
    {
        Vector2Int pos = cell.gridPosition;        
        if (gridHeuristics[pos.x, pos.y] < 0)
        {
            gridHeuristics[pos.x, pos.y] = CalculateHeuristic(cell);
        }
        // Devuelve el valor aprendido o recién calculado
        return gridHeuristics[pos.x, pos.y];
    }

    /// <summary>
    /// Calcula la heurística según el tipo seleccionado por la clase.
    /// </summary>
    /// <param name="currentCell"></param>
    /// <returns></returns>
    private float CalculateHeuristic(GridCell currentCell)
    {
        switch (heuristicType)
        {     
            case HeuristicType.Euclidean:
                return CalculateEuclideanHeuristic(currentCell);
            case HeuristicType.Chebyshev:
                return CalculateChebyshevHeuristic(currentCell);
            case HeuristicType.Manhattan:
            default:
                return CalculateManhattanHeuristic(currentCell);
        }        
    }
    
    /// <summary>
    /// Calcula la heurística según el algoritmo de Manhattan.
    /// </summary>
    /// <param name="current">Celda actual</param>
    /// <returns>Heurística</returns>
    private float CalculateManhattanHeuristic(GridCell current)
    {
        if (GoalCell == null)
        {
            Debug.LogError("GoalCell no está asignada. No se puede calcular la heurística.");
            return float.MaxValue;
        }

        return Mathf.Abs(current.gridPosition.x - GoalCell.gridPosition.x) + Mathf.Abs(current.gridPosition.y - GoalCell.gridPosition.y);
    }

    /// <summary>
    /// Calcula la heurística según la distancia euclídea.
    /// </summary>
    /// <param name="current">Celda actual</param>
    /// <returns>Heurística</returns>
    private float CalculateEuclideanHeuristic(GridCell current)
    {
        if (GoalCell == null)
        {
            Debug.LogError("GoalCell no está asignada. No se puede calcular la heurística.");
            return float.MaxValue;
        }
        return Vector2Int.Distance(current.gridPosition, GoalCell.gridPosition);
    }

    /// <summary>
    /// Calcula la heurística según la distancia de Chebyshev.
    /// </summary>
    /// <param name="current">Celda actual</param>
    /// <returns>Heurística</returns>
    private float CalculateChebyshevHeuristic(GridCell current)
    {
        if (GoalCell == null)
        {
            Debug.LogError("GoalCell no está asignada. No se puede calcular la heurística.");
            return float.MaxValue;
        }
        return Mathf.Max(Mathf.Abs(current.gridPosition.x - GoalCell.gridPosition.x), Mathf.Abs(current.gridPosition.y - GoalCell.gridPosition.y));
    }


    private void ResetHeuristics()
    {
        for (int x = 0; x < gridHeuristics.GetLength(0); x++)
        {
            for (int z = 0; z < gridHeuristics.GetLength(1); z++)
            {
                gridHeuristics[x, z] = -1.0f;
            }
        }
    }
}