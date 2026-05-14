using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AStartTactical : PathFindingAlgorithm
{
    public GridCell[,] parentGrid;
    public float[,] terrainCosts;
    private Unit.Type _unitType;
    public Unit.Type UnitType
    {
        get => _unitType;
        set
        {
            _unitType = value;
            terrainCosts = GetTerrainCostsForUnitType(_unitType);
        }
    }

    private float[,] GetTerrainCostsForUnitType(Unit.Type unitType)
    {
        float[,] costs = new float[grid.columnas, grid.filas];
        for (int i = 0; i < grid.columnas; i++)
        {
            for (int j = 0; j < grid.filas; j++)
            {
                costs[i, j] = 1 / (float)Modifier.GetInstance().getMovementModifier(_unitType, grid.GetCellAt(i, j).terrainType);
            }
        }
        return null;
    }

    public Path FindPath(GridCell startCell)
    {
        parentGrid = new GridCell[grid.columnas, grid.filas];
        ResetParents();


        List<GridCell> closed = new List<GridCell>();
        var open = new PriorityQueue<GridCell, float>();
        open.Enqueue(startCell, this.GetCellHeuristicSafe(startCell));

        while (open.Count > 0)
        {
            open.TryDequeue(out var current, out _);
            closed.Add(current);
            if (current == this.GoalCell)
                return this.ReconstructPath();
            else
            {
                foreach (var neighbor in grid.GetNeighbors(current))
                {
                    Improve(current, neighbor, open, closed);
                }
            }
        }

        return null;
    }

    public Path FindPath(GridCell startCell, Unit.Type unitType)
    {
        this.UnitType = unitType;
        return FindPath(startCell);
    }
    public Path FindPath(GridCell startCell, GridCell endCell, Unit.Type unitType)
    {
        this._goalCell = endCell;
        this.UnitType = unitType;
        return FindPath(startCell);
    }

    public Path FindPath(GridCell startCell, GridCell endCell)
    {
        this._goalCell = endCell;
        return FindPath(startCell);
    }


    private void ResetParents()
    {
        for (int x = 0; x < grid.columnas; x++)
        {
            for (int y = 0; y < grid.filas; y++)
            {
                parentGrid[x, y] = null;
            }
        }
    }

    private void SetParent(GridCell child, GridCell parent)
    {
        parentGrid[child.gridPosition.x, child.gridPosition.y] = parent;
    }

    private GridCell GetParent(GridCell cell)
    {
        return parentGrid[cell.gridPosition.x, cell.gridPosition.y];
    }

    private void Improve(GridCell current, GridCell neighbor, PriorityQueue<GridCell, float> open, List<GridCell> closed)
    {
        if (open.Contains(neighbor))
        {
            if (GetCellHeuristicSafe(current) + terrainCosts[current.gridPosition.x, current.gridPosition.y] < GetCellHeuristicSafe(neighbor) - CalculateHeuristic(neighbor))
            {
                SetParent(neighbor, current);
                SetGridHeuristic(neighbor, GetCellHeuristicSafe(current) + terrainCosts[current.gridPosition.x, current.gridPosition.y] + CalculateHeuristic(neighbor));
            }
        }
        else if (closed.Contains(neighbor))
        {
            if (GetCellHeuristicSafe(current) + terrainCosts[current.gridPosition.x, current.gridPosition.y] < GetCellHeuristicSafe(neighbor))
            {
                SetParent(neighbor, current);
                SetGridHeuristic(neighbor, GetCellHeuristicSafe(current) + terrainCosts[current.gridPosition.x, current.gridPosition.y] + CalculateHeuristic(neighbor));
                closed.Remove(neighbor);
                open.Enqueue(neighbor, GetCellHeuristicSafe(neighbor));
            }
        }
        else
        {
            SetParent(neighbor, current);
            SetGridHeuristic(neighbor, GetCellHeuristicSafe(current) +
                terrainCosts[current.gridPosition.x, current.gridPosition.y] + CalculateHeuristic(neighbor));
            open.Enqueue(neighbor, GetCellHeuristicSafe(neighbor));
        }
    }

    public GridCell GetNearestCellWithTerrain(GridCell currentCell, TipoTerreno[] targetTerrains, int maximoRango = 10)
    {
        // Nota: Maximo rango arbitrario para limitar busqueda
        // Busqueda en anchura para encontrar la celda más cercana con el terreno objetivo dentro del espacio de búsqueda

        Queue<(GridCell, int)> nodosBusquedas = new Queue<(GridCell, int)>();
        List<GridCell> espacioBusqueda = new List<GridCell>();

        nodosBusquedas.Enqueue((currentCell, 1));

        while (nodosBusquedas.Count > 0)
        {

            (GridCell nodo, int radio) = nodosBusquedas.Dequeue();
            espacioBusqueda.Add(nodo);
            List<GridCell> neighborCells = grid.GetNeighbors(nodo);
            foreach (GridCell neighbor in neighborCells)
            {
                if (!espacioBusqueda.Contains(neighbor))
                {
                    if (targetTerrains.Contains(neighbor.terrainType))
                        return neighbor;

                    espacioBusqueda.Add(neighbor);

                    if (radio < maximoRango)
                        nodosBusquedas.Enqueue((neighbor, radio + 1));
                }
            }
        }
        return null;
    }


    private Path ReconstructPath()
    {
        GridCell current = this.GoalCell;
        Path path = new Path();
        while (current != null)
        {
            path.AddNode(grid.GetCellCenter(current));
            current = GetParent(current);
        }
        path.Reverse();
        return path;
    }
}
