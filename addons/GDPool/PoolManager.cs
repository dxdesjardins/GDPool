using Godot;
using System;
using System.Collections.Generic;
using Chomp.Essentials;

namespace Chomp.Pool;

public partial class PoolManager : NodeSingleton<PoolManager>
{
    [Export]
    public bool PrintObjectPoolStatus {
        get { return false; }
        set {
            if (value == true)
                PrintStatus();
        }
    }
    private static Dictionary<PackedScene, ObjectPool<Node>> packedLookup = new();
    private static Dictionary<Node, ObjectPool<Node>> instanceLookup = new();
    public static NodeEvent ReturnObjectsToPool { get; set; } = new();

    public override void _EnterTree() {
        RequestReady();
    }

    public override void _Ready() {
        StageManager.StageUnloading += ReturnObjectsToPool.Invoke;
    }

    public override void _ExitTree() {
        StageManager.StageUnloading -= ReturnObjectsToPool.Invoke;
    }

    public static void WarmPool(PackedScene packedScene, int size, int maxSize = -1) {
        if (packedLookup.ContainsKey(packedScene))
            GDE.LogErr($"Pool for Scene({System.IO.Path.GetFileNameWithoutExtension(packedScene.ResourcePath)}) has already been created.");
        ObjectPool<Node> pool = new(() => {
            Node instance = packedScene.Instantiate<Node>();
            instance.SetUniqueName();
            instance.InstantiateChild<ReturnToPool>();
            return instance;
        }, size, maxSize);
        packedLookup[packedScene] = pool;
    }

    public static Node GetObject(PackedScene packedScene, out bool isRecycled, bool dontOverSpawn = true) {
        bool createdPool = false;
        if (!packedLookup.ContainsKey(packedScene)) {
            WarmPool(packedScene, 1);
            createdPool = true;
        }
        var pool = packedLookup[packedScene];
        var clone = pool.GetItem(out isRecycled, dontOverSpawn);
        if (clone == null) {
            if (dontOverSpawn == false)
                GDE.Log($"Pool for PackedScene({System.IO.Path.GetFileNameWithoutExtension(packedScene.ResourcePath)}) is at maximum capacity and did not return a new object.");
            return null;
        }
        if (isRecycled)
            instanceLookup.Add(clone, pool);
        if (createdPool)
            isRecycled = false;
        return clone;
    }

    public static Node SpawnObject(PackedScene packedScene, Node parent, Vector2 position, float rotation, Vector2 scale, out bool isRecycled, bool dontOverSpawn = true) {
        Node clone = GetObject(packedScene, out isRecycled, dontOverSpawn);
        if (clone == null)
            return null;
        else if (clone is Node2D node2D) {
            node2D.Position = position;
            node2D.Rotation = rotation;
            node2D.Scale = scale;
        }
        else if (clone is Control control) {
            control.Position = position;
            control.Rotation = rotation;
            control.Scale = scale;
        }
        parent.AddChild(clone);
        return clone;
    }

    public static Node SpawnObject(PackedScene packedScene, Node parent, Transform2D transform, out bool isRecycled, bool dontOverSpawn = true) {
        Node clone = GetObject(packedScene, out isRecycled, dontOverSpawn);
        if (clone == null)
            return null;
        else if (clone is Node2D node2D) 
            node2D.Transform = transform;
        else if (clone is Control control)
            control.SetTransform(transform);
        parent.AddChild(clone);
        return clone;
    }

    public static Node SpawnObject(PackedScene packedScene, Node parent, Vector3 position, Vector3 rotation, Vector3 scale, out bool isRecycled, bool dontOverSpawn = true) {
        Node clone = GetObject(packedScene, out isRecycled, dontOverSpawn);
        if (clone == null)
            return null;
        else if (clone is Node3D node3D) {
            node3D.Position = position;
            node3D.Rotation = rotation;
        }
        parent.AddChild(clone);
        return clone;
    }

