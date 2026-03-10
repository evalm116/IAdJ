using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

public class PathfindingManager : MonoBehaviour
{
    public Grid grid;
    public GridCell procedureLRTA(GridCell startCell, GridCell goalCell)
    {
        GridCell currentCell = startCell;
        HashSet<GridCell> espacioBusqueda;
        while (currentCell != goalCell)
        {
            espacioBusqueda = getEspacioBusqueda(currentCell, goalCell, 1);

            //algoritmo 11.2 (calcula la heuristica de las celdas del espacio de búsqueda)

            currentCell = GetNextStepLRTA(currentCell, goalCell);
            if (currentCell == null)
            {
                Debug.LogWarning("No se encontró un camino válido.");
                return null;
            }
            while (espacioBusqueda.Contains(currentCell))
            {
                currentCell = GetNextStepLRTA(currentCell, goalCell);
                if (currentCell == null)
                {
                    Debug.LogWarning("No se encontró un camino válido.");
                    return null;
                }
            }
        }

        return currentCell;
    }

    public HashSet<GridCell> getEspacioBusqueda(GridCell currentCell, GridCell goalCell, int tamanoEspacio)
    {
        HashSet<GridCell> espacioBusqueda = new HashSet<GridCell>();

        // Agregar la celda actual al espacio de búsqueda
        espacioBusqueda.Add(currentCell);

        // Agregar los vecinos de la celda actual al espacio de búsqueda
        List<GridCell> neighbors = grid.GetNeighbors(currentCell);
        foreach (GridCell neighbor in neighbors)
        {
            if (!espacioBusqueda.Contains(neighbor) && neighbor != goalCell)
            {
                espacioBusqueda.Add(neighbor);
                if (tamanoEspacio > 1)
                {
                    espacioBusqueda.UnionWith(getEspacioBusqueda(neighbor, goalCell, tamanoEspacio - 1));
                }
            }
        }

        return espacioBusqueda;
    }

    public void ValueUpdateStep(GridCell currentCell, GridCell goalCell, HashSet<GridCell> espacioBusqueda)
    {
        List<(GridCell, float)> oldValues = new List<(GridCell, float)>();
        foreach (GridCell cell in espacioBusqueda)
        {
            oldValues.Add((cell, cell.learnedHeuristic));
            cell.learnedHeuristic = float.MaxValue;
        }

        foreach (GridCell cell in espacioBusqueda)
        {

            List<GridCell> neighbors = grid.GetNeighbors(cell);
            float minF = float.MaxValue;
            foreach (GridCell neighbor in neighbors)
            {
                float f = neighbor.cost + neighbor.learnedHeuristic;
                if (f < minF)
                {
                    minF = f;
                }
            }
            cell.learnedHeuristic = Mathf.Max(cell.learnedHeuristic, minF);
        }

        //FALTA EL PASO 4

    }
    public GridCell GetNextStepLRTA(GridCell currentCell, GridCell goalCell)
    {
        // Si ya estamos en la meta, no nos movemos
        //if (currentCell == goalCell) return currentCell;

        GridCell bestNextCell = null;
        float minF = float.MaxValue;

        List<GridCell> neighbors = grid.GetNeighbors(currentCell);

        // 1. EVALUAR VECINOS 
        foreach (GridCell neighbor in neighbors)
        {
            // if (neighbor.isOccupied) continue;

            // Si el vecino nunca ha sido visitado/evaluado, calculamos su heurística inicial (Distancia Manhattan)
            if (neighbor.learnedHeuristic < 0)
            {
                neighbor.learnedHeuristic = CalculateManhattanHeuristic(neighbor, goalCell);
            }

            // f(vecino) = coste de ir al vecino (w) + heurística del vecino (h)
            // Aquí usamos neighbor.cost como la 'w' de la fórmula
            float f = neighbor.cost + neighbor.learnedHeuristic;

            // Buscamos el vecino con la f más pequeña (argmin)
            if (f < minF)
            {
                minF = f;
                bestNextCell = neighbor;
            }
        }

        // 2. APRENDIZAJE: Actualizar la heurística de la celda actual
        if (currentCell.learnedHeuristic < 0)
        {
            currentCell.learnedHeuristic = CalculateManhattanHeuristic(currentCell, goalCell);
        }

        // h(u) = max(h(u), f_minimo)
        currentCell.learnedHeuristic = Mathf.Max(currentCell.learnedHeuristic, minF);

        // 3. RETORNAR RESULTADO
        return bestNextCell;
    }

    // Calculamos la distancia Manhattan pura y dura
    private float CalculateManhattanHeuristic(GridCell a, GridCell b)
    {
        return Mathf.Abs(a.gridPosition.x - b.gridPosition.x) + Mathf.Abs(a.gridPosition.y - b.gridPosition.y);
    }
}