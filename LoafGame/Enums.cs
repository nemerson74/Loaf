using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoafGame
{
    public class Enums
    {
        public enum TileType
        {
            Grassland,
            Badland,
            Desert,
            Forest
        }
        public enum GameType
        {
            Carpentry,
            Mining,
            Cactus,
            Wheat
        }
        public static readonly float[] CARPENTRY_LIMITS = { 30f, 45f, 70f };
        public static readonly float[] MINING_LIMITS = { 20f, 35f, 55f };
        public static readonly float[] CACTUS_LIMITS = { 40f, 55f, 70f };
        public static readonly float[] WHEAT_LIMITS = { 15f, 25f, 55f };
    }
}