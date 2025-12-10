using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoafGame
{
    public class ScoreTracker
    {
        public ScoreTracker() { }
        public int ForestPoints { get; set; } = 0;
        public int GrasslandPoints { get; set; } = 0;
        public int DesertPoints { get; set; } = 0;
        public int BadlandPoints { get; set; } = 0;

        public int[] GetScores() { return new int[] { ForestPoints, GrasslandPoints, DesertPoints, BadlandPoints }; }
    }
}