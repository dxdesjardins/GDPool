GDPool
=================
Object Pooling implementation for the Godot Game Engine.

Overview
----
An object pool provides an efficient way to reuse objects, and thus keep the memory foot print of all dynamically created objects within fixed bounds. This is crucial for maintianing consistent framerates in realtime games.

Setup
----
No setup is required. A PoolManager Instance will automatically be created and added to the scene if called and one did not exist. You may optionally choose to add the PoolManager Class as an Autoload (recommended).

Usage
----
This project consists of:

1. A PoolManager Singleton Class that allows you to easily pool scene objects. The Singleton will be automatically instantiated and added to the root node of the project if it does not exist when called.

2. A generic ObjectPool class that can be used for non-Godot objects.

3. A ReturnToPool Node Class that will be added as a child of the scene being pooled.

When a pooled object is instantiated, a ReturnToPool child component will be automatically added to it. This component will ensure that the object is returned back to the pool after being removed from the scene tree or during OnReturnObjectsToPool.Invoke()

Freeing a pooled object (via Free or QueueFree) will cause errors. Call OnReturnObjectsToPool.Invoke() before freeing a scene to prevent this error.

Use Examples
----
```csharp
// Initialize a pool of ten sceneObjects. The pool will have a maximum size of 15 objects (setting a max size is optional).
PoolManager.Instance.WarmObjects(packedSceneObject, 10, 15);

// Spawning a Pool Object at GlobalPosition (0,0) with a rotation of 0 Radians.
PoolManager.Instance.SpawnObject(packedSceneObject, someParentNode, Vector2.Zero, 0f);

// Configuring a Pool Scene child component before adding it to the SceneTree.
Node2D poolScene = PoolManager.Instance.GetObject(packedSceneObject, out bool isRecycled) as Node2D;
Component component = poolScene.GetChild<Component>();
component.Configure(/* Do configuration of your custom script component here */);
someParentNode.AddChild(poolScene);

// Removing all pooled objects from the SceneTree and returning them to their Object Pools (Ex: useful before QueueFreeing a Scene)
PoolManager.Instance.OnReturnObjectsToPool.Invoke();
```

Background
---
This Pool Pool is a standalone version with slight modifications to make it function independantly from my larger GDEssentials repository.

Licence
---
MIT
