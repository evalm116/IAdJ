using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LRTASeek : PathFindingAlgorithm
{
   

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
        List<GridCell> espacioBusqueda;
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
    public List<GridCell> GetEspacioBusqueda(GridCell currentCell, int tamanoEspacio)
    {


        Queue<(GridCell, int)> nodosBusquedas = new Queue<(GridCell, int)>();
        List<GridCell> espacioBusqueda = new List<GridCell>();

        nodosBusquedas.Enqueue((currentCell, 1));

        // Agregar los vecinos de la celda actual al espacio de búsqueda

        while (nodosBusquedas.Any())
        {
            (var nodo, var radio) = nodosBusquedas.Dequeue();
            espacioBusqueda.Add(nodo);
            var neighborCells = grid.GetNeighbors(nodo);
            foreach (GridCell neighbor in neighborCells)
            {
                if (neighbor != GoalCell && !espacioBusqueda.Contains(neighbor))
                {
                    espacioBusqueda.Add(neighbor);
                    if (radio < tamanoEspacio)
                    {
                        nodosBusquedas.Enqueue((neighbor, radio + 1));
                    }
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
    public void ValueUpdateStep(GridCell goalCell, List<GridCell> espacioBusqueda)
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
            if (value < minValue || minCell == null)
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

        gridHeuristics[currentCell.gridPosition.x, currentCell.gridPosition.y] = Mathf.Max(minF, GetCellHeuristicSafe(currentCell));


        return bestNextCell;
    }
}