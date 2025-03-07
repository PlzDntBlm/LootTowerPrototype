using System;

namespace LootTowerPrototype.FloorGeneration
{
    public class FloorGenerator
    {
        private Random _random;

        public FloorGenerator(int seed)
        {
            // Use the seed for reproducible randomness
            _random = new Random(seed);
        }

        public Tile[,] GenerateFloor(int width, int height)
        {
            Tile[,] map = new Tile[height, width];

            // 1. Initialize all tiles as walls
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    map[y, x] = new Tile(true);
                }
            }

            // 2. Randomly pick half of the cells to be floors (just as an example)
            int floorCount = (width * height) / 2;
            for (int i = 0; i < floorCount; i++)
            {
                int randX = _random.Next(0, width);
                int randY = _random.Next(0, height);
                map[randY, randX].IsWall = false;
            }

            return map;
        }
    }
}
