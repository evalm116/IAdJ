using System.Collections.Generic;
using UnityEngine;

public class PathfindingManager : MonoBehaviour
{
    public Grid gameGrid;

    public GridCell GetNextStepLRTA(GridCell currentCell, GridCell goalCell)
    {
        // Si ya estamos en la meta, no nos movemos
        if (currentCell == goalCell) return currentCell;

        GridCell bestNextCell = null;
        float minF = float.MaxValue;

        List<GridCell> neighbors = gameGrid.GetNeighbors(currentCell);

        // 1. EVALUAR VECINOS (Lookahead-One)
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