using Godot;
using System;
using System.Collections.Generic;

namespace Lambchomp.Pool;

public partial class ReturnToPool : Node
{
    [Export] private NodeEvent onWarpAfterFadeOut;
    private bool isAddedToPool = false;

    public override void _Notification(int what) {
        switch (what) {
            // Note: You can't QueueFree a pool object. You have to implement a system to ensure it's removed from a parent scene about to be QueueFreed.
            case (int)NotificationReady:
                onWarpAfterFadeOut?.AddListener(() => this.GetAncestor(2)?.RemoveChild(this.GetParent()));
                break;
            case (int)NotificationExitTree:
                isAddedToPool = PoolManager.ReleaseObject(this.GetParent());
                if (!isAddedToPool)
                    isAddedToPool = PoolManager.AddObject(this.GetParent());
                break;
            case (int)NotificationPredelete:
                if (isAddedToPool)
                    GD.PrintErr("Warning: ", this.GetParent().Name, " is being freed while part of an Object Pool.");
                break;
        }
    }
}
