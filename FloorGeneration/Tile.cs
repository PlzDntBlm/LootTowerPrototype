namespace LootTowerPrototype.FloorGeneration
{
    // RoomTheme enumerates possible decorative or thematic categories
    public enum RoomTheme
    {
        None,       // For corridors or unthemed areas
        Library,
        Prison,
        Cave,
        Generic
    }

    public class Tile
    {
        public bool IsWall { get; set; }
        public RoomTheme Theme { get; set; } // New property for theming

        public Tile(bool isWall)
        {
            IsWall = isWall;
            Theme = RoomTheme.None;
        }
    }
}
