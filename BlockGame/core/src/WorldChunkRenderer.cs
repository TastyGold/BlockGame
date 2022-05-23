using Raylib_cs;

namespace BlockGame
{
    public class WorldChunkRenderer
    {
        public WorldChunk chunk;
        public Texture2D lightmapTexture;
        private bool isTextureLoaded = false;

        public void LoadSmoothLightmap()
        {
            if (isTextureLoaded) Raylib.UnloadTexture(lightmapTexture);
            lightmapTexture = SmoothLightmapGenerator.GetSmoothLightmap(chunk.lightManager.Get18x18Lightmap()); 
            isTextureLoaded = true;
        }

        public void UnloadLightmapTexture()
        {
            isTextureLoaded = false;
            Raylib.UnloadTexture(lightmapTexture);
        }
    }
}