    public static Node SpawnObject(PackedScene packedScene, Node parent, Transform3D transform, out bool isRecycled, bool dontOverSpawn = true) {
        Node clone = GetObject(packedScene, out isRecycled, dontOverSpawn);
        if (clone == null)
            return null;
        else if (clone is Node3D node3D) 
            node3D.Transform = transform;
        parent.AddChild(clone);
        return clone;
    }

    public static bool AddObject(Node clone) {
        foreach (KeyValuePair<PackedScene, ObjectPool<Node>> keyVal in packedLookup) {
            if (keyVal.Key.ResourcePath == clone.SceneFilePath) {
                var objectPool = keyVal.Value;
                objectPool.AddItem(clone);
                return true;
            }
        }
        return false;
    }

    public static bool TryReleaseObject(Node clone) {
        if (instanceLookup.TryGetValue(clone, out ObjectPool<Node> value)) {
            value.ReleaseItem(clone);
            instanceLookup.Remove(clone);
            return true;
        }
        return false;
    }

    public static bool IsPooledObject(Node clone) {
        return instanceLookup.ContainsKey(clone);
    }

    public static void PrintStatus() {
        foreach (KeyValuePair<PackedScene, ObjectPool<Node>> keyVal in packedLookup)
            GDE.Log($"Object Pool for Scene: {System.IO.Path.GetFileNameWithoutExtension(keyVal.Key.ResourcePath)} | In Use: {keyVal.Value.CountUsedItems} | Total: {keyVal.Value.Count} | Max Size: {(keyVal.Value.MaxSize == -1 ? "Infinite" : keyVal.Value.MaxSize)}");
    }

    public static Node Spawn(PackedScene packedScene, Node parent, bool dontOverSpawn = true) => SpawnObject(packedScene, parent, Vector2.Zero, 0, Vector2.One, out _, dontOverSpawn);
    public static Node Spawn(PackedScene packedScene, Node parent, out bool isRecycled, bool dontOverSpawn = true) => SpawnObject(packedScene, parent, Vector2.Zero, 0f, Vector2.One, out isRecycled, dontOverSpawn);
    public static Node Spawn(PackedScene packedScene, Node parent, Vector2 position, bool dontOverSpawn = true) => SpawnObject(packedScene, parent, position, 0f, Vector2.One, out _, dontOverSpawn);
    public static Node Spawn(PackedScene packedScene, Node parent, Vector2 position, out bool isRecycled, bool dontOverSpawn = true) => SpawnObject(packedScene, parent, position, 0f, Vector2.One, out isRecycled, dontOverSpawn);
    public static Node Spawn(PackedScene packedScene, Node parent, Vector2 position, float rotation, bool dontOverSpawn = true) => SpawnObject(packedScene, parent, position, rotation, Vector2.One, out _, dontOverSpawn);
    public static Node Spawn(PackedScene packedScene, Node parent, Vector2 position, float rotation, out bool isRecycled, bool dontOverSpawn = true) => SpawnObject(packedScene, parent, position, rotation, Vector2.One, out isRecycled, dontOverSpawn);
    public static Node Spawn(PackedScene packedScene, Node parent, Vector3 position, bool dontOverSpawn = true) => SpawnObject(packedScene, parent, position, Vector3.One, default, out _, dontOverSpawn);
    public static Node Spawn(PackedScene packedScene, Node parent, Vector3 position, out bool isRecycled, bool dontOverSpawn = true) => SpawnObject(packedScene, parent, position, Vector3.One, default, out isRecycled, dontOverSpawn);
    public static Node Spawn(PackedScene packedScene, Node parent, Vector3 position, Vector3 rotation, bool dontOverSpawn = true) => SpawnObject(packedScene, parent, position, rotation, Vector3.One, out _, dontOverSpawn);
    public static Node Spawn(PackedScene packedScene, Node parent, Vector3 position, Vector3 rotation, out bool isRecycled, bool dontOverSpawn = true) => SpawnObject(packedScene, parent, position, rotation, Vector3.One, out isRecycled, dontOverSpawn);
    public static Node Spawn(PackedScene packedScene, Node parent, Transform2D transform, bool dontOverSpawn = true) => SpawnObject(packedScene, parent, transform, out _, dontOverSpawn);
    public static Node Spawn(PackedScene packedScene, Node parent, Transform2D transform, out bool isRecycled, bool dontOverSpawn = true) => SpawnObject(packedScene, parent, transform, out isRecycled, dontOverSpawn);
    public static Node Spawn(PackedScene packedScene, Node parent, Transform3D transform, bool dontOverSpawn = true) => SpawnObject(packedScene, parent, transform, out _, dontOverSpawn);
    public static Node Spawn(PackedScene packedScene, Node parent, Transform3D transform, out bool isRecycled, bool dontOverSpawn = true) => SpawnObject(packedScene, parent, transform, out isRecycled, dontOverSpawn);

