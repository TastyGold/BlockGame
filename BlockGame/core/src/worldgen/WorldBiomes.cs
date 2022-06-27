namespace BlockGame.Worlds.Generation
{
    public static partial class WorldBiomes
    {
        public static Biome plains, forest, desert, plateau, ocean, deepocean, mountains, peaks;

        public static BiomeBlockPalette defaultPalette = new BiomeBlockPalette()
        {
            fgLayers = new (Tiles.Fg, int)[3]
            {
                (Tiles.Fg.grass, 1),
                (Tiles.Fg.dirt, 8),
                (Tiles.Fg.stone, 999)
            },
            bgLayers = new(Tiles.Bg, int)[2]
            {
                (Tiles.Bg.dirt, 9),
                (Tiles.Bg.stone, 999)
            }
        };
        public static BiomeBlockPalette sandyPalette = new BiomeBlockPalette()
        {
            fgLayers = new (Tiles.Fg, int)[3]
            {
                (Tiles.Fg.sand, 7),
                (Tiles.Fg.sandstone, 100),
                (Tiles.Fg.stone, 999)
            },
            bgLayers = new (Tiles.Bg, int)[3]
            {
                (Tiles.Bg.air, 4),
                (Tiles.Bg.sandstone, 103),
                (Tiles.Bg.stone, 999)
            }
        };


        public static Biome GetBiome(int index) => index switch
        {
            0 => plains,
            1 => forest,
            2 => desert,
            3 => plateau,
            4 => ocean,
            5 => deepocean,
            6 => mountains,
            7 => peaks,
            _ => plains
        };

        public static void Load()
        {
            plains = new Biome()
            {
                biomeIndex = 0,
                groundHeight = 15,
                noiseAmplitude = 0.05f,
                primaryNoiseScale = 3,
                //noiseIterations = 2,
                //noiseLacunarity = 2,
                //caveStartHeight = 30,
                //caveWidth = 6,
                palette = defaultPalette,
            };
            forest = new Biome()
            {
                biomeIndex = 1,
                groundHeight = 20,
                primaryNoiseScale = 3,
                noiseAmplitude = 0.05f,
                palette = defaultPalette,
            };
            desert = new Biome()
            {
                biomeIndex = 2,
                groundHeight = 10,
                primaryNoiseScale = 3,
                noiseAmplitude = 0.03f,
                //primaryNoiseScale = 3,
                //noiseIterations = 2,
                //noiseLacunarity = 2,
                //caveStartHeight = 30,
                //caveWidth = 6,
                palette = sandyPalette
            };
            plateau = new Biome()
            {
                biomeIndex = 3,
                groundHeight = 30,
                primaryNoiseScale = 3,
                noiseAmplitude = 0.05f,
                palette = defaultPalette,
            };
            ocean = new Biome()
            {
                biomeIndex = 4,
                primaryNoiseScale = 3,
                groundHeight = -70,
                noiseAmplitude = 0.01f,
                palette = sandyPalette
            };
            deepocean = new Biome()
            {
                biomeIndex = 5,
                groundHeight = -100,
                primaryNoiseScale = 3,
                noiseAmplitude = 0.05f,
                palette = sandyPalette
            };
            mountains = new Biome()
            {
                biomeIndex = 6,
                groundHeight = 50,
                primaryNoiseScale = 3,
                noiseAmplitude = 0.05f,
                palette = defaultPalette,
            };
            peaks = new Biome()
            {
                biomeIndex = 7,
                groundHeight = 100,
                primaryNoiseScale = 3,
                noiseAmplitude = 0.05f,
                palette = defaultPalette,
            };
        }
    }
}