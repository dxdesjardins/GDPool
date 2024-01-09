using Godot;
using System;
using System.Collections.Generic;

namespace Lambchomp.Essentials;

public partial class ReturnToPool : Node
{
    // Note: You can't QueueFree a pool object. You have to implement a system to ensure it's removed from a parent scene about to be QueueFreed. Commented out code is an example of how to do this.
    //[Export] private NodeEvent onWarpAfterFadeOut;
    private bool isAddedToPool;

    public override void _Notification(int what) {
        switch (what) {
            //case (int)NotificationReady:
            //	onWarpAfterFadeOut?.AddListener(() => this.GetAncestor(2)?.RemoveChild(this.GetParent()));
            //	break;
            case (int)NotificationExitTree:
                isAddedToPool = PoolManager.ReleaseObject(this.GetParent());
                if (!isAddedToPool)
                    isAddedToPool = PoolManager.AddObject(this.GetParent());
                break;
        }
    }
}
