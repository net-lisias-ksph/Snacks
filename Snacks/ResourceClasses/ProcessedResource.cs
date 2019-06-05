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
    /// This class represents resources consumed or produced by a SnacksResourceProcessor. Consumption and production is applied vessel-wide, or to individual kerbal roster entries depending on the configuration.
    /// If applied vessel-wide, the resource can be produced or consumed per kerbal. Finally, the resource can be displayed in the Snapshots view.
    /// </summary>
    public class ProcessedResource
    {
        #region Constants
        public const string NodeName = "ProcessedResource";
        #endregion

        #region Fields
        /// <summary>
        /// Name of the consumed/produced resource
        /// </summary>
        public string resourceName = string.Empty;

        /// <summary>
        /// Name of the dependency resource if the resource to process depends upon the results of another resource's process result. E.G. 1 unit of Soil is produced for 1 unt of Snacks consumed.
        /// </summary>
        public string dependencyResourceName = string.Empty;

        /// <summary>
        /// Flag to indicate whether or not the resource is applied to roster entries instead of parts and vessels. if set to true, then appliedPerCrew is ignored.
        /// Default: false
        /// </summary>
        public bool isRosterResource = false;

        /// <summary>
        /// Flag to indicate whether or not to show the resource in the Snapshots window.
        /// Default: true
        /// </summary>
        public bool showInSnapshot = true;

        /// <summary>
        /// By default penalties aren’t applied until a resource is depeleted or completely full. But if the amount / maxAmount reaches the threshold, then the penalties will be applied.
        /// Default: 0
        /// </summary>
        public float penaltyThreshold = 0f;

        /// <summary>
        /// The amount of resource to consume or produce. If appliedPerCrew is true, then the amount consumed/produced is multiplied by the number of crew aboard the vessel.
        /// If isRosterResource is true, then each individual crew member's roster entry will be affected instead.
        /// Default: 0
        /// </summary>
        public double amount = 0;
        #endregion

        #region Housekeeping
        public ProcessedResource()
        {

        }

        public ProcessedResource(string resourceName, string dependencyResourceName, double amount, bool isRosterResource = false, bool showInSnapshot = true, float penaltyThreshold = 0f)
        {
            this.resourceName = resourceName;
            this.dependencyResourceName = dependencyResourceName;
            this.amount = amount;
            this.isRosterResource = isRosterResource;
            this.showInSnapshot = showInSnapshot;
            this.penaltyThreshold = penaltyThreshold;
        }

        /// <summary>
        /// Loads the fields from the config node.
        /// </summary>
        /// <param name="node">A ConfigNode containing fields to load.</param>
        public void Load(ConfigNode node)
        {
            if (node.HasValue("resourceName"))
                resourceName = node.GetValue("resourceName");
            if (node.HasValue("dependencyResourceName"))
                dependencyResourceName = node.GetValue("dependencyResourceName");
            if (node.HasValue("amount"))
                double.TryParse(node.GetValue("amount"), out amount);
            if (node.HasValue("isRosterResource"))
                bool.TryParse(node.GetValue("isRosterResource"), out isRosterResource);
            if (node.HasValue("showInSnapshot"))
                bool.TryParse(node.GetValue("showInSnapshot"), out showInSnapshot);
            if (node.HasValue("penaltyThreshold"))
                float.TryParse(node.GetValue("penaltyThreshold"), out penaltyThreshold);
        }

        /// <summary>
        /// Saves current values to a ConfigNode.
        /// </summary>
        /// <returns>A ConfigNode containing the field data.</returns>
        public ConfigNode Save()
        {
            ConfigNode node = new ConfigNode(NodeName);

            node.AddValue("resourceName", resourceName);
            node.AddValue("amount", amount);
            node.AddValue("isRosterResource", isRosterResource);
            node.AddValue("showInSnapshot", showInSnapshot);
            node.AddValue("penaltyThreshold", penaltyThreshold);

            return node;
        }
        #endregion

        /// <summary>
        /// Consumes the resource.
        /// </summary>
        /// <param name="vessel">The vessel to work on</param>
        /// <param name="elapsedTime">Elapsed seconds</param>
        /// <param name="crewCount">Current crew count</param>
        /// <param name="crewCapacity">Current crew capacity</param>
        /// <returns>A SnacksConsumerResult containing the resuls of the consumption.</returns>
        public SnacksConsumerResult ConsumeResource(Vessel vessel, double elapsedTime, int crewCount, int crewCapacity)
        {
            List<ProtoPartResourceSnapshot> protoPartResources = new List<ProtoPartResourceSnapshot>();

            SnacksConsumerResult result = new SnacksConsumerResult();
            result.resourceName = resourceName;
            result.resultType = SnacksResultType.resultConsumption;

            if (!isRosterResource)
            {
                double vesselCurrentAmount = 0;
                double vesselMaxAmount = 0;
                double requestAmount = amount;

                //Get current totals
                getResourceTotals(vessel, out vesselCurrentAmount, out vesselMaxAmount, protoPartResources);

                //Multiply request amount by crew count
                requestAmount *= crewCount;

                //If we have enough to support the whole crew, then we're good.
                if (vesselCurrentAmount >= requestAmount)
                {
                    //Request resource
                    requestResource(vessel, requestAmount, protoPartResources);

                    //Update results
                    result.affectedKerbalCount = crewCount;
                    result.completedSuccessfully = true;
                    result.currentAmount = vesselCurrentAmount - requestAmount;
                    result.maxAmount = vesselMaxAmount;
                }

                //We don't have enough to support the whole crew. Figure out how many we can support.
                else
                {
                    result.affectedKerbalCount = (int)Math.Floor(vesselCurrentAmount / amount);

                    requestAmount = result.affectedKerbalCount * amount;
                    requestResource(vessel, requestAmount, protoPartResources);

                    result.completedSuccessfully = false;
                    result.currentAmount = vesselCurrentAmount - requestAmount;
                    result.maxAmount = vesselMaxAmount;
                }
            }

            //Process the roster resource
            else
            {

            }

            return result;
        }

        /// <summary>
        /// Produces the resource
        /// </summary>
        /// <param name="vessel">The vessel to work on</param>
        /// <param name="elapsedTime">Elapsed seconds</param>
        /// <param name="crewCount">Current crew count</param>
        /// <param name="crewCapacity">Current crew capacity</param>
        /// <param name="consumptionResults">Results of resource consumption.</param>
        /// <returns>A SnacksConsumerResult containing the resuls of the production.</returns>
        public SnacksConsumerResult ProduceResource(Vessel vessel, double elapsedTime, int crewCount, int crewCapacity, Dictionary<string, SnacksConsumerResult> consumptionResults)
        {
            SnacksConsumerResult result = new SnacksConsumerResult();
            SnacksConsumerResult dependencyConsumptionResult;

            //If our output depends upon the results of a dependency resource, then retrieve the results.
            if (!string.IsNullOrEmpty(dependencyResourceName) && consumptionResults.ContainsKey(dependencyResourceName))
            {
                dependencyConsumptionResult = consumptionResults[dependencyResourceName];
            }
            return result;
        }

        #region Helpers
        protected virtual double requestResource(Vessel vessel, double demand, List<ProtoPartResourceSnapshot> resourceList)
        {
            double amountObtained = 0;
            PartResourceDefinitionList definitions = PartResourceLibrary.Instance.resourceDefinitions;
            int resourceID = definitions[resourceName].id;
            double demandRemaining = demand;
            int count = resourceList.Count;
            ProtoPartResourceSnapshot resource;

            if (vessel.loaded)
            {
                amountObtained = vessel.rootPart.RequestResource(resourceID, demand);
            }
            else
            {
                for (int index = 0; index < count; index++)
                {
                    resource = resourceList[index];

                    if (resource.amount >= demandRemaining)
                    {
                        resource.amount -= demandRemaining;
                        amountObtained = demandRemaining;
                        break;
                    }
                    else
                    {
                        demandRemaining -= resource.amount;
                        amountObtained += resource.amount;
                        resource.amount = 0;
                    }
                }
            }

            return amountObtained;
        }

        protected virtual void getResourceTotals(Vessel vessel, out double amount, out double maxAmount, List<ProtoPartResourceSnapshot> resourceList)
        {
            amount = 0;
            maxAmount = 0;
            PartResourceDefinitionList definitions = PartResourceLibrary.Instance.resourceDefinitions;
            int resourceID = definitions[resourceName].id;

            if (vessel.loaded)
            {
                vessel.rootPart.GetConnectedResourceTotals(resourceID, out amount, out maxAmount);
            }
            else
            {
                ProtoPartSnapshot[] protoPartSnapshots = vessel.protoVessel.protoPartSnapshots.ToArray();
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
                            resourceList.Add(resource);
                        }
                    }
                }
            }
        }
        #endregion
    }
}
