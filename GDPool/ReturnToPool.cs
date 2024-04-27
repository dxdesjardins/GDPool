using Godot;
using System;
using System.Collections.Generic;

namespace Lambchomp.Pool;

public partial class ReturnToPool : Node
{
    public override void _Notification(int what) {
        switch (what) {
            case (int)NotificationReady:
                PoolManager.Instance.OnReturnObjectsToPool += this.RemoveParent;
                break;
            case (int)NotificationExitTree:
                bool isPooled = PoolManager.Instance.ReleaseObject(this.GetParent());
                if (!isPooled)
                    isPooled = PoolManager.Instance.AddObject(this.GetParent());
                break;
            case (int)NotificationPredelete:
                if (PoolManager.Instance.IsPooledObject(this.GetParent()))
                    GD.PrintErr("Warning: ", System.IO.Path.GetFileNameWithoutExtension(this.GetParent().SceneFilePath), " is being freed while part of an Object Pool.");
                break;
        }
    }
}
