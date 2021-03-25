using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class Coordinates
{
    public enum Direction { North, North_East, East, South_East, South, South_West, West, North_West }

    public int X { get; set; }
    public int Y { get; set; }

    public Coordinates(int x, int y)
    {
        X = x;
        Y = y;
    }

    public Coordinates(int x, int y, Direction delta)
    {
        X = x;
        Y = y;
        Shift(delta);
    }

    public Coordinates(Coordinates coordinates)
    {
        X = coordinates.X;
        Y = coordinates.Y;
    }

    public Vector2 Vector
    {
        get {
            return new Vector2(X, Y);
        }
        set {
            X = (int)value.x;
            Y = (int)value.y;
        }
    }

    /// <summary>
    /// Shifts coordinate point to specified direction
    /// </summary>
    /// <param name="direction"></param>
    public void Shift(Direction direction)
    {
        switch (direction) {
            case Direction.North:
                Y++;
                break;
            case Direction.North_East:
                Y++;
                X++;
                break;
            case Direction.East:
                X++;
                break;
            case Direction.South_East:
                Y--;
                X++;
                break;
            case Direction.South:
                Y--;
                break;
            case Direction.South_West:
                Y--;
                X--;
                break;
            case Direction.West:
                X--;
                break;
            case Direction.North_West:
                Y++;
                X--;
                break;
        }
    }

    public Coordinates Shift(Coordinates coordinates)
    {
        X += coordinates.X;
        Y += coordinates.Y;
        return this;
    }

    public static Coordinates Shift_Delta(Direction direction)
    {
        Coordinates delta = new Coordinates(0, 0);
        delta.Shift(direction);
        return delta;
    }

    public string Parse_Text(bool brackets, bool spaces)
    {
        StringBuilder builder = new StringBuilder();
        if (brackets) {
            builder.Append("(");
        }
        builder.Append("X:");
        if (spaces) {
            builder.Append(" ");
        }
        builder.Append(X).Append(",");
        if (spaces) {
            builder.Append(" ");
        }
        builder.Append("Y:");
        if (spaces) {
            builder.Append(" ");
        }
        builder.Append(Y);
        if (brackets) {
            builder.Append(")");
        }
        return builder.ToString();
    }

    public override string ToString()
    {
        return string.Format("Coordinates{0}", Parse_Text(true, true));
    }

    public override bool Equals(object obj)
    {
        if (obj is Coordinates) {
            return ((Coordinates)obj).X == X && ((Coordinates)obj).Y == Y;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return Int32.Parse(X + string.Empty + Y);
    }

    public float Distance(Coordinates coordinates)
    {
        return Mathf.Sqrt((X - coordinates.X) * (X - coordinates.X) + (Y - coordinates.Y) * (Y - coordinates.Y));
    }

    public Dictionary<Direction, Coordinates> Get_Adjanced_Coordinates(List<Coordinates> other_coordinates)
    {
        Dictionary<Direction, Coordinates> coordinates = new Dictionary<Direction, Coordinates>();
        foreach (Direction direction in Enum.GetValues(typeof(Direction))) {
            Coordinates new_coordinates = new Coordinates(this);
            new_coordinates.Shift(direction);
            if (other_coordinates.Contains(new_coordinates)) {
                coordinates.Add(direction, new_coordinates);
            }
        }
        return coordinates;
    }

    public bool Is_Adjacent_To(Coordinates coordinates)
    {
        return Math.Abs(X - coordinates.X) == 1 || Math.Abs(Y - coordinates.Y) == 1;
    }

    public static List<Direction> Directly_Adjacent_Directions
    {
        get {
            return new List<Direction>() { Direction.North, Direction.East, Direction.South, Direction.West };
        }
    }
}
