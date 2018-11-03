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
            productionEfficiency = SnacksProperties.ProductionEfficiency;

            return originalSnacksRatio * productionEfficiency * 21600;
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
            if (SnackController.debugMode)
                Debug.Log("[" + this.ClassName + "] - resourceRatios: " + resourceRatios);
        }

        protected virtual void updateSettings()
        {
            productionEfficiency = SnacksProperties.ProductionEfficiency;

            SetEfficiencyBonus((float)productionEfficiency);

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
    }
}
