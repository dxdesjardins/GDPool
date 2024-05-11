using Godot;
using System;
using System.Collections.Generic;

namespace Chomp.Pool;

public partial class PoolManager : Singleton<PoolManager>
{
    private Dictionary<PackedScene, ObjectPool<Node>> packedLookup = new();
    private Dictionary<Node, ObjectPool<Node>> instanceLookup = new();
    private List<Action> returnToPoolActions = new();

    private void _WarmPool(PackedScene packedScene, int size, int maxSize = -1) {
        if (packedLookup.ContainsKey(packedScene))
            throw new Exception("Pool for Scene " + packedScene.ResourcePath + " has already been created");
        ObjectPool<Node> pool = new(() => {
            Node instance = packedScene.Instantiate<Node>();
            instance.MakeNameUnique();
            var returnComponent = Activator.CreateInstance(typeof(ReturnToPool)) as ReturnToPool;
            instance.AddChild(returnComponent);
            return instance;
        }, size, maxSize);
        packedLookup[packedScene] = pool;
    }

    private Node _GetObject(PackedScene packedScene, out bool isRecycled, bool dontOverSpawn = true) {
        bool createdPool = false;
        if (!packedLookup.ContainsKey(packedScene)) {
            _WarmPool(packedScene, 1);
            createdPool = true;
        }
        var pool = packedLookup[packedScene];
        var clone = pool.GetItem(out isRecycled, dontOverSpawn);
        if (clone == null)
            return null;
        if (isRecycled)
            instanceLookup.Add(clone, pool);
        if (createdPool)
            isRecycled = false;
        return clone;
    }

    private Node _SpawnObject(PackedScene packedScene, Node parent, Vector2 position, float rotation, out bool isRecycled, bool dontOverSpawn = true) {
        Node clone = _GetObject(packedScene, out isRecycled, dontOverSpawn);
        if (clone == null)
            return null;
        else if (clone is Node2D node2D) {
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

    private Node _SpawnObject(PackedScene packedScene, Node parent, Vector3 position, Vector3 rotation, out bool isRecycled, bool dontOverSpawn = true) {
        Node clone = _GetObject(packedScene, out isRecycled, dontOverSpawn);
        if (clone == null)
            return null;
        else if (clone is Node3D node3D) {
            node3D.Position = position;
            node3D.Rotation = rotation;
        }
        parent.AddChild(clone);
		return clone;
    }

    private bool _AddObject(Node clone) {
        foreach (KeyValuePair<PackedScene, ObjectPool<Node>> keyVal in packedLookup) {
            if (keyVal.Key.ResourcePath == clone.SceneFilePath) {
                var objectPool = keyVal.Value;
                objectPool.AddItem(clone);
                return true;
            }
        }
        return false;
    }

    private bool _ReleaseObject(Node clone) {
        if (instanceLookup.ContainsKey(clone)) {
            instanceLookup[clone].ReleaseItem(clone);
            instanceLookup.Remove(clone);
            return true;
        }
        return false;
    }

    private bool _IsPooledObject(Node clone) {
        return instanceLookup.ContainsKey(clone);
    }

    private void _PrintStatus() {
        foreach (KeyValuePair<PackedScene, ObjectPool<Node>> keyVal in packedLookup)
            GD.Print(string.Format("Object Pool for Scene: {0} | In Use: {1} | Total {2}", System.IO.Path.GetFileNameWithoutExtension(keyVal.Key.ResourcePath), keyVal.Value.CountUsedItems, keyVal.Value.Count));
    }

    public static void ReturnObjectsToPool() {
        foreach (Action action in Instance.returnToPoolActions)
            action.Invoke();
    }

    public static List<Action> ReturnToPoolActions => Instance.returnToPoolActions;
    public static void Warm(PackedScene packedScene, int size, int maxSize = -1) => Instance._WarmPool(packedScene, size, maxSize);
    public static Node GetObject(PackedScene packedScene, out bool isRecycled, bool includeUsed = false) => Instance._GetObject(packedScene, out isRecycled, includeUsed);
    public static Node GetObject(PackedScene packedScene, bool includeUsed = false) => Instance._GetObject(packedScene, out _, includeUsed);
    public static Node Spawn(PackedScene packedScene, Node parent, bool dontOverSpawn = true) => Instance._SpawnObject(packedScene, parent, Vector2.Zero, 0, out _, dontOverSpawn);
    public static Node Spawn(PackedScene packedScene, Node parent, out bool isRecycled, bool dontOverSpawn = true) => Instance._SpawnObject(packedScene, parent, Vector2.Zero, 0f, out isRecycled, dontOverSpawn);
    public static Node Spawn(PackedScene packedScene, Node parent, Vector2 position, bool dontOverSpawn = true) => Instance._SpawnObject(packedScene, parent, position, 0f, out _, dontOverSpawn);
    public static Node Spawn(PackedScene packedScene, Node parent, Vector2 position, out bool isRecycled, bool dontOverSpawn = true) => Instance._SpawnObject(packedScene, parent, position, 0f, out isRecycled, dontOverSpawn);
    public static Node Spawn(PackedScene packedScene, Node parent, Vector2 position, float rotation, bool dontOverSpawn = true) => Instance._SpawnObject(packedScene, parent, position, rotation, out _, dontOverSpawn);
    public static Node Spawn(PackedScene packedScene, Node parent, Vector2 position, float rotation, out bool isRecycled, bool dontOverSpawn = true) => Instance._SpawnObject(packedScene, parent, position, rotation, out isRecycled, dontOverSpawn);
    public static Node Spawn(PackedScene packedScene, Node parent, Vector3 position, bool dontOverSpawn = true) => Instance._SpawnObject(packedScene, parent, position, default, out _, dontOverSpawn);
    public static Node Spawn(PackedScene packedScene, Node parent, Vector3 position, out bool isRecycled, bool dontOverSpawn = true) => Instance._SpawnObject(packedScene, parent, position, default, out isRecycled, dontOverSpawn);
    public static Node Spawn(PackedScene packedScene, Node parent, Vector3 position, Vector3 rotation, bool dontOverSpawn = true) => Instance._SpawnObject(packedScene, parent, position, rotation, out _, dontOverSpawn);
    public static Node Spawn(PackedScene packedScene, Node parent, Vector3 position, Vector3 rotation, out bool isRecycled, bool dontOverSpawn = true) => Instance._SpawnObject(packedScene, parent, position, rotation, out isRecycled, dontOverSpawn);
    public static bool AddObject(Node clone) => Instance._AddObject(clone);
    public static bool ReleaseObject(Node clone) => Instance._ReleaseObject(clone);
    public static bool IsPooledObject(Node clone) => Instance._IsPooledObject(clone);
    public static void PrintStatus() => Instance._PrintStatus();
}
