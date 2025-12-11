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

        public bool IsComplete()
        {
            return ForestPoints >= 1 && GrasslandPoints >= 1 && DesertPoints >= 1 && BadlandPoints >= 1;
        }

        public int GetTotalScore()
        {
            return ForestPoints + GrasslandPoints + DesertPoints + BadlandPoints;
        }

        public int[] GetScores() { return new int[] { ForestPoints, GrasslandPoints, DesertPoints, BadlandPoints }; }

        public void TakeScores (int[] scores)
        {
            ForestPoints = scores[0];
            GrasslandPoints = scores[1];
            DesertPoints = scores[2];
            BadlandPoints = scores[3];
        }
}
}