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
        public static string[] GetText(TutorialScene.TutorialType tutorialType)
        {
            return tutorialType switch
            {
                TutorialScene.TutorialType.Carpentry => new[] 
                { 
                    "Drive all the nails in the shortest time possible",
                    "Left mouse button to spin the hammer clockwise",
                    "Right mouse button to spin the hammer counterclockwise",
                    "Faster spins result in harder hits"
                },
                TutorialScene.TutorialType.Mining => new[] 
                { 
                    "Use the pickaxe to mine all the ores (rocks not required).",
                    "Left mouse button to swing the pickaxe clockwise",
                    "Right mouse button to swing the pickaxe counterclockwise",
                    "Change swing direction after hit to mine faster"
                },
                TutorialScene.TutorialType.Cactus => new[]
                {
                    "Use the pickaxe to mine all the ores (rocks not required).",
                    "Left mouse button to swing the pickaxe clockwise",
                    "Right mouse button to swing the pickaxe counterclockwise",
                    "Alternate swing direction after hit to take advantage of momentum"
                },
                TutorialScene.TutorialType.Wheat => new[]
                {
                    "Use the scythe to harvest all the crops (weeds not required).",
                    "Left mouse button to swing the scythe clockwise",
                    "Right mouse button to swing the scythe counterclockwise",
                    "Alternate swing direction after hit to take advantage of momentum"
                },
                _ => throw new ArgumentOutOfRangeException(nameof(tutorialType), tutorialType, null)
            };
        }
    }
}
