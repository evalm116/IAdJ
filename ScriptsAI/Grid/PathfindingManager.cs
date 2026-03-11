using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

public class PathfindingManager : MonoBehaviour
{
    public Grid grid;
    private GridCell _goalCell;
    public GridCell GoalCell
    {
        get => _goalCell;
        set
        {
            _goalCell = value;
            // Reiniciar las heurísticas aprendidas de todas las celdas al cambiar la meta
            grid.ResetHeuristics();
        }
    }
    public GridCell procedureLRTA(GridCell startCell)
    {
        GridCell currentCell = startCell;
        HashSet<GridCell> espacioBusqueda;
        if (currentCell != GoalCell)
        {
            espacioBusqueda = getEspacioBusqueda(currentCell, GoalCell, 1);

            //algoritmo 11.2 (calcula la heuristica de las celdas del espacio de búsqueda)
            ValueUpdateStep(currentCell, GoalCell, espacioBusqueda);

            currentCell = GetNextStepLRTA(currentCell);
            if (currentCell == null)
            {
                Debug.LogWarning("No se encontró un camino válido.");
                return null;
            }
            while (espacioBusqueda.Contains(currentCell))
            {
                currentCell = GetNextStepLRTA(currentCell);
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
        HashSet<GridCell> espacioBusqueda = new HashSet<GridCell>
        {
            // Agregar la celda actual al espacio de búsqueda
            currentCell
        };

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
            oldValues.Add((cell, getCellHeuristicOrDefault(cell)));
            cell.learnedHeuristic = float.MaxValue;
            infinitos.Add(cell);
        }

        while (infinitos.Any())
        {
            GridCell actual;
            float new_value;
            (actual, new_value) = argMinMax(infinitos, oldValues, goalCell);
            infinitos.Remove(actual);
            actual.learnedHeuristic = new_value;

            if (actual.learnedHeuristic == float.MaxValue)
            {
                Debug.LogWarning("No se encontró un camino válido.");
                return;
            }
        }
    }

    private (GridCell, float) argMinMax(List<GridCell> infinitos, List<(GridCell, float)> oldValues, GridCell goalCell)
    {
        float minValue = float.MaxValue;
        GridCell minCell = null;

        foreach (GridCell cell in infinitos)
        {
            float oldValue = oldValues.FirstOrDefault(x => x.Item1 == cell).Item2;

            float bestCost = float.MaxValue;
            grid.GetNeighbors(cell).ForEach(neighbor =>
            {
                // Nota: En pimera parte esto no es necesario porque todos
                // los costos son 1, pero en la segunda necesitamos calcular
                // el costo según el terreno. Así que lo dejo.
                float currentCost = cell.cost + getCellHeuristicOrDefault(neighbor);
                if (currentCost < bestCost)
                {
                    bestCost = currentCost;
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

    public GridCell GetNextStepLRTA(GridCell currentCell)
    {
        // Si ya estamos en la meta, no nos movemos
        //if (currentCell == goalCell) return currentCell;

        GridCell bestNextCell = null;
        float minF = float.MaxValue;


        // 1. EVALUAR VECINOS 
        foreach (GridCell neighbor in grid.GetNeighbors(currentCell))
        {
            // if (neighbor.isOccupied) continue;

            // Si el vecino nunca ha sido visitado/evaluado, calculamos su heurística inicial (Distancia Manhattan)

            // f(vecino) = coste de ir al vecino (w) + heurística del vecino (h)
            // Aquí usamos neighbor.cost como la 'w' de la fórmula
            float f = currentCell.cost + getCellHeuristicOrDefault(neighbor);

            // Buscamos el vecino con la f más pequeña (argmin)
            if (f < minF)
            {
                minF = f;
                bestNextCell = neighbor;
            }
        }

        // 2. APRENDIZAJE: Actualizar la heurística de la celda actual
        getCellHeuristicOrDefault(currentCell);

        // h(u) = max(h(u), f_minimo)
        // Esto no debería estar aquí, pero si lo comento peta 
        // currentCell.learnedHeuristic = Mathf.Max(currentCell.learnedHeuristic, minF);
        //bestNextCell.learnedHeuristic = minF;

        // 3. RETORNAR RESULTADO
        return bestNextCell;
    }

    public float getCellHeuristicOrDefault(GridCell cell)
    {
        if (cell.learnedHeuristic < 0)
        {
            cell.learnedHeuristic = CalculateManhattanHeuristic(cell);
        }
        // Devuelve el valor aprendido o recién calculado
        return cell.learnedHeuristic;
    }


    // Calculamos la distancia Manhattan pura y dura
    private float CalculateManhattanHeuristic(GridCell a)
    {
        if (GoalCell == null)
        {
            Debug.LogError("GoalCell no está asignada. No se puede calcular la heurística.");
            return float.MaxValue;
        }

        return Mathf.Abs(a.gridPosition.x - GoalCell.gridPosition.x) + Mathf.Abs(a.gridPosition.y - GoalCell.gridPosition.y);
    }
}