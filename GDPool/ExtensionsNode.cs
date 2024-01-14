using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lambchomp.Pool;

public static class ExtensionsNode
{
    public static T[] GetComponentsInChildren<T>(this Node node, bool isChild = true, bool includeParent = true) {
        return GetComponentsInChildren(node, isChild, includeParent).OfType<T>().ToArray();
    }

    public static Node[] GetComponentsInChildren(this Node node, bool isChild = true, bool includeParent = true) {
        List<Node> components = new() { };
        if (isChild)
            node = node.GetParent() ?? node;
        if (includeParent)
            components.Add(node);
        foreach (Node i in node.GetChildren(true)) {
            foreach (Node j in i.GetComponentsInChildren(false, true))
                components.Add(j);
        }
        return components.ToArray();
    }

    public static void SafeAddChild(this Node parent, Node child) {
        if (parent.IsNodeReady())
            parent.AddChild(child);
        else
            parent.CallDeferred(Node.MethodName.AddChild, child);
    }

    public static Node GetAncestor(this Node node, int generations) {
        for (int i = 0; i < generations; i++)
            node = node.GetParent();
        return node;
    }
}
