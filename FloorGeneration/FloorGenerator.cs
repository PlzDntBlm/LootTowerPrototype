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
            // We no longer rely strictly on center-based connection,
            // but still store them for reference or fallback.
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

            // Pick a random point within the room area
            public (int x, int y) GetRandomPointInside(Random rng)
            {
                int rx = X + rng.Next(Width);
                int ry = Y + rng.Next(Height);
                return (rx, ry);
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

            // 4. Connect rooms
            // Sort by center X just as a basic approach
            rooms.Sort((a, b) => a.CenterX.CompareTo(b.CenterX));

            // Create at least one corridor between consecutive rooms
            for (int i = 0; i < rooms.Count - 1; i++)
            {
                ConnectRoomsInInterestingWay(map, rooms[i], rooms[i + 1]);
            }

            // Optional: chance to add a second corridor between random pairs
            // for more interconnectivity
            int extraConnections = _random.Next(0, rooms.Count / 2);
            for (int i = 0; i < extraConnections; i++)
            {
                var r1 = rooms[_random.Next(rooms.Count)];
                var r2 = rooms[_random.Next(rooms.Count)];
                if (r1 != r2)
                {
                    // Low chance to connect them, but only if they are not too close
                    // or the same instance. Just a fun random link.
                    ConnectRoomsInInterestingWay(map, r1, r2);
                }
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

        /// <summary>
        /// Connect two rooms using a more interesting path approach,
        /// optionally including some random wiggling or diagonal movement.
        /// </summary>
        private void ConnectRoomsInInterestingWay(Tile[,] map, Room r1, Room r2)
        {
            // Randomly pick points inside each room, not just the center
            (int startX, int startY) = r1.GetRandomPointInside(_random);
            (int endX, int endY) = r2.GetRandomPointInside(_random);

            // We'll do a quick random walk approach from start to end
            // that can occasionally step diagonally or deviate slightly.
            int x = startX;
            int y = startY;

            // We'll iterate a maximum number of steps to avoid infinite loops
            int maxSteps = map.GetLength(0) * map.GetLength(1);

            while ((x != endX || y != endY) && maxSteps-- > 0)
            {
                // Carve current
                if (map[y, x].IsWall)
                {
                    map[y, x].IsWall = false;
                    map[y, x].Theme = RoomTheme.None;
                }

                int dx = endX - x;
                int dy = endY - y;

                // Step direction tries to reduce both dx, dy
                // Introduce a small random chance to deviate or use diagonal
                int stepX = Math.Sign(dx);
                int stepY = Math.Sign(dy);

                bool allowDiagonal = (_random.NextDouble() < 0.2); // 20% chance to do diagonal
                if (allowDiagonal && stepX != 0 && stepY != 0)
                {
                    // Step diagonally
                    x += stepX;
                    y += stepY;
                }
                else
                {
                    // Weighted random: horizontal or vertical first
                    if (Math.Abs(dx) > Math.Abs(dy))
                    {
                        x += stepX;
                    }
                    else
                    {
                        y += stepY;
                    }
                }

                // Additional random small deviation
                if (_random.NextDouble() < 0.1) // 10% chance
                {
                    // Try a small random turn
                    int turnDirection = _random.Next(4);
                    switch (turnDirection)
                    {
                        case 0: if (y > 0) y -= 1; break;
                        case 1: if (y < map.GetLength(0) - 1) y += 1; break;
                        case 2: if (x > 0) x -= 1; break;
                        case 3: if (x < map.GetLength(1) - 1) x += 1; break;
                    }
                }

                // Clamp
                x = Math.Max(0, Math.Min(x, map.GetLength(1) - 1));
                y = Math.Max(0, Math.Min(y, map.GetLength(0) - 1));
            }

            // Carve final position
            if (map[y, x].IsWall)
            {
                map[y, x].IsWall = false;
                map[y, x].Theme = RoomTheme.None;
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