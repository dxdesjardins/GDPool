using Godot;
using System;
using System.Collections.Generic;

namespace Lambchomp.Essentials;

public partial class Singleton<T> : Node where T : Singleton<T>
{
    private static T instance;

    public static T Instance {
        get {
            if (instance == null) {
                Window root = (Godot.Engine.GetMainLoop() as SceneTree).Root;
                T[] managers = root.GetComponentsInChildren<T>(false);
                if (managers.Length != 0) {
                    if (managers.Length == 1) {
                        instance = managers[0];
                        instance.Name = typeof(T).Name;
                        return instance;
                    }
                    else {
                        GD.PrintErr("Class " + typeof(T).Name + " exists multiple times in violation of singleton pattern. Destroying all copies");
                        foreach (T manager in managers)
                            manager.QueueFree();
                    }
                }
                T _instance = (T)Activator.CreateInstance(typeof(T));
                root.SafeAddChild(_instance);
                instance = _instance;
            }
            return instance;
        }
        set { instance = value as T; }
    }
}
