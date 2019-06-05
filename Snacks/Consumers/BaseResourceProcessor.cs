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
using UnityEngine;

namespace Snacks
{
    public enum SnacksResultType
    {
        resultConsumption,
        resultProduction
    }

    /// <summary>
    /// This is a result that has data regarding what happened during resource consumption or production.
    /// </summary>
    public struct SnacksConsumerResult
    {
        /// <summary>
        /// Name of the resource that was processed.
        /// </summary>
        public string resourceName;

        /// <summary>
        /// Type of result
        /// </summary>
        public SnacksResultType resultType;

        /// <summary>
        /// Flag to indicate whether or not the process completed successfully.
        /// </summary>
        public bool completedSuccessfully;

        /// <summary>
        /// Flag indicating if the process was applied per crew member.
        /// </summary>
        public bool appliedPerCrew;

        /// <summary>
        /// Number of kerbals affected by the process.
        /// </summary>
        public int affectedKerbalCount;

        /// <summary>
        /// Current amount of the resource in the vessel/kerbal.
        /// </summary>
        public double currentAmount;

        /// <summary>
        /// Max amount of the resource in the vessel/kerbal.
        /// </summary>
        public double maxAmount;
    }

    /// <summary>
    /// This is the base class for a resource processor. Similar to ModuleResourceConverter, the consumer will consume resources and produce resources, but it happens at the vessel level, not the part level.
    /// It's also designed to work with both loaded and unloaded vessels. Another important difference is that consumed/produced resources can occur on a per crewmember basis; a vessel with 5 crew will
    /// consume and/or produce 5 times the resources as a vessel with 1 crewmember. The configuration of a BaseResourceProcessor is done through config files.
    /// </summary>
    public class BaseResourceProcessor
    {
        #region Constants
        public const string ConfigNodeName = "SNACKS_RESOURCE_PROCESSOR";
        #endregion

        #region Fields
        /// <summary>
        /// User friendly name of the resource processor
        /// </summary>
        public string processName = string.Empty;

        /// <summary>
        /// Number of seconds that must pass before running the consumer.
        /// </summary>
        public double secondsPerCycle = 3600.0f;
        #endregion

        #region Housekeeping
        public List<ProcessedResource> inputList;
        public List<ProcessedResource> outputList;
        public Dictionary<string, SnacksConsumerResult> consumptionResults;
        public Dictionary<string, SnacksConsumerResult> productionResults;

        double remainingTime = 0;

        public BaseResourceProcessor()
        {

        }

