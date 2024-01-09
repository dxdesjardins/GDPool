using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lambchomp.Essentials;

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

    public static T GetComponentInChildren<T>(this Node node, bool isChild = true, bool includeParent = true) {
        if (isChild)
            node = node.GetParent() ?? node;
        if (includeParent && node is T parent)
            return parent;
        if (node != null) {
            foreach (Node i in node.GetChildren(true)) {
                foreach (Node j in i.GetComponentsInChildren(false, true))
                    if (j is T component)
                        return component;
            }
        }
        return default;
    }

    public static Node GetAncestor(this Node node, int generations) {
        for (int i = 0; i < generations; i++)
			node = node.GetParent();
        return node;
    }
}