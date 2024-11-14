using Godot;
using System;
using System.Collections.Generic;
using Chomp.Essentials;

namespace Chomp.Pool;

public class ObjectPoolContainer<T> where T : class
{
    private T item;
    public bool Used { get; private set; }
    public T Item { get { return item; } set { item = value; } }

    public void Consume() {
        Used = true;
    }

    public void Release() {
        Used = false;
    }
}
