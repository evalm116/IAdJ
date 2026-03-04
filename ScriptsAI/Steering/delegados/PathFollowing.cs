using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFollowing : Seek
{
    protected Path _path;
    [SerializeField] protected GameObject _pathParent;
    [SerializeField] protected float _pathOffset;
    private float _currentParam;

    [SerializeField] public bool loop;
    protected bool finished;

    public GameObject PathParent
    {
        get { return _pathParent; }
        set { _pathParent = value;  SetUpPath(); }
    }
    
    public float PathOffset
    {
        get { return _pathOffset; }
        set { _pathOffset = Mathf.Max(value, 0f); }
    }

    private void SetUpPath()
    {
        if (_pathParent == null) return;
        if (_path == null) _path = new Path();

        List<Vector3> nodes = new List<Vector3>();
        foreach (Transform child in _pathParent.transform)
        {
            nodes.Add(child.position);
        }
        _path.PathNodes = nodes;
    }

    private void Awake()
    {
        this.nameSteering = "PathFollowing";
    }

    private void Start()
    {
        SetUpPath();
        if (target == null)
        {
            GameObject dummy = new GameObject("DummyPathTarget");
            target = dummy.AddComponent<Agent>();
        }

        // Si existe un face, hacemos que su target sea el mismo que el de este PathFollowing
        Face faceBehavior = GetComponent<Face>();
        if (faceBehavior != null)
        {
            faceBehavior.target = this.target;
        }

    }

    public override Steering GetSteering(AgentNPC character)
    {
        if (finished && !loop) return new Steering();

        _currentParam = _path.getParam(character.Position, _currentParam);

        float targetParam = _currentParam + _pathOffset;

        target.Position = _path.getPosition(targetParam);

        // Control de Finalización y Looping
        if (loop && !finished && targetParam >= _path.TotalLength)
        {
            finished = true;
            target.Position = _path.getFirstPosition();
            _currentParam = 0f;
        }

        if (loop && finished)
        {
            target.Position = _path.getFirstPosition();
            if (Vector3.Distance(character.Position, _path.getFirstPosition()) <= 0.5f)
            {
                finished = false;
            }
        }

        if (!loop && !finished && Vector3.Distance(character.Position, _path.getLastPosition()) <= 0.5f)
        {
            finished = true;
            character.StopMoving();
            return new Steering();
        }
        return base.GetSteering(character);
    }

    public void OnDestroy()
    {
        if (target != null)
            Destroy(target.gameObject);
    }
}

