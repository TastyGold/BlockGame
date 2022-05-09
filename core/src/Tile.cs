namespace BlockGame
{
    public class Tile
    {
        public byte FgTile = 0;
        public byte BgTile = 0;

        public Tile(int tile)
        {
            FgTile = (byte)tile;
        }
        public Tile(byte fg, byte bg)
        {
            FgTile = fg;
            BgTile = bg;
        }
    }
}