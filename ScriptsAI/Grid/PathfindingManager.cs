using System;
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
        if(currentCell != goalCell) 
        {
            espacioBusqueda = getEspacioBusqueda(currentCell, goalCell, 1);

            //algoritmo 11.2 (calcula la heuristica de las celdas del espacio de búsqueda)
            ValueUpdateStep(currentCell, goalCell, espacioBusqueda);

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
        List<GridCell> infinitos = new List<GridCell>();
        foreach (GridCell cell in espacioBusqueda)
        {
            if (cell.learnedHeuristic == -1)
                cell.learnedHeuristic = CalculateManhattanHeuristic(cell, goalCell);
            oldValues.Add((cell, cell.learnedHeuristic));
            cell.learnedHeuristic = float.MaxValue;
            infinitos.Add(cell);
        }

        while (infinitos.Any())
        {
            GridCell actual;
            float new_value;
            (actual, new_value)= getMin(infinitos, oldValues, goalCell);
            infinitos.Remove(actual);
            actual.learnedHeuristic = new_value;

            if (actual.learnedHeuristic == float.MaxValue)
            {
                Debug.LogWarning("No se encontró un camino válido.");
                return;
            }
        }
        /*
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
            cell.learnedHeuristic = Mathf.Max(cell.learnedHeuristic, cell.cost + minF);
            pq.Add(cell);
        }

        // Ordenar de menor a mayor según el valor de learnedHeuristic
        pq.Sort((a, b) => a.learnedHeuristic.CompareTo(b.learnedHeuristic));
        
        while (pq.Any())
        {
            
        }*/

    }

    private (GridCell, float) getMin(List<GridCell> infinitos, List<(GridCell, float)> oldValues, GridCell goalCell)
    {
        float minValue = float.MaxValue;
        GridCell minCell = null;

        foreach (GridCell cell in infinitos)
        {
            float oldValue = oldValues.FirstOrDefault(x => x.Item1 == cell).Item2;

            float bestCost = float.MaxValue;
            grid.GetNeighbors(cell).ForEach(neighbor =>
            {
                if (neighbor.learnedHeuristic == -1)
                    CalculateManhattanHeuristic(neighbor, goalCell);
                float f = cell.cost + neighbor.learnedHeuristic;
                if (f < bestCost)
                {
                    bestCost = f;
                }
            });

            float value = Mathf.Max(oldValue, bestCost);

            if (value < minValue)
            {
                minValue = value;
                minCell = cell;
            }
        }

        return (minCell, minValue);
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