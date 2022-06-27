using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text;
using BlockGame.Worlds;

namespace BlockGame.Rendering
{
    public class WorldLightingManager
    {
        private readonly World world;

        public void CalculateSmoothLightmaps(List<WorldChunk> chunks)
        {
            for (int i = 0; i < chunks.Count; i++)
            {
                chunks[i].renderer.LoadSmoothLightmap();
            }
        }

        public class LightmapLoadEventArgs : EventArgs
        {
            public List<WorldChunk> Chunks { get; set; }

            public LightmapLoadEventArgs(List<WorldChunk> chunks)
            {
                Chunks = chunks;
            }
        }

        public WorldLightingManager(World world)
        {
            this.world = world;
        }
    }
}
