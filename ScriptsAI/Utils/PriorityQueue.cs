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