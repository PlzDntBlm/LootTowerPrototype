using System;
using LootTowerPrototype.FloorGeneration;

namespace LootTowerPrototype
{
    class Program
    {
        static void Main(string[] args)
        {
            // Optional: parse seed
            int seed = 12345;
            if (args.Length > 0 && int.TryParse(args[0], out int parsedSeed))
            {
                seed = parsedSeed;
            }

            FloorGenerator generator = new FloorGenerator(seed);

            // Let's use a bigger map for variety
            Tile[,] floor = generator.GenerateFloor(width: 50, height: 20);

            // Print the layout with theme-based symbols
            for (int y = 0; y < floor.GetLength(0); y++)
            {
                for (int x = 0; x < floor.GetLength(1); x++)
                {
                    if (floor[y, x].IsWall)
                    {
                        // Walls
                        Console.Write("#");
                    }
                    else
                    {
                        // The floor tile has a theme
                        switch (floor[y, x].Theme)
                        {
                            case RoomTheme.Library: Console.Write("L"); break;
                            case RoomTheme.Prison:  Console.Write("P"); break;
                            case RoomTheme.Cave:    Console.Write("C"); break;
                            case RoomTheme.Generic: Console.Write("G"); break;
                            default:                Console.Write("."); break; // corridor or None
                        }
                    }
                }
                Console.WriteLine();
            }

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}