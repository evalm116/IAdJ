using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class PathFindingTactical : PathFollowing
{
    [Header("PathFinding Settings")]
    public Transform _objective;
    public Grid gameGrid;
    public int localAreaRadius = 1;
    private AStarTactical _pathManager;
    private bool _pathSearched = false;

    public Transform Objective
    {
        get { return _objective; }
        set
        {
            _objective = value;
            SetUpObjective();
        }
    }

    public Vector3 ObjectivePosition
    {
        get { return _objective.position; }
        set
        {
            if (_objective == null)
            {
                GameObject dummy = new GameObject("DummyObjective");
                _objective = dummy.transform;
            }
            _objective.position = value;
            SetUpObjective();
        }
    }

    public bool Finished
    {
        get { return finished; }
    }

    private void Awake()
    {
        this.nameSteering = "PathFindingTactical";
    }

    private void Start()
    {
        if (_objective == null)
        {
            GameObject dummy = new GameObject("DummyObjectiveTarget");
            dummy.AddComponent<Agent>();
            _objective = dummy.transform;
            _objective.position = GameManager.Instance.GetInitialPosition(GetComponent<Unit>());
        }

        if (gameGrid == null)
        {
            gameGrid = GameManager.Instance.GameGrid;
            if (gameGrid == null)
            {
                Debug.LogError("No se ha asignado un Grid para el PathFindingFollow.");
                return;
            }
        }

        if (target == null)
        {
            GameObject dummy = new GameObject("DummyPathTarget");
            target = dummy.AddComponent<Agent>();
        }

        // Si existe un face, hacemos que su target sea el mismo que el de este PathFollowing
        if (TryGetComponent<Face>(out var faceBehavior))
        {
            faceBehavior.target = this.target;
        }

        SetUpObjective();

        Unit unit = null;
        gameObject.TryGetComponent<Unit>(out unit);
        if (unit == null)
        {
            Debug.LogError("Unidad no encontrada");
            return;
        }
        _pathManager.UnitType = unit.type;

    }

    public void SetUpObjective()
    {
        if (_pathManager == null) _pathManager = gameObject.GetComponent<AStarTactical>();
        if (_pathManager.grid == null) _pathManager.grid = gameGrid;
        var objectiveCell = gameGrid.GetGridPosition(_objective.position);
        if (gameGrid.PosicionValida((int)objectiveCell.x, (int)objectiveCell.y))
            _pathManager.GoalCell = gameGrid.GetCellAt(_objective.position);
        else
        {
            Debug.LogError("El objetivo está fuera del Grid.");
            return;
        }

        if (_path == null) _path = new Path();
        else _path.ResetNodes();

        finished = false;
        _pathSearched = false;
    }

    public override Steering GetSteering(AgentNPC character)
    {
        if (_objective == null || !gameGrid.IsInside(_objective.position))
        {
            Debug.LogError("Objetivo no puesto o fuera del grid");
            return null;
        }
        if (!_pathSearched || _path == null)
        {
            _path = _pathManager.FindPath(gameGrid.GetCellAt(character.Position));
            _pathSearched = true;
        }

        if (!_pathManager.CaminoValido)
        {
            character.StopMoving();
            return new Steering();
        }

        if (_objective == null || gameGrid == null) return new Steering();
        // Por si se ha terminado el path, pero se han añadido nodos al path después
        if (finished && _currentParam < _path.GetLength()) finished = false;
        return base.GetSteering(character);
    }

    public void ForceRepath()
    {
        _pathSearched = false;
        finished = false;
    }
}
