using System;
using System.Collections.Generic;

namespace LootTowerPrototype.FloorGeneration
{
    public class FloorGenerator
    {
        private Random _random;

        // Represents a rectangular room within the grid
        private class Room
        {
            public int X, Y, Width, Height;
            public int CenterX => X + Width / 2;
            public int CenterY => Y + Height / 2;

            public Room(int x, int y, int width, int height)
            {
                X = x;
                Y = y;
                Width = width;
                Height = height;
            }
        }

        public FloorGenerator(int seed)
        {
            _random = new Random(seed);
        }

        public Tile[,] GenerateFloor(int width, int height)
        {
            // 1. Create a 2D array filled with walls
            Tile[,] map = new Tile[height, width];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    map[y, x] = new Tile(true); // true => wall
                }
            }

            // 2. Decide how many rooms to create
            int roomCount = _random.Next(5, 10); // for example, 5 to 9 rooms
            List<Room> rooms = new List<Room>();

            // 3. Generate each room with random position and size
            for (int i = 0; i < roomCount; i++)
            {
                int rWidth = _random.Next(3, 8);  // random room width
                int rHeight = _random.Next(3, 8); // random room height

                int rX = _random.Next(0, width - rWidth);
                int rY = _random.Next(0, height - rHeight);

                Room newRoom = new Room(rX, rY, rWidth, rHeight);

                // Carve the room (turn walls into floors)
                CarveRoom(map, newRoom);

                // Add to list so we can connect rooms with corridors later
                rooms.Add(newRoom);
            }

            // 4. Connect all rooms in a simple chain
            //    Sort them by the center for a consistent corridor path
            rooms.Sort((a, b) => a.CenterX.CompareTo(b.CenterX));

            for (int i = 0; i < rooms.Count - 1; i++)
            {
                Room r1 = rooms[i];
                Room r2 = rooms[i + 1];

                // Connect r1 to r2 via corridors
                CarveCorridor(map, r1.CenterX, r1.CenterY, r2.CenterX, r2.CenterY);
            }

            return map;
        }

        private void CarveRoom(Tile[,] map, Room room)
        {
            for (int y = room.Y; y < room.Y + room.Height; y++)
            {
                for (int x = room.X; x < room.X + room.Width; x++)
                {
                    map[y, x].IsWall = false; 
                }
            }
        }

        // Simple corridor: carve a horizontal line, then a vertical line
        // (or vice versa) from (x1,y1) to (x2,y2)
        private void CarveCorridor(Tile[,] map, int x1, int y1, int x2, int y2)
        {
            // First carve horizontally from x1 to x2
            for (int x = Math.Min(x1, x2); x <= Math.Max(x1, x2); x++)
            {
                map[y1, x].IsWall = false;
            }

            // Then carve vertically from y1 to y2
            for (int y = Math.Min(y1, y2); y <= Math.Max(y1, y2); y++)
            {
                map[y, x2].IsWall = false;
            }
        }
    }
}