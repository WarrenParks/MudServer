using System;
using System.Collections.Generic;

namespace MudServer.Models
{
  // Represents a single tile or cell on the map
  public class MapTile
  {
    public int X { get; set; }
    public int Y { get; set; }
    public bool IsWalkable { get; set; }
    public string TerrainType { get; set; } // e.g., "Grass", "Water", "Wall"

    public MapTile(int x, int y, bool isWalkable, string terrainType)
    {
      X = x;
      Y = y;
      IsWalkable = isWalkable;
      TerrainType = terrainType;
    }
  }

  // Represents the 2D game map
  public class GameMap
  {
    public int Width { get; }
    public int Height { get; }
    private MapTile[,] tiles;

    public GameMap(int width, int height)
    {
      Width = width;
      Height = height;
      tiles = new MapTile[width, height];

      // Initialize with default tiles (e.g., all grass and walkable)
      for (int x = 0; x < width; x++)
      {
        for (int y = 0; y < height; y++)
        {
          tiles[x, y] = new MapTile(x, y, true, "Grass");
        }
      }
    }

    public MapTile GetTile(int x, int y)
    {
      if (x < 0 || x >= Width || y < 0 || y >= Height)
        return null;
      return tiles[x, y];
    }

    public void SetTile(int x, int y, MapTile tile)
    {
      if (x < 0 || x >= Width || y < 0 || y >= Height)
        throw new ArgumentOutOfRangeException();
      tiles[x, y] = tile;
    }
  }

  // Example usage
  public static class MapExample
  {
    public static void Run()
    {
      var map = new GameMap(10, 10);

      // Set a wall at (2,3)
      map.SetTile(2, 3, new MapTile(2, 3, false, "Wall"));

      // Print map info
      for (int y = 0; y < map.Height; y++)
      {
        for (int x = 0; x < map.Width; x++)
        {
          var tile = map.GetTile(x, y);
          Console.Write(tile.IsWalkable ? "." : "#");
        }
        Console.WriteLine();
      }
    }
  }
}