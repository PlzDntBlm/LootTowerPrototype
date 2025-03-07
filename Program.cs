using System;
using LootTowerPrototype.FloorGeneration;

namespace LootTowerPrototype
{
    class Program
    {
        static void Main(string[] args)
        {
            // Optionally parse a seed from the command line, default is 12345
            int seed = 12345;
            if (args.Length > 0 && int.TryParse(args[0], out int parsedSeed))
            {
                seed = parsedSeed;
            }

            // Create a FloorGenerator with the chosen seed
            FloorGenerator generator = new FloorGenerator(seed);

            // Generate a floor of size 20 x 10
            // (You can tweak these numbers for different shapes)
            Tile[,] floor = generator.GenerateFloor(20, 10);

            // Print the generated floor in ASCII
            for (int y = 0; y < floor.GetLength(0); y++)
            {
                for (int x = 0; x < floor.GetLength(1); x++)
                {
                    if (floor[y, x].IsWall)
                    {
                        // Print '#' for walls
                        Console.Write("#");
                    }
                    else
                    {
                        // Print '.' for floors
                        Console.Write(".");
                    }
                }
                Console.WriteLine(); // New line after each row
            }

            // Pause (optional): Press any key to exit
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}
