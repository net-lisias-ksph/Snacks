using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.IO;

namespace Snacks
{
    public class SnacksProperties : GameParameters.CustomParameterNode
    {
        public static string SnacksResourceName = "Snacks";

        public enum RepLoss
        {
            Low,
            Medium,
            High
        }

        [GameParameters.CustomFloatParameterUI("Snacks per meal", minValue = 1.0f, maxValue = 10.0f, toolTip = "How much do kerbals snack on", autoPersistance = true)]
        public double snacksPerMeal = 1.0f;

        [GameParameters.CustomIntParameterUI("Meals per day", minValue = 1, maxValue = 3, stepSize = 1, toolTip = "How often do kerbals eat", autoPersistance = true)]
        public int mealsPerDay = 1;

        [GameParameters.CustomParameterUI("Some kerbals eat more, some eat less", toolTip = "At snack time, kerbals eat random amounts of Snacks", autoPersistance = true)]
        public bool enableRandomSnacking = true;

        [GameParameters.CustomParameterUI("Recyclers reduce snack consumption", toolTip = "Recyclers reduce the snacks eaten but consume ElectrcCharge", autoPersistance = true)]
        public bool enableRecyclers = true;

        [GameParameters.CustomParameterUI("Hungry kerbals hurt your reputation.", toolTip = "When kerbals go hungry, you lose Reputation", autoPersistance = true, gameMode = GameParameters.GameMode.CAREER)]
        public bool loseRepWhenHungry = true;

        [GameParameters.CustomParameterUI("Rep loss per kerbal per meal", toolTip = "How mad your kerbals will be", autoPersistance = true, gameMode = GameParameters.GameMode.CAREER)]
        public RepLoss repLostWhenHungry = RepLoss.Low;

        [GameParameters.CustomParameterUI("Hungry kerbals hurt your bottom line.", toolTip = "When kerbals go hungry, you lose Funds", autoPersistance = true, gameMode = GameParameters.GameMode.CAREER)]
        public bool loseFundsWhenHuntry = true;

        [GameParameters.CustomIntParameterUI("Fine per kerbal", maxValue = 50000, minValue = 1000, stepSize = 1000, toolTip = "How much is it gonna cost", autoPersistance = true, gameMode = GameParameters.GameMode.CAREER)]
        public int finePerKerbal = 1000;
        
//        [GameParameters.CustomParameterUI("Hungry kerbals can't fly straight.", toolTip = "When kerbals go hungry, ships partialy lose control", autoPersistance = true)]
        public bool partialControlWhenHungry = false;

        #region Properties
        public static int SnackResourceID
        {
            get
            {
                PartResourceDefinition snacksResource = PartResourceLibrary.Instance.GetDefinition("Snacks");
                return snacksResource.id;
            }
        }

        public static bool PartialControlWhenHungry
        {
            get
            {
                SnacksProperties snackProperties = HighLogic.CurrentGame.Parameters.CustomParams<SnacksProperties>();
                return snackProperties.partialControlWhenHungry;
            }
        }

        public static bool RecyclersEnabled
        {
            get
            {
                SnacksProperties snackProperties = HighLogic.CurrentGame.Parameters.CustomParams<SnacksProperties>();
                return snackProperties.enableRecyclers;
            }
        }

        public static bool EnableRandomSnacking
        {
            get
            {
                SnacksProperties snackProperties = HighLogic.CurrentGame.Parameters.CustomParams<SnacksProperties>();
                return snackProperties.enableRandomSnacking;
            }
        }

        public static double SnacksPerMeal
        {
            get
            {
                SnacksProperties snackProperties = HighLogic.CurrentGame.Parameters.CustomParams<SnacksProperties>();
                return snackProperties.snacksPerMeal;
            }
        }

        public static int MealsPerDay
        {
            get
            {
                SnacksProperties snackProperties = HighLogic.CurrentGame.Parameters.CustomParams<SnacksProperties>();
                return snackProperties.mealsPerDay;
            }
        }

        public static bool LoseFundsWhenHungry
        {
            get
            {
                SnacksProperties snackProperties = HighLogic.CurrentGame.Parameters.CustomParams<SnacksProperties>();
                return snackProperties.loseFundsWhenHuntry;
            }
        }

        public static double FinePerKerbal
        {
            get
            {
                SnacksProperties snackProperties = HighLogic.CurrentGame.Parameters.CustomParams<SnacksProperties>();
                return snackProperties.finePerKerbal;
            }
        }

        public static bool LoseRepWhenHungry
        {
            get
            {
                SnacksProperties snackProperties = HighLogic.CurrentGame.Parameters.CustomParams<SnacksProperties>();
                return snackProperties.loseRepWhenHungry;
            }
        }

        public static double RepLostWhenHungry
        {
            get
            {
                SnacksProperties snackProperties = HighLogic.CurrentGame.Parameters.CustomParams<SnacksProperties>();
                switch (snackProperties.repLostWhenHungry)
                {
                    case SnacksProperties.RepLoss.Low:
                    default:
                        return 0.0025;

                    case SnacksProperties.RepLoss.Medium:
                        return 0.005;

                    case SnacksProperties.RepLoss.High:
                        return 0.01;
                }
            }
        }
#endregion

        #region CustomParameterNode
        public override string Title
        {
            get
            {
                return "Snacking";
            }
        }

        public override string Section
        {
            get
            {
                return "Snacks!";
            }
        }

        public override int SectionOrder
        {
            get
            {
                return 1;
            }
        }

        public override void SetDifficultyPreset(GameParameters.Preset preset)
        {
            base.SetDifficultyPreset(preset);
        }

        public override GameParameters.GameMode GameMode
        {
            get
            {
                return GameParameters.GameMode.ANY;
            }
        }

        public override bool HasPresets
        {
            get
            {
                return true;
            }
        }

        public override bool Enabled(System.Reflection.MemberInfo member, GameParameters parameters)
        {
            Game.Modes gameMode = HighLogic.CurrentGame.Mode;

            if (member.Name == "repLoss" && loseRepWhenHungry)
                return true;
            else if (member.Name == "repLoss")
                return false;

            if (member.Name == "finePerKerbal" && loseFundsWhenHuntry)
                return true;
            else if (member.Name == "finePerKerbal")
                return false;

            return base.Enabled(member, parameters);
        }
        #endregion
    }
}
