using Godot;
using System;
using System.Collections.Generic;

namespace Chomp.Pool;

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

    public T GetItem(out bool isRecycled, bool dontOverSpawn = true) {
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
            if (maxSize == -1 || list.Count < maxSize) {
                isRecycled = true;
                container = CreateConatiner();
            }
            else if (dontOverSpawn) {
                isRecycled = true;
                return null;
            }
            else {
                T item = factoryFunc();
                GD.Print("Warning: Object Pool is at max size and no objects are available. Instancing non-pooled object: ", item);
                return item;
            }
        }
        container.Consume();
        lookup.Add(container.Item, container);
        return container.Item;
    }

    public void AddItem(T item) {
        if (lookup.ContainsKey(item))
            GD.PrintErr("Object pool add failed: This object pool already contains the item provided: " + item);
        else if (maxSize == -1 || list.Count < maxSize) {
            var container = CreateConatiner();
            container.Item = item;
        }
        else
            GD.Print("Warning: Object pool add failed because this object pool is at max size: ", item);
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
            GD.PrintErr("Object pool release failed: This object pool does not contain the item provided: ", item);
    }
}
