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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Snacks
{
    /// <summary>
    /// This struct tracks vessel resources in the simulator. For the sake of simplicity, simulated resources aren't locked and are considered flow mode ALL_VESSEL.
    /// </summary>
    public struct SimResource
    {
        public string resourceName;
        public double amount;
        public double maxAmount;

        //Estimated number of seconds that the resource will last.
        public double durationSeconds;
    }

    /// <summary>
    /// This class is a simulated ModuleResourceConverter. It processes inputs, produces outputs, and when the time's up, generates yield resources. For the sake of simplicity, all vessel resources are available.
    /// </summary>
    public class SimConverter
    {
        #region Housekeeping
        public List<ResourceRatio> inputList;
        public List<ResourceRatio> outputList;
        public List<ResourceRatio> yieldsList;
        public double yieldSecondsPerCycle;
        public double elapsedTime;
        public string converterName;
        public bool isActivated = true;
        #endregion

        #region Constructors
        public SimConverter()
        {
            inputList = new List<ResourceRatio>();
            outputList = new List<ResourceRatio>();
            yieldsList = new List<ResourceRatio>();
        }
        #endregion

        #region Converter Operations
        /// <summary>
        /// Processes resources, consuming inputs, producing outputs, and when time expires, producing yield resources.
        /// For the purposes of simulation, we assume dumpExcess = true, yield resources always suceed, no heat generation, and no crew bonuses.
        /// </summary>
        /// <param name="resources">The map of vessel resources to process.</param>
        /// <param name="secondsPerSimulatorCycle">The number of seconds per simulator cycle.</param>
        public void ProcessResources(Dictionary<string, SimResource> resources, double secondsPerSimulatorCycle)
        {
            int count = inputList.Count;
            string resourceName;
            SimResource simResource;

            //Make sure we have enough inputs
            for (int index = 0; index < count; index++)
            {
                resourceName = inputList[index].ResourceName;

                if (!resources.ContainsKey(resourceName))
                {
                    isActivated = false;
                    return;
                }
                if (resources[resourceName].amount < inputList[index].Ratio)
                {
                    return;
                }
            }

            //Consume input resources
            for (int index = 0; index < count; index++)
            {
                resourceName = inputList[index].ResourceName;

                simResource = resources[resourceName];
                simResource.amount -= inputList[index].Ratio;
                if (simResource.amount <= 0)
                    simResource.amount = 0;
                resources[resourceName] = simResource;
            }

            //Produce output resources
            count = outputList.Count;
            for (int index = 0; index < count; index++)
            {
                //If the vessel doesn't have the output resource then skip this output.
                resourceName = outputList[index].ResourceName;
                if (!resources.ContainsKey(resourceName))
                    continue;

                simResource = resources[resourceName];
                simResource.amount += outputList[index].Ratio;
                if (simResource.amount > simResource.maxAmount)
                    simResource.amount = simResource.maxAmount;
                resources[resourceName] = simResource;
            }

            //Check for yield resources
            count = yieldsList.Count;
            if (count <= 0)
                return;
            elapsedTime += secondsPerSimulatorCycle;
            if (elapsedTime >= yieldSecondsPerCycle)
            {
                //Reset elapsed time
                elapsedTime = 0;

                //Now produce the yield
                for (int index = 0; index < count; index++)
                {
                    //If the vessel doesn't have the yield resource then skip this output.
                    resourceName = yieldsList[index].ResourceName;
                    if (!resources.ContainsKey(resourceName))
                        continue;

                    simResource = resources[resourceName];
                    simResource.amount += yieldsList[index].Ratio;
                    if (simResource.amount > simResource.maxAmount)
                        simResource.amount = simResource.maxAmount;
                    resources[resourceName] = simResource;
                }
            }
        }
        #endregion
    }
}
