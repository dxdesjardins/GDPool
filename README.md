GDPool
=================
Scene Pooling System for the Godot Game Engine.

Overview
----
An object pool improves performance by reusing a set of objects rather than frequently allocating and deallocating memory for new ones. This is crucial for maintianing consistent framerate in games.

Core Features
----
1. A PoolManager Singleton class that allows you to easily pool scene objects. The Singleton will be automatically instantiated and added to the root node of the game if it does not exist when called. You may optionally set the PoolManager to be an autoload.
2. A generic ObjectPool class that can be used for non-Godot objects.
3. A ReturnToPool Node that will be added as a child of the scene being pooled. This will communicate with my StageManager to automatically remove and return all pooled scenes from a Stage being unloaded or freed.

Requirements
----
1. Godot .NET v4.3+
2. .NET 8+
3. Install my GDEssentials repository and follow the setup instructions to use my StageManager.

Use Examples
----
```csharp
// Initialize a pool of ten scenes. The pool will have a maximum size of 15 scenes.
// Setting a max size is optional. If not specified, the pool size will grow when needed.
// Setting a max size of 1 is useful for User Interfaces.
PoolManager.WarmPool(packedScene, 10, 15);

// Examples of spawning a Pooled Scene.
PoolManager.Spawn(packedScene, parentNode);
PoolManager.Spawn(stringPackedSceneUid, parentNode);
PoolManager.Spawn(longPackedSceneUid, parentNode);
PoolManager.Spawn(packedScene, parentNode);
PoolManager.Spawn(packedScene, parentNode, vector2Position, floatRotation, out isRecycled, dontOverSpawn = true);
PoolManager.Spawn(packedScene, parentNode, vector3Position, Vector3Rotation, out isRecycled, dontOverSpawn = true);
PoolManager.Spawn(packedScene, parentNode, transform2D, out isRecycled, dontOverSpawn = true);
PoolManager.Spawn(packedScene, parentNode, transform3D, out isRecycled, dontOverSpawn = true);

// Configuring a Pooled Scene child component before adding the scene to the tree.
Node pooledScene = PoolManager.GetObject(packedScene);
Component component = poolScene.GetComponent<Component>();
component.Configure(/* Do configuration of your custom script component here */);
parentNode.AddChild(pooledScene);

// Manually removing all pooled scenes from a specified Stage and returning them to their Object Pools.
PoolManager.ReturnObjectsToPool.Invoke(stage);

// Printing debug info on the size and utilization of all pools.
PoolManager.PrintStatus();
```
