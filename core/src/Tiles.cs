namespace BlockGame
{
    public static class Tiles
    {
        public enum Fg
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

        public enum Bg
        {
            // basic natural bg tiles
            air, dirt, stone, sandstone, log, leaves, logLeaves,

            // building bg tiles
            woodPlanks, stoneBricks
        }
    }
}