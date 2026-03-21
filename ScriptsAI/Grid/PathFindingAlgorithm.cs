using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PathFindingAlgorithm : MonoBehaviour
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
    protected GridCell _goalCell;
    protected bool _caminoValido = true;
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
    protected float CalculateHeuristic(GridCell currentCell)
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
    protected float CalculateManhattanHeuristic(GridCell current)
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
    protected float CalculateEuclideanHeuristic(GridCell current)
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
    protected float CalculateChebyshevHeuristic(GridCell current)
    {
        if (GoalCell == null)
        {
            Debug.LogError("GoalCell no está asignada. No se puede calcular la heurística.");
            return float.MaxValue;
        }
        return Mathf.Max(Mathf.Abs(current.gridPosition.x - GoalCell.gridPosition.x), Mathf.Abs(current.gridPosition.y - GoalCell.gridPosition.y));
    }


    protected void ResetHeuristics()
    {
        gridHeuristics ??= new float[grid.columnas, grid.filas];

        for (int x = 0; x < gridHeuristics.GetLength(0); x++)
        {
            for (int z = 0; z < gridHeuristics.GetLength(1); z++)
            {
                gridHeuristics[x, z] = -1.0f;
            }
        }
    }
}
