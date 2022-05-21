using Raylib_cs;

namespace BlockGame
{
    public class WorldChunkRenderer
    {
        public WorldChunk chunk;
        public Texture2D lightmapTexture;

        public void LoadLightmapTexture()
        {
            Image i = SmoothLightmapGenerator.GetSmoothLightmap(chunk.lightManager.Get18x18Lightmap());
            lightmapTexture = Raylib.LoadTextureFromImage(i);
            Raylib.UnloadImage(i);
        }

        public void UnloadLightmapTexture()
        {
            Raylib.UnloadTexture(lightmapTexture);
        }
    }
}