    public static Node Spawn(long uid, Node parent, bool dontOverSpawn = true) => Spawn(GDE.UidToResource<PackedScene>(uid), parent, dontOverSpawn);
    public static Node Spawn(long uid, Node parent, out bool isRecycled, bool dontOverSpawn = true) => Spawn(GDE.UidToResource<PackedScene>(uid), parent, out isRecycled, dontOverSpawn);
    public static Node Spawn(long uid, Node parent, Vector2 position, bool dontOverSpawn = true) => Spawn(GDE.UidToResource<PackedScene>(uid), parent, position, dontOverSpawn);
    public static Node Spawn(long uid, Node parent, Vector2 position, out bool isRecycled, bool dontOverSpawn = true) => Spawn(GDE.UidToResource<PackedScene>(uid), parent, position, out isRecycled, dontOverSpawn);
    public static Node Spawn(long uid, Node parent, Vector2 position, float rotation, bool dontOverSpawn = true) => Spawn(GDE.UidToResource<PackedScene>(uid), parent, position, rotation, dontOverSpawn);
    public static Node Spawn(long uid, Node parent, Vector2 position, float rotation, out bool isRecycled, bool dontOverSpawn = true) => Spawn(GDE.UidToResource<PackedScene>(uid), parent, position, rotation, out isRecycled, dontOverSpawn);
    public static Node Spawn(long uid, Node parent, Vector3 position, bool dontOverSpawn = true) => Spawn(GDE.UidToResource<PackedScene>(uid), parent, position, dontOverSpawn);
    public static Node Spawn(long uid, Node parent, Vector3 position, out bool isRecycled, bool dontOverSpawn = true) => Spawn(GDE.UidToResource<PackedScene>(uid), parent, position, out isRecycled, dontOverSpawn);
    public static Node Spawn(long uid, Node parent, Vector3 position, Vector3 rotation, bool dontOverSpawn = true) => Spawn(GDE.UidToResource<PackedScene>(uid), parent, position, rotation, dontOverSpawn);
    public static Node Spawn(long uid, Node parent, Vector3 position, Vector3 rotation, out bool isRecycled, bool dontOverSpawn = true) => Spawn(GDE.UidToResource<PackedScene>(uid), parent, position, rotation, out isRecycled, dontOverSpawn);
    public static Node Spawn(long uid, Node parent, Transform2D transform, bool dontOverSpawn = true) => Spawn(GDE.UidToResource<PackedScene>(uid), parent, transform, dontOverSpawn);
    public static Node Spawn(long uid, Node parent, Transform2D transform, out bool isRecycled, bool dontOverSpawn = true) => Spawn(GDE.UidToResource<PackedScene>(uid), parent, transform, out isRecycled, dontOverSpawn);

    public static Node GetObject(PackedScene packedScene, bool includeUsed = false) => GetObject(packedScene, out _, includeUsed);
}
