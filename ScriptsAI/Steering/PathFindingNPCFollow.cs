using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFindingNPCFollow : PathFollowing
{
    public Transform _objective;
    public Grid gameGrid;
    public int localAreaRadius = 1;
    private LRTAStar _pathManager;
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

    private void Awake()
    {
        this.nameSteering = "PathFindingLRTAStar";
    }

    private void Start()
    {
        if (_objective == null)
        {
            Debug.LogError("No se ha asignado un objetivo para el PathFindingFollow.");
            return;
        }

        if (gameGrid == null)
        {
            Debug.LogError("No se ha asignado un Grid para el PathFindingFollow.");
            return;
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
    }

    public void SetUpObjective()
    {
        if (_pathManager == null) _pathManager = GetComponent<LRTAStar>();
        if (_pathManager.grid == null) _pathManager.grid = gameGrid;
        var objectiveCell = gameGrid.GetGridPosition(_objective.position);
        if (gameGrid.PosicionValida((int) objectiveCell.x, (int)objectiveCell.y))
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
        if (!_pathSearched)
        {
            StartCoroutine(_pathManager.FindPath(gameGrid.GetCellAt(character.Position), localAreaRadius, _path));
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

}
