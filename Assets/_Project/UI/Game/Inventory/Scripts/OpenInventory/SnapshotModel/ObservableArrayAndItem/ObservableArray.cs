using System;

public interface IObservableArray<T>
{
    // get array of T type 
    // Declaration of event
    event Action<T[]> AnyValueChanged;

    // can read but can't write property
    int Count { get; } 
    
    // indexator
    T this[int index] { get; }

    void Swap(int index1, int index2);
    void Clear();
    bool TryAdd(T item);
    bool TryRemove(T item);
}