        public BaseResourceProcessor(ConfigNode node)
        {
            if (node.HasValue("processName"))
                processName = node.GetValue("processName");

            if (node.HasValue("secondsPerCycle"))
                double.TryParse(node.GetValue("secondsPerCycle"), out secondsPerCycle);

            //Create lists
            inputList = new List<ProcessedResource>();
            outputList = new List<ProcessedResource>();
            consumptionResults = new Dictionary<string, SnacksConsumerResult>();
            productionResults = new Dictionary<string, SnacksConsumerResult>();
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// Handles the situation where the kerbal went on EVA.
        /// </summary>
        /// <param name="astronaut">The kerbal that went on EVA.</param>
        /// <param name="part">The part that the kerbal left.</param>
        public virtual void onKerbalEVA(ProtoCrewMember astronaut, Part part)
        {

        }

        /// <summary>
        /// Handles the situation where a kerbal boards a vessel.
        /// </summary>
        /// <param name="astronaut">The kerbal boarding a vessel.</param>
        /// <param name="part">The part boarded.</param>
        public virtual void onKerbalBoardedVessel(ProtoCrewMember astronaut, Part part)
        {

        }

        /// <summary>
        /// Handles the situation where a kerbal levels up.
        /// </summary>
        /// <param name="astronaut">The kerbal that has leveled up.</param>
        public virtual  void onKerbalLevelUp(ProtoCrewMember astronaut)
        {

        }

        /// <summary>
        /// Handles adding of a new kerbal, giving the consumer a chance to add custom roster data.
        /// </summary>
        /// <param name="astronaut">The kerbal being added.</param>
        public virtual void onKerbalAdded(ProtoCrewMember astronaut)
        {

        }

        /// <summary>
        /// Handles removal of a kerbal, giving the consumer a chance to update custom data if needed.
        /// </summary>
        /// <param name="astronaut">The kerbal being removed.</param>
        /// <param name="astronautData">Data regarding the astronaut.</param>
        public virtual void onKerbalRemoved(ProtoCrewMember astronaut)
        {

        }

        /// <summary>
        /// Handles a kerbal's name change.
        /// </summary>
        /// <param name="astronaut">The kerbal whose name has changed. Note that roster data is already being carried over, this event is used to give consumers a chance to update custom data kept outside of the roster.</param>
        /// <param name="previousName">The kerbal's previous name.</param>
        /// <param name="newName">The kerbal's new name.</param>
        public virtual void onKerbalNameChanged(ProtoCrewMember astronaut, string previousName, string newName)
        {

        }

        /// <summary>
        /// Handles vessel loaded event, for instance, adding resources that should be on the vessel.
        /// </summary>
        /// <param name="vessel">The vessel that was loaded.</param>
        public virtual void onVesselLoaded(Vessel vessel)
        {

        }

        /// <summary>
        /// Handles changes to game settings.
        /// </summary>
        public virtual void OnGameSettingsApplied()
        {

        }
        #endregion

        #region API
        /// <summary>
        /// Loads the SNACKS_RESOURCE_CONSUMER config nodes and returns a list of consumers.
        /// </summary>
        /// <returns>A list of resource consumers.</returns>
        public static List<BaseResourceProcessor> LoadConsumers()
        {
            List<BaseResourceProcessor> resourceProcessors = new List<BaseResourceProcessor>();
            BaseResourceProcessor resourceConsumer;
            ConfigNode[] consumerNodes = GameDatabase.Instance.GetConfigNodes(ConfigNodeName);
            ConfigNode node;

            //Add the snacks consumer
            SnacksResourceProcessor snacksConsumer = new SnacksResourceProcessor();
            snacksConsumer.Initialize();
            resourceProcessors.Add(snacksConsumer);

            //Now go through all the config nodes and load them
            for (int index = 0; index < consumerNodes.Length; index++)
            {
                node = consumerNodes[index];
                resourceConsumer = new BaseResourceProcessor(node);
                resourceConsumer.Initialize();
                resourceProcessors.Add(resourceConsumer);
            }

            //Return all the consumers
            return resourceProcessors;
        }

        /// <summary>
        /// Initializes the consumer
        /// </summary>
        public virtual void Initialize()
        {

        }

        /// <summary>
        /// De-serializes persistence data
        /// </summary>
        /// <param name="node">The ConfigNode with the persistence data</param>
        public virtual void OnLoad(ConfigNode node)
        {
            if (node.HasValue("remainingTime"))
                double.TryParse(node.GetValue("remainingTime"), out remainingTime);
        }

        /// <summary>
        /// Saves persistence data to a ConfigNode and returns it.
        /// </summary>
        /// <returns>A ConfigNode containing persistence data, if any.</returns>
        public virtual ConfigNode OnSave()
        {
            ConfigNode node = new ConfigNode(ConfigNodeName);

            node.AddValue("remainingTime", remainingTime);

            return node;
        }

        /// <summary>
        /// Used primarily for simulations, returns the consumed and produced resources for the given unit of time.
        /// </summary>
        /// <param name="vessel">The vessel to query for data.</param>
        /// <param name="secondsPerCycle">The number of seconds to calculate total inputs and outputs.</param>
        /// <param name="consumedResources">The list of consumed resources to add the inputs to.</param>
        /// <param name="producedResources">The list of produced resources to add the outputs to.</param>
        public virtual void AddConsumedAndProducedResources(Vessel vessel, double secondsPerCycle, List<ResourceRatio> consumedResources, List<ResourceRatio>  producedResources)
        {

        }

        /// <summary>
        /// Used primarily for simulations, returns the consumed and produced resources for the given unit of time.
        /// </summary>
        /// <param name="crewCount">The number of crew to simulate.</param>
        /// <param name="secondsPerCycle">The number of seconds to calculate total inputs and outputs.</param>
        /// <param name="consumedResources">The list of consumed resources to add the inputs to.</param>
        /// <param name="producedResources">The list of produced resources to add the outputs to.</param>
        public virtual void AddConsumedAndProducedResources(int crewCount, double secondsPerCycle, List<ResourceRatio> consumedResources, List<ResourceRatio> producedResources)
        {

        }

        /// <summary>
        /// Returns the amount and max amount of the desired resource in the unloaded vessel.
        /// </summary>
        /// <param name="protoVessel">The vessel to query for the resource totals.</param>
        /// <param name="resourceName">The name of the resource to query.</param>
        /// <param name="amount">The amount of the resource that the entire vessel has.</param>
        /// <param name="maxAmount">The max amount of the resource that the entire vessel has.</param>
        public virtual void GetUnloadedResourceTotals(ProtoVessel protoVessel, string resourceName, out double amount, out double maxAmount)
        {
            amount = 0;
            maxAmount = 0;

            ProtoPartSnapshot[] protoPartSnapshots = protoVessel.protoPartSnapshots.ToArray();
            ProtoPartSnapshot partSnapshot;
            ProtoPartResourceSnapshot[] protoResources;
            ProtoPartResourceSnapshot resource;
            for (int partIndex = 0; partIndex < protoPartSnapshots.Length; partIndex++)
            {
                partSnapshot = protoPartSnapshots[partIndex];

                protoResources = partSnapshot.resources.ToArray();
                for (int resourceIndex = 0; resourceIndex < protoResources.Length; resourceIndex++)
                {
                    resource = protoResources[resourceIndex];
                    if (resource.resourceName == resourceName && resource.flowState)
                    {
                        amount += resource.amount;
                        maxAmount += resource.maxAmount;
                    }
                }
            }
        }
        #endregion

        #region Consumer Cycle
        /// <summary>
        /// Runs the processor, consuming input resources, producing output resources, and collating results.
        /// </summary>
        /// <param name="vessel">The vessel to run the consumer on.</param>
        /// <param name="elapsedTime">Number of seconds that have passed.</param>
        /// <param name="crewCount">Number of crew aboard the vessel.</param>
        /// <param name="crewCapacity">The vessel's total crew capacity.</param>
        public virtual void ProcessResources(Vessel vessel, double elapsedTime, int crewCount, int crewCapacity)
        {
            elapsedTime = 3600;
            int count = 0;
            ProcessedResource resource;
            SnacksConsumerResult result;

            remainingTime += elapsedTime;
            while (remainingTime >= secondsPerCycle)
            {
                remainingTime -= secondsPerCycle;

                //Consume inputs
                consumptionResults.Clear();
                count = inputList.Count;
                for (int index = 0; index < count; index++)
                {
                    resource = inputList[index];
                    result = resource.ConsumeResource(vessel, elapsedTime, crewCount, crewCapacity);
                    consumptionResults.Add(result.resourceName, result);
                }

                //Produce outputs
                productionResults.Clear();
                count = outputList.Count;
                for (int index = 0; index < count; index++)
                {
                    resource = outputList[index];
                    result = resource.ProduceResource(vessel, elapsedTime, crewCount, crewCapacity, consumptionResults);
                    productionResults.Add(result.resourceName, result);
                }
            }
        }
        #endregion

        #region Helpers
        #endregion
    }
}
