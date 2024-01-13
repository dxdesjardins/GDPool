using Godot;
using System;
using System.Collections.Generic;

namespace Lambchomp.Essentials;

public class ObjectPool<T> where T : class
{
    private List<ObjectPoolContainer<T>> list;
    private Dictionary<T, ObjectPoolContainer<T>> lookup;
    private Func<T> factoryFunc;
    private int lastIndex = 0;
    private int maxSize = -1;

    public int Count => list.Count;
    public int CountUsedItems => lookup.Count;
    public int MaxSize => maxSize;

    public ObjectPool(Func<T> factoryFunc, int initialSize, int maxSize = -1) {
        this.factoryFunc = factoryFunc;
        list = new List<ObjectPoolContainer<T>>(initialSize);
        lookup = new Dictionary<T, ObjectPoolContainer<T>>(initialSize);
        Warm(initialSize, maxSize);
    }

    private void Warm(int amount, int maxSize = -1) {
        this.maxSize = maxSize;
        for (int i = 0; i < amount; i++)
            CreateConatiner();
    }

    private ObjectPoolContainer<T> CreateConatiner() {
        var container = new ObjectPoolContainer<T>();
        container.Item = factoryFunc();
        list.Add(container);
        return container;
    }

    public T GetItem(out bool isRecycled) {
        isRecycled = false;
        ObjectPoolContainer<T> container = null;
        for (int i = 0; i < list.Count; i++) {
            lastIndex++;
            if (lastIndex > list.Count - 1) lastIndex = 0;
            if (list[lastIndex].Used)
                continue;
            else {
                if (list[lastIndex].Item as Node != null) {
                    isRecycled = true;
                    container = list[lastIndex];
                    break;
                }
                else {
                    list.RemoveAt(lastIndex);
                    continue;
                }
            }
        }
        if (container == null) {
            if (list.Count < maxSize || maxSize == -1)
                container = CreateConatiner();
            else
                return factoryFunc();
        }
        container.Consume();
        lookup.Add(container.Item, container);
        return container.Item;
    }

    public void AddItem(T item) {
        if (lookup.ContainsKey(item))
            GD.PrintErr("This object pool already contains the item provided: " + item);
        else if (maxSize == -1 || list.Count < maxSize) {
            var container = CreateConatiner();
            container.Item = item;
        }
    }

    public void ReleaseItem(object item) {
        ReleaseItem((T)item);
    }

    public void ReleaseItem(T item) {
        if (lookup.ContainsKey(item)) {
            var container = lookup[item];
            container.Release();
            lookup.Remove(item);
        }
        else
            GD.PrintErr("This object pool does not contain the item provided: " + item);
    }
}
