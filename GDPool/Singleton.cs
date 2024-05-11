using Godot;
using System;
using System.Collections.Generic;

namespace Lambchomp.Pool;

public abstract partial class Singleton<T> : Node2D where T : Singleton<T>
{
    private static T instance;
    protected static bool addToTree = true;

    public static T Instance {
        get {
            if (instance == null)
                CreateInstance();
            return instance;
        }
        set { instance = value as T; }
    }

    public Singleton() {
        if (instance == null)
            instance = this as T;
        else {
            GD.PrintErr("Class " + typeof(T).Name + " exists multiple times in violation of singleton pattern. Destroying copy.");
            this.QueueFree();
        }
    }

    protected static T CreateInstance() {
        Window root = (Engine.GetMainLoop() as SceneTree).Root;
        GD.Print("Searching for instance of: ", typeof(T).Name);
        T[] singletons = addToTree ? root.GetComponentsInChildren<T>(false) : Array.Empty<T>();
        if (singletons.Length != 0) {
            if (singletons.Length == 1) {
                instance = singletons[0];
                instance.Name = typeof(T).Name;
                return instance;
            }
            else {
                GD.PrintErr("Class " + typeof(T).Name + " exists multiple times in violation of singleton pattern. Destroying all copies.");
                foreach (T singleton in singletons)
                    singleton.QueueFree();
            }
        }
        T _instance = (T)Activator.CreateInstance(typeof(T));
        _instance.Name = typeof(T).Name;
        if (addToTree)
            root.SafeAddChild(_instance);
        instance = _instance;
        return instance;
    }
}
