using System;

namespace BlockGame.Worlds.Generation
{
    public class BiomeBlockPalette
    {
        //(tile id, thickness)
        public (Tiles.Fg, int)[] fgLayers;
        public (Tiles.Bg, int)[] bgLayers;

        public Tiles.Fg GetFgPaletteTile(int depth)
        {
            if (fgLayers.Length == 0 || depth <= 0) return 0;
            else
            {
                int i = 0;
                while (depth >= 0 && i < fgLayers.Length)
                {
                    depth -= fgLayers[i].Item2;
                    if (depth > 0) i++;
                }
                return fgLayers[i].Item1;
            }
        }

        public Tiles.Bg GetBgPaletteTile(int depth)
        {
            if (bgLayers.Length == 0 || depth <= 0) return 0;
            else
            {
                int i = 0;
                while (depth >= 0 && i < bgLayers.Length)
                {
                    depth -= bgLayers[i].Item2;
                    if (depth > 0) i++;
                }
                return bgLayers[i].Item1;
            }
        }

        public BiomeBlockPalette() { }

        public BiomeBlockPalette((Tiles.Fg, int)[] fg, (Tiles.Bg, int)[] bg)
        {
            this.fgLayers = fg;
            this.bgLayers = bg;
        }
    }
}