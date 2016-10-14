/**
The MIT License (MIT)
Copyright (c) 2014 Troy Gruetzmacher, Michael Billard

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
 * 
 * 
 * */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.IO;

namespace Snacks
{
    public class SciencePenalty : ISnacksPenalty
    {
        public bool IsEnabled()
        {
            return SnacksProperties.LoseScienceWhenHungry;
        }

        public bool AlwaysApply()
        {
            return !SnacksProperties.RandomPenaltiesEnabled;
        }

        public void RemovePenalty(Vessel vessel)
        {
        }

        public void ApplyPenalty(int hungryKerbals, Vessel vessel)
        {
            if ((HighLogic.CurrentGame.Mode == Game.Modes.CAREER || HighLogic.CurrentGame.Mode == Game.Modes.SCIENCE_SANDBOX) && SnacksProperties.LoseScienceWhenHungry)
            {
                //If the vessel is loaded, apply the penalties.
                if (vessel.loaded)
                {
                    applySciencePenalties(vessel);
                }

                //Not loaded, keep track of how many penalties we acquire
                else
                {
                    ScreenMessages.PostScreenMessage("Kerbals have ruined some science aboard the " + vessel.vesselName + "! Check the vessel for details.", 5f, ScreenMessageStyle.UPPER_LEFT);
                    if (SnacksScenario.Instance.sciencePenalties.Contains(vessel.id.ToString()) == false)
                        SnacksScenario.Instance.sciencePenalties.Add(vessel.id.ToString(), 1);
                    else
                        SnacksScenario.Instance.sciencePenalties[vessel.id.ToString()] += 1;
                }
            }
        }

        public static void CheckSciencePenalties(Vessel vessel)
        {
            //Apply science loss
            if ((HighLogic.CurrentGame.Mode == Game.Modes.CAREER || HighLogic.CurrentGame.Mode == Game.Modes.SCIENCE_SANDBOX) && SnacksProperties.LoseScienceWhenHungry)
            {
                if (vessel.loaded)
                {
                    //Apply all the penalties we acquired
                    if (SnacksScenario.Instance.sciencePenalties.Contains(vessel.id.ToString()))
                    {
                        int amount = SnacksScenario.Instance.sciencePenalties[vessel.id.ToString()];

                        for (int index = 0; index < amount; index++)
                            applySciencePenalties(vessel);

                        SnacksScenario.Instance.sciencePenalties.Remove(vessel.id.ToString());
                    }
                }
            }
        }

        protected static void applySciencePenalties(Vessel vessel)
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
