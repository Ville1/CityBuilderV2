using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Pathfinding
{
    /// <summary>
    /// Returns path between two nodes. On the path first node is start and last node is end.
    /// TODO: limit range for optimization?
    /// TODO: use this for all pathfinding
    /// TODO: diagonals?
    /// </summary>
    /// <param name="all_nodes"></param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public static List<PathfindingNode> Path(List<PathfindingNode> all_nodes, PathfindingNode start, PathfindingNode end)
    {
        List<PathfindingNode> path = new List<PathfindingNode>();
        List<PathfindingNode> Q = new List<PathfindingNode>();
        Dictionary<PathfindingNode, float> dist = new Dictionary<PathfindingNode, float>();
        Dictionary<PathfindingNode, PathfindingNode> prev = new Dictionary<PathfindingNode, PathfindingNode>();

        if (!all_nodes.Contains(end)) {
            all_nodes.Add(end);
        }
        if (!all_nodes.Contains(start)) {
            all_nodes.Add(start);
        }

        for (int i = 0; i < all_nodes.Count; i++) {
            dist.Add(all_nodes[i], float.MaxValue);
            prev.Add(all_nodes[i], null);
            Q.Add(all_nodes[i]);
        }
        dist[start] = 0.0f;

		///////TODO: Check for impassable tiles and give them dist = float.MaxValue
        foreach (KeyValuePair<Coordinates.Direction, PathfindingNode> v in start.Get_Adjanced_nodes(all_nodes.ToList())) {
            dist[v.Value] = Helper.Is_Diagonal(v.Key) ? Mathf.Sqrt(2.0f) * v.Value.Cost : v.Value.Cost;
            prev[v.Value] = start;
        }

        while (Q.Count > 0) {
            int min_dist_index = 0;
            float min_dist = -1.0f;
            for (int i = 0; i < Q.Count; i++) {
                if (dist[Q[i]] < min_dist || min_dist == -1.0f) {
                    min_dist_index = i;
                    min_dist = dist[Q[i]];
                }
            }

            PathfindingNode u = Q[min_dist_index];
            Q.RemoveAt(min_dist_index);

            if (u.Equals(end)) {
                while (prev[u] != null) {
                    path.Insert(0, prev[u]);
                    u = prev[u];
                }
                path.Add(end);
                break;
            } else {
                foreach (KeyValuePair<Coordinates.Direction, PathfindingNode> v in u.Get_Adjanced_nodes(all_nodes)) {
                    float alt = Helper.Is_Diagonal(v.Key) ? dist[u] + (Mathf.Sqrt(2.0f) * v.Value.Cost) : dist[u] + v.Value.Cost;
                    ///////TODO: Check for impassable tiles and give them dist = float.MaxValue
					if (alt < dist[v.Value]) {
                        dist[v.Value] = alt;
                        prev[v.Value] = u;
                    }
                }
            }
        }
		
		if(path.Count == 1 && path[0] == end) {
            return new List<PathfindingNode>();
        }

        return path;
    }

    public static List<Coordinates> Straight_Line(Coordinates start, Coordinates end)
    {
        List<Coordinates> line = new List<Coordinates>();

        // :-)
        //https://stackoverflow.com/questions/11678693/all-cases-covered-bresenhams-line-algorithm
        int x = start.X;
        int y = start.Y;
        int x2 = end.X;
        int y2 = end.Y;

        int w = x2 - x;
        int h = y2 - y;
        int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0;
        if (w < 0) dx1 = -1; else if (w > 0) dx1 = 1;
        if (h < 0) dy1 = -1; else if (h > 0) dy1 = 1;
        if (w < 0) dx2 = -1; else if (w > 0) dx2 = 1;
        int longest = Math.Abs(w);
        int shortest = Math.Abs(h);
        if (!(longest > shortest)) {
            longest = Math.Abs(h);
            shortest = Math.Abs(w);
            if (h < 0) dy2 = -1; else if (h > 0) dy2 = 1;
            dx2 = 0;
        }
        int numerator = longest >> 1;
        for (int i = 0; i <= longest; i++) {
            line.Add(new Coordinates(x, y));
            numerator += shortest;
            if (!(numerator < longest)) {
                numerator -= longest;
                x += dx1;
                y += dy1;
            } else {
                x += dx2;
                y += dy2;
            }
        }

        return line;
    }
}
