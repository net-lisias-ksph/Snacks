using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.IO;

namespace Snacks
{
    public class SnackProcessor : ModuleResourceConverter
    {
        [KSPField(guiActive = true, guiActiveEditor = true, guiName = "Max Production")]
        public string dailyOutput = string.Empty;

        protected string productionInfo = "Produces up to {0:f2} Snacks per day";
        protected double originalSnacksRatio;
        protected double efficiency = 1.0f;

        public virtual double GetDailySnacksOutput()
        {
            return originalSnacksRatio * efficiency * 21600;
        }

        public override void OnStart(StartState state)
        {
            ResourceRatio[] outputs = outputList.ToArray();
            ResourceRatio output;

            base.OnStart(state);
            GameEvents.OnGameSettingsApplied.Add(setupProcessor);

            //Find the Snacks output ratio
            for (int index = 0; index < outputs.Length; index++)
            {
                output = outputs[index];
                if (output.ResourceName == SnacksProperties.SnacksResourceName)
                {
                    originalSnacksRatio = output.Ratio;
                    break;
                }
            }

            setupProcessor();
        }

        public virtual void Destroy()
        {
            GameEvents.OnGameSettingsApplied.Remove(setupProcessor);
        }

        protected virtual void setupProcessor()
        {
            efficiency = SnacksProperties.ProductionEfficiency;
            dailyOutput = string.Format("{0:f2} Snacks/day", GetDailySnacksOutput());
        }

        protected override ConversionRecipe PrepareRecipe(double deltatime)
        {
            //Get the snacks per second
            ResourceRatio[] outputs = outputList.ToArray();
            ResourceRatio output;

            //Find the snacks output and apply efficiency
            for (int index = 0; index < outputs.Length; index++)
            {
                output = outputs[index];
                if (output.ResourceName == SnacksProperties.SnacksResourceName)
                {
                    output.Ratio = originalSnacksRatio * 21600f * efficiency;
                    break;
                }
            }

            return base.PrepareRecipe(deltatime);
        }
    }
}
