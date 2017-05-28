using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.IO;

namespace Snacks
{
    public class SoilRecycler : SnackProcessor
    {
        [KSPField()]
        public int RecyclerCapacity;

        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            Fields["dailyOutput"].guiName = "Max Recycling";
        }

        public override double GetDailySnacksOutput()
        {
            productionEfficiency = getProductionEfficiency();

            return productionEfficiency * (originalSnacksRatio * 21600);
        }

        protected double getProductionEfficiency()
        {
            return SnacksProperties.SnacksPerMeal * SnacksProperties.MealsPerDay * RecyclerCapacity * SnacksProperties.RecyclerEfficiency;
        }

        protected override void updateSettings()
        {
            productionEfficiency = getProductionEfficiency();

            SetEfficiencyBonus((float)productionEfficiency);

            dailyOutput = string.Format("{0:f2} Soil/day", productionEfficiency * originalSnacksRatio * 21600);
        }
    }
}
