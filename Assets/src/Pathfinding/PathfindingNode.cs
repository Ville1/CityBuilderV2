using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PathfindingNode
{
    public Coordinates Coordinates { get; set; }
    public float Cost { get; set; }
    public bool Passable { get { return Cost > 0.0f; } }
    public int X { get { return Coordinates.X; } set { Coordinates.X = value; } }
    public int Y { get { return Coordinates.Y; } set { Coordinates.Y = value; } }

    public PathfindingNode(Coordinates coordinates, float cost)
    {
        Coordinates = new Coordinates(coordinates);
        Cost = cost;
    }

    public override bool Equals(object obj)
    {
        if (obj is PathfindingNode) {
            return ((PathfindingNode)obj).X == X && ((PathfindingNode)obj).Y == Y;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return Int32.Parse(X + "" + Y);
    }

    public float Distance(Coordinates coordinates)
    {
        return Mathf.Sqrt((X - coordinates.X) * (X - coordinates.X) + (Y - coordinates.Y) * (Y - coordinates.Y));
    }

    public override string ToString()
    {
        return "PathfindingNode(X: " + X + ", Y: " + Y + ")";
    }

    public Dictionary<Coordinates.Direction, PathfindingNode> Get_Adjanced_nodes(List<PathfindingNode> other_nodes)
    {
        Dictionary<Coordinates.Direction, PathfindingNode> nodes = new Dictionary<Coordinates.Direction, PathfindingNode>();
        foreach (Coordinates.Direction direction in Enum.GetValues(typeof(Coordinates.Direction))) {
            Coordinates new_coordinates = new Coordinates(Coordinates);
            new_coordinates.Shift(direction);
            PathfindingNode node = other_nodes.FirstOrDefault(x => x.Coordinates.X == new_coordinates.X && x.Coordinates.Y == new_coordinates.Y);
            if (node != null) {
                nodes.Add(direction, node);
            }
        }
        return nodes;
    }
}
