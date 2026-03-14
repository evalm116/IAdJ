using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Path
{
    [SerializeField] protected List<Vector3> _pathNodes;
    [SerializeField] protected List<float> _cummulativeLength;
    [SerializeField] protected float _totalLength;

    public List<Vector3> PathNodes
    {
        get { return _pathNodes; }
        set { _pathNodes = value; SetUpCummulative(); }
    }

    public float TotalLength
    {
        get { return _totalLength; }
    }

    public Path()
    {
        ResetNodes();
    }

    private void SetUpCummulative()
    {
        _cummulativeLength = new List<float>();
        _totalLength = 0;
        for (int i = 0; i < _pathNodes.Count - 1; i++)
        {
            float segmentLength = Vector3.Distance(_pathNodes[i], _pathNodes[i + 1]);
            _totalLength += segmentLength;
            _cummulativeLength.Add(_totalLength);
        }
    }

    public float getParam(Vector3 position, float lastParam)
    {
        if (_pathNodes == null || _pathNodes.Count == 0) return 0f;
        if (_cummulativeLength == null || _cummulativeLength.Count == 0) SetUpCummulative();

        lastParam = Mathf.Clamp(lastParam, 0f, _totalLength);

        float bestParam = lastParam;
        Vector3 bestPoint = getPosition(bestParam);
        float bestDistSq = (position - bestPoint).sqrMagnitude;

       
        for (int i = 0; i < _pathNodes.Count - 1; i++)
        {
            float segmentStart = (i == 0) ? 0f : _cummulativeLength[i - 1];
            float segmentEnd = _cummulativeLength[i];
            float segmentLength = segmentEnd - segmentStart;

            if (segmentLength <= Mathf.Epsilon) continue;


            if (segmentEnd < lastParam) continue;

            Vector3 a = _pathNodes[i];
            Vector3 b = _pathNodes[i + 1];
            Vector3 ab = b - a;

            float tUnclamped = Vector3.Dot(position - a, ab) / Vector3.Dot(ab, ab);

            float tMin = 0f;
            if (lastParam > segmentStart)
            {
                tMin = Mathf.Clamp01((lastParam - segmentStart) / segmentLength);
            }

            float t = Mathf.Clamp(tUnclamped, tMin, 1f);

            Vector3 candidatePoint = Vector3.Lerp(a, b, t);
            float distSq = (position - candidatePoint).sqrMagnitude;
            if (distSq < bestDistSq)
            {
                bestDistSq = distSq;
                bestParam = segmentStart + t * segmentLength;
            }
        }

        bestParam = Mathf.Clamp(bestParam, 0f, _totalLength);
        return bestParam;
    }

    public Vector3 getPosition(float param)
    {
        if (_pathNodes == null || _pathNodes.Count == 0) return Vector3.zero;
        if (_cummulativeLength == null || _cummulativeLength.Count == 0) SetUpCummulative();

        if (param <= 0f) return _pathNodes[0];
        if (param >= _totalLength) return _pathNodes[_pathNodes.Count - 1];

        for (int i = 0; i < _cummulativeLength.Count; i++)
        {
            if (param < _cummulativeLength[i])
            {
                float segmentStart = i == 0 ? 0f : _cummulativeLength[i - 1];
                float segmentEnd = _cummulativeLength[i];
                float t = (param - segmentStart) / (segmentEnd - segmentStart);
                return Vector3.Lerp(_pathNodes[i], _pathNodes[i + 1], t);
            }
        }
        return Vector3.zero; // Esto es ejecución erronea
    }

    public Vector3 getLastPosition()
    {
        if (_pathNodes == null || _pathNodes.Count == 0) return Vector3.zero;
        return _pathNodes[_pathNodes.Count - 1];
    }

    public Vector3 getFirstPosition()
    {
        if (_pathNodes == null || _pathNodes.Count == 0) return Vector3.zero;
        return _pathNodes[0];
    }

    public void AddNode(Vector3 node)
    {
        _pathNodes.Add(node);
        if (_pathNodes.Count > 1) { 
            int index = _pathNodes.Count - 2;
            float segmentLength = Vector3.Distance(_pathNodes[index], _pathNodes[index + 1]);
            _totalLength += segmentLength;
            _cummulativeLength.Add(_totalLength);
        }
    }

    internal void ResetNodes()
    {
        _pathNodes = new List<Vector3>();
        _cummulativeLength = new List<float>();
        _totalLength = 0f;
    }

    public float GetLength() {
        return _totalLength;
    }
}
