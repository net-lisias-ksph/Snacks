using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.IO;

namespace Snacks
{
    public enum RepLoss
    {
        Low,
        Medium,
        High
    }

    public class SnackPenalties : GameParameters.CustomParameterNode
    {
        [GameParameters.CustomParameterUI("Enable random penalties", toolTip = "If enabled, then one of the enabled penalties will be randomly chosen", autoPersistance = true, gameMode = GameParameters.GameMode.CAREER)]
        public bool enableRandomPenalties = true;

        [GameParameters.CustomParameterUI("Hungry kerbals hurt your reputation.", toolTip = "When kerbals go hungry, you lose Reputation", autoPersistance = true, gameMode = GameParameters.GameMode.CAREER)]
        public bool loseRepWhenHungry = true;

        [GameParameters.CustomParameterUI("Rep loss per kerbal per meal", toolTip = "How mad your kerbals will be", autoPersistance = true, gameMode = GameParameters.GameMode.CAREER)]
        public RepLoss repLostWhenHungry = RepLoss.Low;

        [GameParameters.CustomParameterUI("Hungry kerbals hurt your bottom line.", toolTip = "When kerbals go hungry, you lose Funds", autoPersistance = true, gameMode = GameParameters.GameMode.CAREER)]
        public bool loseFundsWhenHuntry = true;

        [GameParameters.CustomIntParameterUI("Fine per kerbal", maxValue = 50000, minValue = 1000, stepSize = 1000, toolTip = "How much is it gonna cost", autoPersistance = true, gameMode = GameParameters.GameMode.CAREER)]
        public int finePerKerbal = 10000;

        [GameParameters.CustomParameterUI("Hungry kerbals ruin science.", toolTip = "When kerbals go hungry, they ruin stored experiment results and data", autoPersistance = true, gameMode = GameParameters.GameMode.CAREER | GameParameters.GameMode.SCIENCE)]
        public bool loseScienceWhenHungry = true;

        [GameParameters.CustomParameterUI("Data lost", toolTip = "What percentage of lab data is lost", autoPersistance = true, gameMode = GameParameters.GameMode.CAREER | GameParameters.GameMode.SCIENCE)]
        public RepLoss dataLostWhenHungry = RepLoss.Low;

        [GameParameters.CustomParameterUI("Hungry kerbals can't fly straight.", toolTip = "When kerbals go hungry, ships partialy lose control", autoPersistance = true)]
        public bool partialControlWhenHungry = true;

        #region CustomParameterNode
        public override string Section
        {
            get
            {
                return "Snacks!";
            }
        }

        public override string Title
        {
            get
            {
                return "Penalties";
            }
        }

