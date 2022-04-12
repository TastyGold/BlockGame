using Raylib_cs;

namespace BlockGame
{
    public class WorldChunkRenderer
    {
        public RenderTexture2D renderTexture;

        public void UpdateRenderTexture(WorldChunk c)
        {
            Raylib.BeginDrawing();
            Raylib.BeginTextureMode(renderTexture);

            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < 16; x++)
                {

                }
            }

            Raylib.EndTextureMode();
            Raylib.EndDrawing();
        }
    }
}