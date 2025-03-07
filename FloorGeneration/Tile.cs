namespace LootTowerPrototype.FloorGeneration
{
    public enum RoomTheme
    {
        None,
        Library,
        Prison,
        Cave,
        Generic
    }

    public class Tile
    {
        public bool IsWall { get; set; }
        public RoomTheme Theme { get; set; }

        public Tile(bool isWall)
        {
            IsWall = isWall;
            Theme = RoomTheme.None;
        }
    }
}
