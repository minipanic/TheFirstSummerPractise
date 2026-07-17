using System;
using System.Collections;
using System.Collections.Generic;



public class SmartStack<T> : IEnumerable<T>, IEnumerable
{
    private int _count;
    private T[] _array;

    public int Count => _count;
    public int Capacity => _array.Length; 

    public SmartStack()
    {
        _array = new T[4];
        _count = 0;
    }


    public SmartStack(int capacity)
    {
        if (capacity < 0)
            throw new ArgumentOutOfRangeException(nameof(capacity), "Емкость не может быть отрицательной.");
        _array = new T[capacity];
        _count = 0;
    }


    public SmartStack (IEnumerable<T> collection)
    {
        if (collection == null)
            throw new ArgumentNullException(nameof(collection));

        int size = 0;
        foreach (var _ in collection) size++;
        
        _array = new T[size];
        _count = 0;

        foreach (var item in collection)
            _array[_count++] = item;
    }

    public void Push(T item)
    {
        if (_count == _array.Length)
            Resize(_array.Length == 0 ? 4 : _array.Length * 2);
        _array[_count++] = item;
    }

    public void PushRange(IEnumerable<T> collection)
    {
        if (collection == null)
            throw new ArgumentNullException(nameof(collection));
        
        int addedCount = 0;
        foreach (var _ in collection) addedCount++;

        if (addedCount == 0) return;

        int requiredCapacity = _count + addedCount;
        if (requiredCapacity > _array.Length)
        {
            int newCapacity = _array.Length == 0 ? 4 : _array.Length;
            while (newCapacity < requiredCapacity)
                newCapacity *= 2;
            Resize(newCapacity);
        }

        foreach (var item in collection)
            _array[_count++] = item;
    }

    public T Pop()
    {
        if (_count == 0)
            throw new InvalidOperationException("Стек пуст.");

        _count--;
        T item = _array[_count];
        _array[_count] = default; 
        return item;
    }

    public T Peek()
    {
        if (_count == 0)
            throw new InvalidOperationException("Стек пуст.");
        
        return _array[_count - 1];
    }

    public bool Contains(T item)
    {
        var comparer = EqualityComparer<T>.Default;
        for (int i = 0; i < _count; i++)
        {
            if (comparer.Equals(_array[i], item))
                return true;
        }
        return false;
    }

    public IEnumerator<T> GetEnumerator()
    {
        for (int i = _count - 1; i >= 0; i--)
        {
            yield return _array[i];
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    private void Resize(int newCapacity)
    {
        T[] newArray = new T[newCapacity];
        Array.Copy(_array, newArray, _count);
        _array = newArray;
    }
}