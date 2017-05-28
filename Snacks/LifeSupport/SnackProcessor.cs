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

        [KSPField(isPersistant = true)]
        public double lastBackgroundUpdateTime;

        protected double originalSnacksRatio;
        protected double productionEfficiency = 0f;

        public virtual double GetDailySnacksOutput()
        {
            productionEfficiency = SnacksProperties.ProductionEfficiency;

            return originalSnacksRatio * productionEfficiency * 21600;
        }

        public override void OnStart(StartState state)
        {
            ResourceRatio[] outputs = outputList.ToArray();
            ResourceRatio output;

            base.OnStart(state);
            GameEvents.OnGameSettingsApplied.Add(updateSettings);

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

            //Now set up the processor
            updateSettings();
        }

        public virtual void Destroy()
        {
            GameEvents.OnGameSettingsApplied.Remove(updateSettings);
        }

        protected virtual void updateSettings()
        {
            productionEfficiency = SnacksProperties.ProductionEfficiency;

            SetEfficiencyBonus((float)productionEfficiency);

            dailyOutput = string.Format("{0:f2} Snacks/day", GetDailySnacksOutput());
        }
    }
}
