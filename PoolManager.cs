using Godot;
using System;
using System.Collections.Generic;

namespace Lambchomp.Essentials;

public partial class PoolManager : Singleton<PoolManager>
{
    private Dictionary<PackedScene, ObjectPool<Node>> packedLookup = new();
    private Dictionary<Node, ObjectPool<Node>> instanceLookup = new();
    private static bool closingGame = false;

    public override void _ExitTree() {
        closingGame = true;
    }

    public void WarmObjs(PackedScene packedScene, int size, int maxSize = -1) {
        if (packedLookup.ContainsKey(packedScene))
            throw new Exception("Pool for Scene " + packedScene.ResourcePath + " has already been created");
        ObjectPool<Node> pool = new(() => { return packedScene.Instantiate<Node>(); }, size, maxSize);
        packedLookup[packedScene] = pool;
    }

    public Node GetObj(PackedScene packedScene, out bool isRecycled) {
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

    public Node SpawnObj(PackedScene packedScene, Node parent, out bool isRecycled) {
        return SpawnObj(packedScene, parent, Vector2.Zero, 0f, out isRecycled);
    }

    public Node SpawnObj(PackedScene packedScene, Node parent, Vector2 position, float rotation, out bool isRecycled) {
        Node clone = GetObj(packedScene, out isRecycled);
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

    public bool AddObj(Node clone) {
        foreach (KeyValuePair<PackedScene, ObjectPool<Node>> keyVal in packedLookup) {
			if (keyVal.Key.ResourcePath == clone.SceneFilePath) {
                var objectPool = keyVal.Value;
				objectPool.AddItem(clone);
				return true;
            }
		}
		return false;
    }

    public bool ReleaseObj(Node clone) {
        if (instanceLookup.ContainsKey(clone)) {
            instanceLookup[clone].ReleaseItem(clone);
            instanceLookup.Remove(clone);
            return true;
        }
        return false;
    }

    public void PrintStatus() {
        foreach (KeyValuePair<PackedScene, ObjectPool<Node>> keyVal in packedLookup)
            GD.Print(string.Format("Object Pool for Scene: {0} In Use: {1} Total {2}", keyVal.Key.ResourcePath, keyVal.Value.CountUsedItems, keyVal.Value.Count));
    }

    #region Static API

    public static void WarmObjects(PackedScene packedScene, int size, int maxSize = -1  ) {
        Instance.WarmObjs(packedScene, size, maxSize);
    }

    public static Node SpawnObject(PackedScene packedScene, Node parent) {
        return SpawnObject(packedScene, parent, out bool _);
    }

    public static Node SpawnObject(PackedScene packedScene, Node parent, out bool isRecycled) {
        return Instance.SpawnObj(packedScene, parent, out isRecycled);
    }

    public static Node SpawnObject(PackedScene packedScene, Node parent, Vector2 position, float rotation) {
        return SpawnObject(packedScene, parent, position, rotation, out bool _);
    }

    public static Node SpawnObject(PackedScene packedScene, Node parent, Vector2 position, float rotation, out bool isRecycled) {
        return Instance.SpawnObj(packedScene, parent, position, rotation, out isRecycled);
    }

    public static bool ReleaseObject(Node clone) {
        if (!closingGame)
            return Instance.ReleaseObj(clone);
        return false;
    }

    public static bool AddObject(Node clone) {
        return Instance.AddObj(clone);
    }

    public static Node GetObject(PackedScene packedScene, out bool isRecycled) {
		return Instance.GetObj(packedScene, out isRecycled);
	}

    #endregion
}
