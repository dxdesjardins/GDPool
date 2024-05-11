using Godot;
using System;
using System.Collections.Generic;

namespace Chomp.Pool;

public partial class ReturnToPool : Node
{
    public override void _Notification(int what) {
        switch (what) {
            case (int)NotificationReady:
                PoolManager.ReturnToPoolActions.Add(() => {
                    this.RemoveParent(); });
                break;
            case (int)NotificationExitTree:
                bool isPooled = PoolManager.ReleaseObject(this.GetParent());
                if (!isPooled)
                    isPooled = PoolManager.AddObject(this.GetParent());
                break;
            case (int)NotificationPredelete:
                if (PoolManager.IsPooledObject(this.GetParent()))
                    GD.PrintErr("Warning: ", System.IO.Path.GetFileNameWithoutExtension(this.GetParent().SceneFilePath), " is being freed while part of an Object Pool.");
                break;
        }
    }
}
