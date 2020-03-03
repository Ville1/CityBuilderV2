﻿using System;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    public enum MapState { Inactive, Normal, Generating, Loading, Saving }
    public static readonly float Z_LEVEL = 0.0f;

    public static Map Instance { get; private set; }
    
    public GameObject Tile_GameObject;

    public int Width { get; private set; }
    public int Height { get; private set; }
    public MapState State { get; private set; }

    private List<List<Tile>> tiles;
    private int generation_loop;
    private int generation_index_x;
    private int generation_index_y;
    private int loop_progress;

    /// <summary>
    /// Initializiation
    /// </summary>
    private void Start()
    {
        if (Instance != null) {
            CustomLogger.Instance.Error(LogMessages.MULTIPLE_INSTANCES);
            return;
        }
        Instance = this;
        State = MapState.Inactive;
    }

    /// <summary>
    /// Per frame update
    /// </summary>
    private void Update()
    {
        if(State == MapState.Generating) {
            switch (generation_loop) {
                case 1:
                    Generation_Loop_1();
                    break;
                case 2:
                    Generation_Loop_2();
                    break;
                case 3:
                    Generation_Loop_3();
                    break;
            }
            return;
        }
    }
    
    public void Start_Generation(int width, int height)
    {
        Delete();
        Active = false;
        State = MapState.Generating;
        Width = width;
        Height = height;

        generation_loop = 1;
        generation_index_x = 0;
        generation_index_y = 0;
        loop_progress = 0;

        tiles = new List<List<Tile>>();
        for(int x = 0; x < width; x++) {
            tiles.Add(new List<Tile>());
            for(int y = 0; y < height; y++) {
                tiles[x].Add(null);
            }
        }

        ProgressBarManager.Instance.Active = true;
        Update_Progress();
    }

    private void Generation_Loop_1()
    {
        Tile tile = new Tile(generation_index_x, generation_index_y, TilePrototypes.Instance.Get(RNG.Instance.Next(0, 100) < 95 ? "grass" : "sparse_forest"));
        tiles[generation_index_x][generation_index_y] = tile;

        loop_progress++;
        generation_index_x++;
        if(generation_index_x == Width) {
            generation_index_x = 0;
            generation_index_y++;
        }
        if(generation_index_y == Height) {
            Finish_Generation();
            return;
            generation_index_x = 0;
            generation_index_y = 0;
            loop_progress = 0;
            generation_loop++;
        }
        Update_Progress();
    }

    private void Generation_Loop_2()
    {
        loop_progress++;
        generation_index_x++;
        if (generation_index_x == Width) {
            generation_index_x = 0;
            generation_index_y++;
        }
        if (generation_index_y == Height) {
            generation_index_x = 0;
            generation_index_y = 0;
            loop_progress = 0;
            generation_loop++;
        }
        Update_Progress();
    }

    private void Generation_Loop_3()
    {
        loop_progress++;
        generation_index_x++;
        if (generation_index_x == Width) {
            generation_index_x = 0;
            generation_index_y++;
        }
        if (generation_index_y == Height) {
            Finish_Generation();
        } else {
            Update_Progress();
        }
    }

    public void Finish_Generation()
    {
        State = MapState.Normal;
        ProgressBarManager.Instance.Active = false;
        Active = true;
        CameraManager.Instance.Set_Camera_Location(Get_Tile_At(Width / 2, Height / 2).Coordinates.Vector);
        BuildMenuManager.Instance.Interactable = true;
    }
    
    private void Update_Progress()
    {
        if(State == MapState.Generating) {
            float max = Width * Height * 3;
            float current = ((generation_loop - 1) * (Width * Height)) + loop_progress;
            float progress = current / max;
            ProgressBarManager.Instance.Show("Generating map...", progress);
        }
    }

    public Tile Get_Tile_At(int x, int y, Coordinates.Direction? offset = null)
    {
        return Get_Tile_At(new Coordinates(x, y), offset);
    }
    
    public Tile Get_Tile_At(Coordinates coordinates, Coordinates.Direction? offset = null)
    {
        if (offset == null) {
            if (coordinates.X < 0 || coordinates.Y < 0 || coordinates.X >= Width || coordinates.Y >= Height) {
                return null;
            }
            return tiles[coordinates.X][coordinates.Y];
        }
        coordinates.Shift((Coordinates.Direction)offset);
        if (coordinates.X < 0 || coordinates.Y < 0 || coordinates.X >= Width || coordinates.Y >= Height) {
            return null;
        }
        return tiles[coordinates.X][coordinates.Y];
    }

    /// <summary>
    /// Returns list of tiles in the rectangle
    /// </summary>
    /// <param name="coordinates"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <returns></returns>
    public List<Tile> Get_Tiles(Coordinates coordinates, int width, int height)
    {
        return Get_Tiles(coordinates.X, coordinates.Y, width, height);
    }

    /// <summary>
    /// Returns list of tiles in the rectangle
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <returns></returns>
    public List<Tile> Get_Tiles(int x, int y, int width, int height)
    {
        List<Tile> tiles = new List<Tile>();
        for (int x_i = x; x_i < width + x; x_i++) {
            for (int y_i = y; y_i < height + y; y_i++) {
                Tile tile = Get_Tile_At(x_i, y_i);
                if (tile == null) {
                    return null;
                }
                tiles.Add(tile);
            }
        }
        return tiles;
    }
    
    public Dictionary<Coordinates.Direction, Tile> Get_Adjanced_Tiles(Tile tile, bool diagonal = false)
    {
        Dictionary<Coordinates.Direction, Tile> tiles = new Dictionary<Coordinates.Direction, Tile>();
        Dictionary<Coordinates.Direction, Tile> possible_tiles = new Dictionary<Coordinates.Direction, Tile>();
        if (!diagonal) {
            possible_tiles.Add(Coordinates.Direction.North, Get_Tile_At(tile.X, tile.Y, Coordinates.Direction.North));
            possible_tiles.Add(Coordinates.Direction.East, Get_Tile_At(tile.X, tile.Y, Coordinates.Direction.East));
            possible_tiles.Add(Coordinates.Direction.South, Get_Tile_At(tile.X, tile.Y, Coordinates.Direction.South));
            possible_tiles.Add(Coordinates.Direction.West, Get_Tile_At(tile.X, tile.Y, Coordinates.Direction.West));
        } else {
            foreach (Coordinates.Direction direction in Enum.GetValues(typeof(Coordinates.Direction))) {
                possible_tiles.Add(direction, Get_Tile_At(tile.X, tile.Y, direction));
            }
        }
        foreach (KeyValuePair<Coordinates.Direction, Tile> possible_tile in possible_tiles) {
            if (possible_tile.Value != null) {
                tiles.Add(possible_tile.Key, possible_tile.Value);
            }
        }
        return tiles;
    }

    /// <summary>
    /// Returns tiles in a circle around specified point
    /// </summary>
    /// <param name="coordinates"></param>
    /// <param name="range"></param>
    /// <param name="plus_half_x"></param>
    /// <param name="plus_half_y"></param>
    /// <returns></returns>
    public List<Tile> Get_Tiles_In_Circle(Coordinates coordinates, float range, bool plus_half_x = false, bool plus_half_y = false)
    {
        return Get_Tiles_In_Circle(coordinates.X, coordinates.Y, range, plus_half_x, plus_half_y);
    }

    /// <summary>
    /// Returns tiles in a circle around specified point
    /// </summary>
    /// <param name="x_p"></param>
    /// <param name="y_p"></param>
    /// <param name="range"></param>
    /// <param name="plus_half_x"></param>
    /// <param name="plus_half_y"></param>
    /// <returns></returns>
    public List<Tile> Get_Tiles_In_Circle(int x_p, int y_p, float range, bool plus_half_x = false, bool plus_half_y = false)
    {
        List<Tile> list = new List<Tile>();

        bool searching = true;
        int loop_index = 0;
        while (searching) {
            bool insert = false;

            int loop_x = x_p - 1 - loop_index;
            int loop_x_max = x_p + 1 + loop_index;
            if (plus_half_x) {
                loop_x = x_p - loop_index;
                loop_x_max = x_p + 1 + loop_index;
            }
            int loop_y = y_p - 1 - loop_index;
            int loop_y_max = y_p + 1 + loop_index;
            if (plus_half_y) {
                loop_y = y_p - loop_index;
                loop_y_max = y_p + 1 + loop_index;
            }
            float x_p_2 = x_p + (Convert.ToInt32(plus_half_x) * 0.5f);
            float y_p_2 = y_p + (Convert.ToInt32(plus_half_y) * 0.5f);

            for (int x = loop_x; x <= loop_x_max; x++) {
                for (int y = loop_y; y <= loop_y_max; y++) {
                    if (x == loop_x || x == loop_x_max || y == loop_y || y == loop_y_max) {
                        Tile tile = Get_Tile_At(x, y);
                        if (tile != null && range >= Mathf.Sqrt((x_p_2 - tile.X) * (x_p_2 - tile.X) + (y_p_2 - tile.Y) * (y_p_2 - tile.Y))) {
                            list.Add(tile);
                            insert = true;
                        }
                    }
                }
            }

            loop_index++;
            if (!insert) {
                searching = false;
            }
        }

        return list;
    }

    /// <summary>
    /// Returns tiles in line between two points
    /// </summary>
    /// <param name="tile_1"></param>
    /// <param name="tile_2"></param>
    /// <returns></returns>
    public List<Tile> Get_Tiles_In_Line(Tile tile_1, Tile tile_2)
    {
        return Get_Tiles_In_Line(tile_1.Coordinates, tile_2.Coordinates);
    }

    /// <summary>
    /// Returns tiles in line between two points
    /// </summary>
    /// <param name="point_1"></param>
    /// <param name="point_2"></param>
    /// <returns></returns>
    public List<Tile> Get_Tiles_In_Line(Coordinates point_1, Coordinates point_2)
    {
        List<Tile> line = new List<Tile>();
        foreach(Coordinates c in Pathfinding.Straight_Line(point_1, point_2)) {
            line.Add(Get_Tile_At(c));
        }
        return line;
    }

    /// <summary>
    /// Are map's tile's GameObjects active?
    /// </summary>
    public bool Active
    {
        get {
            return Tile_GameObject.activeSelf;
        }
        set {
            if (Tile_GameObject.activeSelf == value) {
                return;
            }
            Tile_GameObject.SetActive(value);
        }
    }

    /// <summary>
    /// Deletes all tile GameObjects
    /// </summary>
    public void Delete()
    {
        if (State != MapState.Normal) {
            return;
        }
        for (int x = 0; x < Width; x++) {
            for (int y = 0; y < Height; y++) {
                tiles[x][y].Delete();
            }
        }
    }
}
