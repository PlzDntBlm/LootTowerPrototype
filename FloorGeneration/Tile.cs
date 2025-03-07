namespace LootTowerPrototype.FloorGeneration
{
    public class Tile
    {
        public bool IsWall { get; set; }

        public Tile(bool isWall)
        {
            IsWall = isWall;
        }
    }
}
