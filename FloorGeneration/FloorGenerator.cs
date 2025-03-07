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
            public bool IsEllipse { get; set; }

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
            // 1. Fill the map with walls
            Tile[,] map = new Tile[height, width];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    map[y, x] = new Tile(true);
                }
            }

            // 2. Decide how many rooms to attempt
            int desiredRoomCount = _random.Next(5, 10);
            List<Room> rooms = new List<Room>();

            // 3. For each room we want, try placing it without overlap
            for (int i = 0; i < desiredRoomCount; i++)
            {
                Room newRoom = TryCreateNonOverlappingRoom(width, height, rooms);
                if (newRoom != null)
                {
                    // Carve the newly placed room
                    CarveRoom(map, newRoom);
                    rooms.Add(newRoom);
                }
            }

            // 4. Sort the rooms by center X and connect them with corridors
            rooms.Sort((a, b) => a.CenterX.CompareTo(b.CenterX));
            for (int i = 0; i < rooms.Count - 1; i++)
            {
                ConnectRooms(map, rooms[i], rooms[i + 1]);
            }

            return map;
        }

        private Room TryCreateNonOverlappingRoom(int mapWidth, int mapHeight, List<Room> existingRooms)
        {
            const int MaxPlacementAttempts = 10;
            for (int attempt = 0; attempt < MaxPlacementAttempts; attempt++)
            {
                int rWidth = _random.Next(4, 10);
                int rHeight = _random.Next(4, 10);

                int rX = _random.Next(0, mapWidth - rWidth);
                int rY = _random.Next(0, mapHeight - rHeight);

                RoomTheme theme = GetRandomRoomTheme();
                bool isEllipse = (_random.NextDouble() < 0.5);

                Room candidate = new Room(rX, rY, rWidth, rHeight, theme, isEllipse);

                // Check overlap
                if (!DoesOverlap(candidate, existingRooms))
                {
                    return candidate;
                }
            }
            return null;
        }

        private bool DoesOverlap(Room candidate, List<Room> existingRooms)
        {
            foreach (Room r in existingRooms)
            {
                if (RectanglesOverlap(candidate.X, candidate.Y, candidate.Width, candidate.Height,
                                      r.X, r.Y, r.Width, r.Height))
                {
                    return true;
                }
            }
            return false;
        }

        private bool RectanglesOverlap(int x1, int y1, int w1, int h1,
                                       int x2, int y2, int w2, int h2)
        {
            // If one rectangle is to the left of the other
            if (x1 + w1 <= x2 || x2 + w2 <= x1) return false;
            // If one rectangle is above the other
            if (y1 + h1 <= y2 || y2 + h2 <= y1) return false;

            return true; // otherwise, they overlap
        }

        private void CarveRoom(Tile[,] map, Room room)
        {
            if (!room.IsEllipse)
            {
                // Rectangular
                for (int y = room.Y; y < room.Y + room.Height; y++)
                {
                    for (int x = room.X; x < room.X + room.Width; x++)
                    {
                        map[y, x].IsWall = false;
                        map[y, x].Theme = room.Theme;
                    }
                }
            }
            else
            {
                // Elliptical approximation
                float rx = room.Width / 2f;
                float ry = room.Height / 2f;
                float cx = room.X + rx;
                float cy = room.Y + ry;

                for (int y = room.Y; y < room.Y + room.Height; y++)
                {
                    for (int x = room.X; x < room.X + room.Width; x++)
                    {
                        float dx = x - cx;
                        float dy = y - cy;
                        if ((dx * dx) / (rx * rx) + (dy * dy) / (ry * ry) <= 1.0f)
                        {
                            map[y, x].IsWall = false;
                            map[y, x].Theme = room.Theme;
                        }
                    }
                }
            }
        }

        // Corridor carve that won't overwrite an existing floor's theme
        private void ConnectRooms(Tile[,] map, Room r1, Room r2)
        {
            int x1 = r1.CenterX;
            int y1 = r1.CenterY;
            int x2 = r2.CenterX;
            int y2 = r2.CenterY;

            // Horizontal carve
            for (int x = Math.Min(x1, x2); x <= Math.Max(x1, x2); x++)
            {
                if (map[y1, x].IsWall)
                {
                    map[y1, x].IsWall = false;
                    map[y1, x].Theme = RoomTheme.None;
                }
            }

            // Vertical carve
            for (int y = Math.Min(y1, y2); y <= Math.Max(y1, y2); y++)
            {
                if (map[y, x2].IsWall)
                {
                    map[y, x2].IsWall = false;
                    map[y, x2].Theme = RoomTheme.None;
                }
            }
        }

        private RoomTheme GetRandomRoomTheme()
        {
            int t = _random.Next(1, 5); // 1..4
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
