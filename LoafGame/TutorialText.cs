using System;
using LoafGame.Scenes;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoafGame
{
    static class TutorialText
    {
        public static string[] GetText(Enums.GameType tutorialType)
        {
            return tutorialType switch
            {
                Enums.GameType.Carpentry => new[] 
                { 
                    "Drive all the nails in the shortest time possible",
                    "Left mouse button to spin the hammer clockwise",
                    "Right mouse button to spin the hammer counterclockwise",
                    "Faster spins result in harder hits"
                },
                Enums.GameType.Mining => new[] 
                { 
                    "Use the pickaxe to mine all the ores (rocks not required).",
                    "Left mouse button to swing the pickaxe clockwise",
                    "Right mouse button to swing the pickaxe counterclockwise",
                    "Change swing direction after hit to mine faster"
                },
                Enums.GameType.Cactus => new[]
                {
                    "Use the hand to harvest the required amount of cactus",
                    "Left mouse button to swing the hand clockwise",
                    "Right mouse button to swing the hand counterclockwise",
                    "Avoid cactus, -5 fruit when hit"
                },
                Enums.GameType.Wheat => new[]
                {
                    "Click on all regions in given order",
                    "First color is red, next color is green",
                    "Alternate colors after each click",
                    "Sub-10 seconds for real gamer hours"
                },
                _ => throw new ArgumentOutOfRangeException(nameof(tutorialType), tutorialType, null)
            };
        }
    }
}