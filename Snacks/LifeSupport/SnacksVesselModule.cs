using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.IO;

namespace Snacks
{
    public class SnacksVesselModule : VesselModule
    {
        public override Activation GetActivation()
        {
            return Activation.LoadedVessels;
        }

        public override bool ShouldBeActive()
        {
            return vessel.loaded;
        }

        protected override void OnStart()
        {
            base.OnStart();

            //Add our vessel ID to the list of known vessels.
            if (SnacksScenario.Instance.knownVessels.Contains(vessel.id.ToString()) == false)
                SnacksScenario.Instance.knownVessels.Add(vessel.id.ToString());

            if (SnacksScenario.Instance.sciencePenalties.ContainsKey(vessel.id.ToString()))
            {
                int amount = SnacksScenario.Instance.sciencePenalties[vessel.id.ToString()];

                for (int index = 0; index < amount; index++)
                    ApplySciencePenalties();

                SnacksScenario.Instance.sciencePenalties.Remove(vessel.id.ToString());
            }
        }

        public void ApplySciencePenalties()
        {
            List<ModuleScienceContainer> scienceContainers = vessel.FindPartModulesImplementing<ModuleScienceContainer>();
            List<ModuleScienceLab> scienceLabs = vessel.FindPartModulesImplementing<ModuleScienceLab>();
            List<ModuleScienceExperiment> scienceExperiments = vessel.FindPartModulesImplementing<ModuleScienceExperiment>();

            //If we have a science lab aboard, see if it has any data. If so then lose some data.
            if (scienceLabs.Count > 0)
            {
                ModuleScienceLab[] labs = scienceLabs.ToArray();
                for (int index = 0; index < labs.Length; index++)
                {
                    if (labs[index].dataStored > 0.001)
                    {
                        float dataLoss = labs[index].dataStored * SnacksProperties.DataLostWhenHungry;
                        labs[index].dataStored -= labs[index].dataStored - dataLoss;
                        ScreenMessages.PostScreenMessage(string.Format("Kerbals fat-fingered ongoing research and lost {0:f3} data in the {1:s}", dataLoss, labs[index].part.partInfo.title),
                            5, ScreenMessageStyle.UPPER_LEFT);
                        return;
                    }
                }
            }

            //If we have containers aboard, see if they have any stored experiment results. If so, lose one.
            if (scienceContainers.Count > 0)
            {
                ModuleScienceContainer[] containers = scienceContainers.ToArray();
                ScienceData[] dataEntries;
                string title;
                for (int index = 0; index < containers.Length; index++)
                {
                    dataEntries = containers[index].GetData();
                    if (dataEntries != null && dataEntries.Length > 0)
                    {
                        title = dataEntries[0].title;
                        containers[index].DumpData(dataEntries[0]);
                        ScreenMessages.PostScreenMessage("Kerbals fat-fingered the controls and lost data from the " + title + " experiment in the " + scienceContainers[index].part.partInfo.title,
                            5, ScreenMessageStyle.UPPER_LEFT);
                        return;
                    }
                }
            }

            //If there is a science experiment aboard that has data, then dump one of the results.
            if (scienceExperiments.Count > 0)
            {
                ModuleScienceExperiment[] experiments = scienceExperiments.ToArray();
                ScienceData[] dataEntries;
                for (int index = 0; index < experiments.Length; index++)
                {
                    if (experiments[index].Deployed)
                    {
                        dataEntries = experiments[index].GetData();
                        for (int dataIndex = 0; dataIndex < dataEntries.Length; dataIndex++)
                            experiments[index].DumpData(dataEntries[dataIndex]);
                        ScreenMessages.PostScreenMessage("Kerbals fat-fingered the controls and lost all data from the " + experiments[index].part.partInfo.title,
                            5, ScreenMessageStyle.UPPER_LEFT);
                    }
                }
            }
        }

    }
}
