using System;
using System.Collections.Generic;

namespace LootTowerPrototype.FloorGeneration
{
    public class FloorGenerator
    {
        private Random _random;

        // Represents a room within the grid
        private class Room
        {
            public int X, Y, Width, Height;
            public int CenterX => X + Width / 2;
            public int CenterY => Y + Height / 2;
            public RoomTheme Theme { get; set; }
            public bool IsEllipse { get; set; } // If true, carve elliptical shape

            public Room(int x, int y, int width, int height,
                        RoomTheme theme, bool isEllipse)
            {
                X = x;
                Y = y;
                Width = width;
                Height = height;
                Theme = theme;
                IsEllipse = isEllipse;
            }
        }

        public FloorGenerator(int seed)
        {
            _random = new Random(seed);
        }

        public Tile[,] GenerateFloor(int width, int height)
        {
            // 1. Fill map with walls
            Tile[,] map = new Tile[height, width];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    map[y, x] = new Tile(true); // All walls initially
                }
            }

            // 2. Decide how many rooms to create
            int roomCount = _random.Next(5, 10); 
            List<Room> rooms = new List<Room>();

            // 3. Generate each room
            for (int i = 0; i < roomCount; i++)
            {
                int rWidth = _random.Next(4, 10);
                int rHeight = _random.Next(4, 10);

                int rX = _random.Next(0, width - rWidth);
                int rY = _random.Next(0, height - rHeight);

                // Random theme from the set, ignoring "None" for actual rooms
                RoomTheme theme = GetRandomRoomTheme();

                // 50/50 chance to be elliptical or rectangular
                bool isEllipse = (_random.NextDouble() < 0.5);

                Room newRoom = new Room(rX, rY, rWidth, rHeight, theme, isEllipse);
                CarveRoom(map, newRoom);

                rooms.Add(newRoom);
            }

            // 4. Sort the rooms by center X and connect them with corridors
            rooms.Sort((a, b) => a.CenterX.CompareTo(b.CenterX));
            for (int i = 0; i < rooms.Count - 1; i++)
            {
                ConnectRooms(map, rooms[i], rooms[i + 1]);
            }

            return map;
        }

        private void CarveRoom(Tile[,] map, Room room)
        {
            if (!room.IsEllipse)
            {
                // Rectangular room
                for (int y = room.Y; y < room.Y + room.Height; y++)
                {
                    for (int x = room.X; x < room.X + room.Width; x++)
                    {
                        map[y, x].IsWall = false;
                        // Assign the tile's theme to match the room's
                        map[y, x].Theme = room.Theme;
                    }
                }
            }
            else
            {
                // Elliptical approximation
                float rx = room.Width / 2f;   // horizontal radius
                float ry = room.Height / 2f;  // vertical radius
                float cx = room.X + rx;       // center X
                float cy = room.Y + ry;       // center Y

                for (int y = room.Y; y < room.Y + room.Height; y++)
                {
                    for (int x = room.X; x < room.X + room.Width; x++)
                    {
                        // Check ellipse equation (x-cx)^2 / rx^2 + (y-cy)^2 / ry^2 <= 1
                        float dx = (x - cx);
                        float dy = (y - cy);
                        if ((dx * dx) / (rx * rx) + (dy * dy) / (ry * ry) <= 1.0f)
                        {
                            map[y, x].IsWall = false;
                            map[y, x].Theme = room.Theme;
                        }
                    }
                }
            }
        }

        private void ConnectRooms(Tile[,] map, Room r1, Room r2)
        {
            // Simple L-shaped corridor from r1 center to r2 center
            int x1 = r1.CenterX;
            int y1 = r1.CenterY;
            int x2 = r2.CenterX;
            int y2 = r2.CenterY;

            // Horizontal carve
            for (int x = Math.Min(x1, x2); x <= Math.Max(x1, x2); x++)
            {
                map[y1, x].IsWall = false;
                map[y1, x].Theme = RoomTheme.None; // corridor is unthemed
            }

            // Vertical carve
            for (int y = Math.Min(y1, y2); y <= Math.Max(y1, y2); y++)
            {
                map[y, x2].IsWall = false;
                map[y, x2].Theme = RoomTheme.None; // corridor is unthemed
            }
        }

        private RoomTheme GetRandomRoomTheme()
        {
            // Weighted random selection from these four
            int t = _random.Next(1, 5); // between 1..4
            switch (t)
            {
                case 1: return RoomTheme.Library;
                case 2: return RoomTheme.Prison;
                case 3: return RoomTheme.Cave;
                default: return RoomTheme.Generic;
            }
        }
    }
}