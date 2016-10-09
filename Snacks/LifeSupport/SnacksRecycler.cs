using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.IO;

namespace Snacks
{
    [KSPModule("Recycler")]
    public class SnacksRecycler : ModuleResourceConverter
    {
        private const float kBaseSoilAmount = 400f;

        [KSPField()]
        public int RecyclerCapacity;

        double originalSnacksRatio;

        public override string GetInfo()
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine(base.GetInfo());
            builder.AppendFormat("Recycles {0:f2} Snacks per day", GetDailySnacksRecycled());

            return builder.ToString();
        }

        public override void OnStart(StartState state)
        {
            ResourceRatio[] outputs = outputList.ToArray();
            ResourceRatio output;

            GameEvents.OnGameSettingsApplied.Add(setupRecycler);

            //Get crew capacity of the recycler
            if (RecyclerCapacity == 0)
                RecyclerCapacity = this.part.CrewCapacity;
            if (RecyclerCapacity == 0)
                RecyclerCapacity = 1;

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
            
            //Now set up the recycler
            setupRecycler();

            base.OnStart(state);
       }

        public void Destroy()
        {
            GameEvents.OnGameSettingsApplied.Remove(setupRecycler);
        }

        public double GetDailySnacksRecycled()
        {
            if (HighLogic.LoadedScene != GameScenes.EDITOR &&
                HighLogic.LoadedScene != GameScenes.FLIGHT)
                return 0;

            return originalSnacksRatio * SnacksProperties.RecyclerEfficiency * 21600;
        }

        private void setupRecycler()
        {
            if (SnacksProperties.RecyclersEnabled)
            {
                //Add soil if needed
                if (this.part.Resources[SnacksProperties.SoilResourceName] == null)
                {
                    double maxAmount = kBaseSoilAmount * RecyclerCapacity;
                    
                    ConfigNode soilResource = new ConfigNode("RESOURCE");
                    soilResource.AddValue("name", SnacksProperties.SoilResourceName);
                    soilResource.AddValue("amount", "0");
                    soilResource.AddValue("maxAmount", maxAmount.ToString());
                    PartResource res = this.part.AddResource(soilResource);
                }

                //Enable the module
                EnableModule();
            }
            else
            {
                //Remove soil
                if (this.part.Resources[SnacksProperties.SoilResourceName] != null)
                    this.part.Resources.Remove(SnacksProperties.SoilResourceName);

                //Disable module
                DisableModule();
            }

            //Dirty the GUI
            MonoUtilities.RefreshContextWindows(this.part);
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
                    output.Ratio = originalSnacksRatio * 21600f * SnacksProperties.RecyclerEfficiency;
                    break;
                }
            }

            return base.PrepareRecipe(deltatime);
        }
    }
}
