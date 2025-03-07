using System;
using LootTowerPrototype.FloorGeneration;

namespace LootTowerPrototype
{
    class Program
    {
        static void Main(string[] args)
        {
            // Optional: parse a seed from the command line
            int seed = 12345;
            if (args.Length > 0 && int.TryParse(args[0], out int parsedSeed))
            {
                seed = parsedSeed;
            }

            // Instantiate FloorGenerator with our chosen seed
            FloorGenerator generator = new FloorGenerator(seed);

            // Generate a floor with multi-room and corridor approach
            Tile[,] floor = generator.GenerateFloor(width: 40, height: 20);

            // Print the result in ASCII
            for (int y = 0; y < floor.GetLength(0); y++)
            {
                for (int x = 0; x < floor.GetLength(1); x++)
                {
                    Console.Write(floor[y, x].IsWall ? "#" : ".");
                }
                Console.WriteLine();
            }

            // Pause so the console stays open
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}