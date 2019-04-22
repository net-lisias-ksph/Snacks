using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.IO;

/**
The MIT License (MIT)
Copyright (c) 2014-2019 by Michael Billard
Original concept by Troy Gruetzmacher

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
 * */
namespace Snacks
{
    public class SnackProcessor : ModuleResourceConverter
    {
        //Default resource ratios used for background processing.
        public const string DefaultProcessorRatios = "Ore,0.002|Snacks,0.01";

        [KSPField(guiActive = true, guiActiveEditor = true, guiName = "Max Production")]
        public string dailyOutput = string.Empty;

        [KSPField]
        public int minimumVesselPercentEC = 5;

        [KSPField(isPersistant = true)]
        public double lastBackgroundUpdateTime;

        //This string is used for background processing.
        [KSPField(isPersistant = true)]
        public string resourceRatios = string.Empty;

        [KSPField(isPersistant = true)]
        protected double originalSnacksRatio;

        protected double productionEfficiency = 0f;
        protected bool hasElectricChargeInput = false;
        PartResourceDefinition resourceDef = null;
        double minECLevel = 0.0f;

        public virtual double GetDailySnacksOutput()
        {
            updateProductionEfficiency();

            return originalSnacksRatio * productionEfficiency * SnacksScenario.GetHoursPerDay();
        }

        public override void OnStart(StartState state)
        {
            base.OnStart(state);
            GameEvents.OnGameSettingsApplied.Add(updateSettings);

            //Build the resource ratios string. It's used for background processing
            updateResourceRatios();

            //Now set up the processor
            updateSettings();
        }

        public virtual void Destroy()
        {
            GameEvents.OnGameSettingsApplied.Remove(updateSettings);
        }

        protected void updateResourceRatios()
        {
            ResourceRatio ratio;
            StringBuilder resourceBuilder = new StringBuilder();
            string inputRatios, outputRatios;

            //Inputs
            for (int index = 0; index < inputList.Count; index++)
            {
                ratio = inputList[index];
                if (ratio.ResourceName == "ElectricCharge")
                {
                    hasElectricChargeInput = true;
                    PartResourceDefinitionList definitions = PartResourceLibrary.Instance.resourceDefinitions;
                    resourceDef = definitions["ElectricCharge"];
                    minECLevel = (double)minimumVesselPercentEC / 100.0f;
                    continue;
                }

                resourceBuilder.Append(ratio.ResourceName);
                resourceBuilder.Append(",");
                resourceBuilder.Append(ratio.Ratio.ToString());
                resourceBuilder.Append(";");
            }

            //Trim the trailing ";" character.
            inputRatios = resourceBuilder.ToString();
            inputRatios = inputRatios.TrimEnd(new char[] { ';' });

            //Outputs
            resourceBuilder = new StringBuilder();
            for (int index = 0; index < outputList.Count; index++)
            {
                ratio = outputList[index];
                resourceBuilder.Append(ratio.ResourceName);
                resourceBuilder.Append(",");
                resourceBuilder.Append(ratio.Ratio.ToString());
                resourceBuilder.Append(";");

                if (ratio.ResourceName == SnacksProperties.SnacksResourceName)
                    originalSnacksRatio = ratio.Ratio;
            }

            //Trim the trailing ";" character.
            outputRatios = resourceBuilder.ToString();
            outputRatios = outputRatios.TrimEnd(new char[] { ';' });

            resourceRatios = inputRatios + "|" + outputRatios;
            if (SnacksProperties.DebugLoggingEnabled)
                Debug.Log("[" + this.ClassName + "] - resourceRatios: " + resourceRatios);
        }

        protected virtual void updateProductionEfficiency()
        {
            productionEfficiency = SnacksProperties.ProductionEfficiency / 100.0f;
        }

        protected virtual void updateSettings()
        {
            updateProductionEfficiency();

            //SetEfficiencyBonus((float)productionEfficiency);

            dailyOutput = string.Format("{0:f2} Snacks/day", GetDailySnacksOutput());
        }

        protected override void CheckForShutdown()
        {
            base.CheckForShutdown();

            if (hasElectricChargeInput)
            {
                double amount = 0;
                double maxAmount = 0;
                this.part.GetConnectedResourceTotals(resourceDef.id, out amount, out maxAmount, true);
                if ((amount / maxAmount) < minECLevel)
                    StopResourceConverter();
            }
        }

        protected override ConversionRecipe PrepareRecipe(double deltatime)
        {
            ConversionRecipe recipe = base.PrepareRecipe(deltatime);
            try
            {
                int count = outputList.Count;
                ResourceRatio ratio;

                recipe.Outputs.Clear();
                for (int index = 0; index < count; index++)
                {
                    ratio = new ResourceRatio(outputList[index].ResourceName, outputList[index].Ratio * productionEfficiency, outputList[index].DumpExcess);
                    ratio.FlowMode = outputList[index].FlowMode;
                    recipe.Outputs.Add(ratio);
                }
            }
            catch (Exception ex)
            {
                Debug.Log("[SnackProcessor] -  error when preparing recipe: " + ex);
            }
            return recipe;
        }
    }
}