        public override int SectionOrder
        {
            get
            {
                return 2;
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

    public class SnacksProperties : GameParameters.CustomParameterNode
    {
        public static string SnacksResourceName = "Snacks";
        public static string SoilResourceName = "Soil";
        public static int SnacksPerCommand = 50;
        public static int SnacksPerCrewModule = 400;

        [GameParameters.CustomFloatParameterUI("Snacks per meal", minValue = 1.0f, maxValue = 10.0f, toolTip = "How much do kerbals snack on", autoPersistance = true)]
        public double snacksPerMeal = 1.0f;

        [GameParameters.CustomIntParameterUI("Meals per day", minValue = 1, maxValue = 3, stepSize = 1, toolTip = "How often do kerbals eat", autoPersistance = true)]
        public int mealsPerDay = 1;

        [GameParameters.CustomParameterUI("Some kerbals eat more, some eat less", toolTip = "At snack time, kerbals eat random amounts of Snacks, and the time between snacking is also random", autoPersistance = true)]
        public bool enableRandomSnacking = true;

        [GameParameters.CustomParameterUI("Enable recycling", toolTip = "Kerbals produce Soil when snacking. Recyclers convert the Soil back to Snacks at the cost of ElectricCharge", autoPersistance = true)]
        public bool enableRecyclers = true;
        
        [GameParameters.CustomFloatParameterUI("Recycler Efficiency", minValue = 0.1f, maxValue = 0.8f, asPercentage = true, toolTip = "How well does the recycler recyle", autoPersistance = true)]
        public float recyclerEfficiency = 0.4f;

        public float productionEfficiency = 1.0f;

        [GameParameters.CustomParameterUI("Show time in years, months, and days", toolTip = "If disabled, time estimates are in days only", autoPersistance = true)]
        public bool yearsMonthsDaysEnabled = true;

        #region Properties

        public static float ProductionEfficiency
        {
            get
            {
                SnacksProperties snackProperties = HighLogic.CurrentGame.Parameters.CustomParams<SnacksProperties>();
                return snackProperties.productionEfficiency;
            }
        }

        public static bool ShowTimeInYMD
        {
            get
            {
                SnacksProperties snackProperties = HighLogic.CurrentGame.Parameters.CustomParams<SnacksProperties>();
                return snackProperties.yearsMonthsDaysEnabled;
            }
        }

        public static int SnackResourceID
        {
            get
            {
                PartResourceDefinition snacksResource = PartResourceLibrary.Instance.GetDefinition("Snacks");
                return snacksResource.id;
            }
        }

        public static bool RandomPenaltiesEnabled
        {
            get
            {
                SnackPenalties penalties = HighLogic.CurrentGame.Parameters.CustomParams<SnackPenalties>();
                return penalties.enableRandomPenalties;
            }
        }

        public static bool LoseScienceWhenHungry
        {
            get
            {
                SnackPenalties penalties = HighLogic.CurrentGame.Parameters.CustomParams<SnackPenalties>();
                return penalties.loseScienceWhenHungry;
            }
        }

        public static float DataLostWhenHungry
        {
            get
            {
                SnackPenalties penalties = HighLogic.CurrentGame.Parameters.CustomParams<SnackPenalties>();

                switch (penalties.dataLostWhenHungry)
                {
                    case RepLoss.Low:
                    default:
                        return 0.05f;

                    case RepLoss.Medium:
                        return 0.1f;

                    case RepLoss.High:
                        return 0.2f;
                }
            }
        }

        public static bool PartialControlWhenHungry
        {
            get
            {
                SnackPenalties penalties = HighLogic.CurrentGame.Parameters.CustomParams<SnackPenalties>();
                return penalties.partialControlWhenHungry;
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

        public static float RecyclerEfficiency
        {
            get
            {
                SnacksProperties snackProperties = HighLogic.CurrentGame.Parameters.CustomParams<SnacksProperties>();
                return snackProperties.recyclerEfficiency;
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
                SnackPenalties penalties = HighLogic.CurrentGame.Parameters.CustomParams<SnackPenalties>();
                return penalties.loseFundsWhenHuntry;
            }
        }

        public static double FinePerKerbal
        {
            get
            {
                SnackPenalties penalties = HighLogic.CurrentGame.Parameters.CustomParams<SnackPenalties>();
                return penalties.finePerKerbal;
            }
        }

        public static bool LoseRepWhenHungry
        {
            get
            {
                SnackPenalties penalties = HighLogic.CurrentGame.Parameters.CustomParams<SnackPenalties>();
                return penalties.loseRepWhenHungry;
            }
        }

        public static double RepLostWhenHungry
        {
            get
            {
                SnackPenalties penalties = HighLogic.CurrentGame.Parameters.CustomParams<SnackPenalties>();
                switch (penalties.repLostWhenHungry)
                {
                    case RepLoss.Low:
                    default:
                        return 0.0025;

                    case RepLoss.Medium:
                        return 0.005;

                    case RepLoss.High:
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
            if (member.Name == "recyclerEfficiency" && enableRecyclers)
                return true;
            else if (member.Name == "recyclerEfficiency")
                return false;

            return base.Enabled(member, parameters);
        }

        #endregion
    }
}
