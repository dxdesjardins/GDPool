using Godot;
using System;
using System.Collections.Generic;
using Chomp.Essentials;

namespace Chomp.Pool;

public partial class ReturnToPool : Node
{
    private Node stage;

    public ReturnToPool() {
        PoolManager.ReturnObjectsToPool.AddListener((node) => {
            if (this.IsInsideTree() && node == stage)
                this.RemoveParent();
        });
        this.TreeExited += OnTreeExited;
    }

    ~ReturnToPool() {
        Node parent = this.GetParent();
        if (!StageManager.IsQuittingGame && PoolManager.IsPooledObject(parent))
            GDE.LogErr($"{System.IO.Path.GetFileNameWithoutExtension(parent.SceneFilePath)} has been freed while part of an Object Pool.");
    }

    public override void _EnterTree() {
        stage = this.GetStage();
        if (StageManager.IsStageUnloading(stage))
            this.RemoveParent();
    }

    private void OnTreeExited() {
        Node parent = this.GetParent();
        if (!PoolManager.TryReleaseObject(parent))
            PoolManager.AddObject(parent);
        stage = null;
    }
}
