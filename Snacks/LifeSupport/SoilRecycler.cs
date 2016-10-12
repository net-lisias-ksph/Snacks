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
        private const float kBaseSoilAmount = 400f;

        [KSPField()]
        public int RecyclerCapacity;


        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            Fields["dailyOutput"].guiName = "Max Recycling";
        }

        protected override void setupProcessor()
        {
            efficiency = SnacksProperties.RecyclerEfficiency;
            dailyOutput = string.Format("{0:f2} Soil/day", GetDailySnacksOutput());
        }

    }
}
