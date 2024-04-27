using Godot;
using System;
using System.Collections.Generic;

namespace Lambchomp.Pool;

public partial class PoolManager : Singleton<PoolManager>
{
    private Dictionary<PackedScene, ObjectPool<Node>> packedLookup = new();
    private Dictionary<Node, ObjectPool<Node>> instanceLookup = new();
    public delegate void ReturnObjectsToPoolHandler();
    public event ReturnObjectsToPoolHandler OnReturnObjectsToPool;

    public void WarmObjects(PackedScene packedScene, int size, int maxSize = -1) {
        if (packedLookup.ContainsKey(packedScene))
            throw new Exception("Pool for Scene " + packedScene.ResourcePath + " has already been created");
        ObjectPool<Node> pool = new(() => {
            Node clone = packedScene.Instantiate<Node>();
            var returnComponent = Activator.CreateInstance(typeof(ReturnToPool)) as ReturnToPool;
            clone.AddChild(returnComponent);
            return clone;
        }, size, maxSize);
        packedLookup[packedScene] = pool;
    }

    public Node GetObject(PackedScene packedScene, out bool isRecycled) {
        bool createdPool = false;
        if (!packedLookup.ContainsKey(packedScene)) {
            WarmObjects(packedScene, 1);
            createdPool = true;
        }
        var pool = packedLookup[packedScene];
        var clone = pool.GetItem(out isRecycled);
        if (isRecycled)
            instanceLookup.Add(clone, pool);
        if (createdPool)
            isRecycled = false;
        return clone;
    }

    public Node GetObject(PackedScene packedScene) => GetObject(packedScene, out _);

    public Node SpawnObject(PackedScene packedScene, Node parent, out bool isRecycled) {
        return SpawnObject(packedScene, parent, Vector2.Zero, 0f, out isRecycled);
    }

    public Node SpawnObject(PackedScene packedScene, Node parent, Vector2 position) => SpawnObject(packedScene, parent, position, 0f, out _);
    public Node SpawnObject(PackedScene packedScene, Node parent, Vector2 position, float rotation) => SpawnObject(packedScene, parent, position, rotation, out _);

    public Node SpawnObject(PackedScene packedScene, Node parent, Vector2 position, float rotation, out bool isRecycled) {
        Node clone = GetObject(packedScene, out isRecycled);
        if (clone is Node2D node2D) {
            node2D.Position = position;
            node2D.Rotation = rotation;
        }
        else if (clone is Control control) {
            control.Position = position;
            control.Rotation = rotation;
        }
        parent.AddChild(clone);
        return clone;
    }

    public Node SpawnObject(PackedScene packedScene, Node parent, Vector3 position) => SpawnObject(packedScene, parent, position, default, out _);
    public Node SpawnObject(PackedScene packedScene, Node parent, Vector3 position, Vector3 rotation) => SpawnObject(packedScene, parent, position, rotation, out _);

    public Node SpawnObject(PackedScene packedScene, Node parent, Vector3 position, Vector3 rotation, out bool isRecycled) {
        Node clone = GetObject(packedScene, out isRecycled);
        if (clone is Node3D node3D) {
            node3D.Position = position;
            node3D.Rotation = rotation;
        }
        parent.AddChild(clone);
        return clone;
    }

    public bool AddObject(Node clone) {
        foreach (KeyValuePair<PackedScene, ObjectPool<Node>> keyVal in packedLookup) {
            if (keyVal.Key.ResourcePath == clone.SceneFilePath) {
                var objectPool = keyVal.Value;
                objectPool.AddItem(clone);
                return true;
            }
        }
        return false;
    }

    public bool ReleaseObject(Node clone) {
        if (instanceLookup.ContainsKey(clone)) {
            instanceLookup[clone].ReleaseItem(clone);
            instanceLookup.Remove(clone);
            return true;
        }
        return false;
    }

    public bool IsPooledObject(Node clone) {
        return instanceLookup.ContainsKey(clone);
    }

    public void PrintStatus() {
        foreach (KeyValuePair<PackedScene, ObjectPool<Node>> keyVal in packedLookup)
            GD.Print(string.Format("Object Pool for Scene: {0} | In Use: {1} | Total {2}", System.IO.Path.GetFileNameWithoutExtension(keyVal.Key.ResourcePath), keyVal.Value.CountUsedItems, keyVal.Value.Count));
    }
}
