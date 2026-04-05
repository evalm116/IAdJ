using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if NET6_0_OR_GREATER
using System.Collections.Generic;
#else
// Nota: PriorityQueue está en .NET 6.0, pero Unity no lo tiene todavía
// Así que hemos implementado una versión simple de PriorityQueue usando SortedDictionary y Queue
public class PriorityQueue<TElement, TPriority>
{
    private readonly SortedDictionary<TPriority, Queue<TElement>> _dict = new SortedDictionary<TPriority, Queue<TElement>>();

    public int Count { get; private set; }

    public void Enqueue(TElement element, TPriority priority)
    {
        if (!_dict.TryGetValue(priority, out var queue))
        {
            queue = new Queue<TElement>();
            _dict.Add(priority, queue);
        }
        queue.Enqueue(element);
        Count++;
    }

    public TElement Dequeue()
    {
        if (Count == 0) throw new System.InvalidOperationException("Queue is empty");
        var pair = _dict.First();
        var element = pair.Value.Dequeue();
        if (pair.Value.Count == 0) _dict.Remove(pair.Key);
        Count--;
        return element;
    }

    public bool TryDequeue(out TElement element, out TPriority priority)
    {
        if (Count == 0)
        {
            element = default;
            priority = default;
            return false;
        }
        var pair = _dict.First();
        element = pair.Value.Dequeue();
        priority = pair.Key;
        if (pair.Value.Count == 0) _dict.Remove(pair.Key);
        Count--;
        return true;
    }

    public bool Contains(TElement element)
        {
            return _dict.Values.Any(queue => queue.Contains(element));
    }
}
#endif

public class AStartBasic : PathFindingAlgorithm
{
    public GridCell[,] parentGrid;

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
            if (GetCellHeuristicSafe(current) + current.cost < GetCellHeuristicSafe(neighbor) - CalculateHeuristic(neighbor))
            {
                SetParent(neighbor, current);
                SetGridHeuristic(neighbor, GetCellHeuristicSafe(current) + current.cost + CalculateHeuristic(neighbor));                
            }
        }
        else if (closed.Contains(neighbor))
        {
            if (GetCellHeuristicSafe(current) + current.cost < GetCellHeuristicSafe(neighbor))
            {
                SetParent(neighbor, current);
                SetGridHeuristic(neighbor, GetCellHeuristicSafe(current) + current.cost + CalculateHeuristic(neighbor));
                closed.Remove(neighbor);
                open.Enqueue(neighbor, GetCellHeuristicSafe(neighbor));
            }
        }
        else
        {
            SetParent(neighbor, current);
            SetGridHeuristic(neighbor, GetCellHeuristicSafe(current) +
                current.cost + CalculateHeuristic(neighbor));
            open.Enqueue(neighbor, GetCellHeuristicSafe(neighbor));
        }        
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
