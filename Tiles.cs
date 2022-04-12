namespace BlockGame
{
    public static class Tiles
    {
        public enum FgTile
        {
            // basic natural tiles
            air, grass, dirt, stone, sand, sandstone, limestone, clay,

            // building tiles
            woodPlanks, stoneBricks, bricks, limestoneBricks,

            // ores
            oreCoal, oreCopper, oreTin, oreAluminium, oreIron, oreSilver,

            // gemstones
            gemstoneSapphire, gemstoneAmethyst, gemstoneOpal, gemstoneEmerald, gemstoneRuby, gemstoneJadeite, gemstoneDiamond,
        }

        public enum BgTile
        {
            // basic natural bg tiles
            air, grass, dirt, stone, sandstone, log, leaves, logLeaves,

            // building bg tiles
            woodPlanks, stoneBricks
        }
    }
}