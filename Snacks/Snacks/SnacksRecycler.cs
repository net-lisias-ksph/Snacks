using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.IO;

namespace Snacks
{
    public class SnacksRecycler : ModuleResourceConverter
    {
        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            GameEvents.OnGameSettingsApplied.Add(setupRecycler);

            setupRecycler();
       }

        public void Destroy()
        {
            GameEvents.OnGameSettingsApplied.Remove(setupRecycler);
        }

        private void setupRecycler()
        {
            if (SnacksProperties.RecyclersEnabled)
            {
                if (this.part.Resources[SnacksProperties.SoilResourceName] == null)
                {
                    double maxAmount = 400 * this.part.CrewCapacity;
                    if (maxAmount == 0)
                        maxAmount = 400;
                    ConfigNode soilResource = new ConfigNode("RESOURCE");
                    soilResource.AddValue("name", SnacksProperties.SoilResourceName);
                    soilResource.AddValue("amount", "0");
                    soilResource.AddValue("maxAmount", maxAmount.ToString());
                    PartResource res = this.part.AddResource(soilResource);
                }
                EnableModule();
            }
            else
            {
                if (this.part.Resources[SnacksProperties.SoilResourceName] != null)
                    this.part.Resources.Remove(SnacksProperties.SoilResourceName);
                DisableModule();
            }
            MonoUtilities.RefreshContextWindows(this.part);
        }
    }
}
