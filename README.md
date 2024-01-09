GDPool
=================

Object Pool Manager for the Godot Game Engine.
Written for 2D games; slight modifications will be needed for 3D games.

Reimplemented and improved upon upon from a Unity Engine MIT Project:
https://github.com/thefuntastic/unity-object-pool

```csharp
// Initialize a pool of ten sceneObjects. The pool will have a maximum size of 15 objects (setting a max pool size is optional).
// If a PoolManager node does not exist in the SceneTree, one will be created and added to the root node automatically.
PoolManager.WarmObjects(packedSceneObject, 10, 15);

// Simple example of spawning a Pool Object at GlobalPosition (0,0) with a rotation of 0 Radians.
PoolManager.SpawnObject(packedSceneObject, someParentNode, Vector2.Zero, 0f, out bool isRecycled);

// Example of configuring a Pool Object's custom component before adding it to the SceneTree.
Node2D scene = PoolManager.GetObject(packedSceneObject, out bool isRecycled) as Node2D;
Component component = scene.GetChild<Component>();
component.Configure(/* Do configuration of your custom component here */);
someParentNode.AddChild(scene);

```
