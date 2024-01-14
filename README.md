GDPool
=================
Object Pool implementation for the Godot Game Engine.

Overview
----
An object pool provides an efficient way to reuse objects, and thus keep the memory foot print of all dynamically created objects within fixed bounds. This is crucial for maintianing consistent framerates in realtime games (especially on mobile).

Usage
----
This project consists of:

1. A PoolManager Singleton Class that allows you to easily pool scene objects. The Singleton will be automatically instantiated and added to the root node of the project if it does not exist when called.

2. A ReturnToPool Node Class that must be a child of root in the scene being pooled.

3. A generic object pool collection that can be used for non Godot objects.

Use Examples
----
```csharp
// Initialize a pool of ten sceneObjects. The pool will have a maximum size of 15 objects (setting a max size is optional).
PoolManager.WarmObjects(packedSceneObject, 10, 15);

// Spawning a Pool Object at GlobalPosition (0,0) with a rotation of 0 Radians.
PoolManager.SpawnObject(packedSceneObject, someParentNode, Vector2.Zero, 0f);

// Configuring a Pool Scene child component before adding it to the SceneTree.
Node2D poolScene = PoolManager.GetObject(packedSceneObject, out bool isRecycled) as Node2D;
Component component = poolScene.GetChild<Component>();
component.Configure(/* Do configuration of your custom script component here */);
someParentNode.AddChild(poolScene);
```

History
----
This project is inspired by and a reimplementation of a Unity Engine Object Pool by:
https://github.com/thefuntastic

Licence
---
MIT
