/**
The MIT License (MIT)
Copyright (c) 2014-2019 by Michael Billard
 

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

namespace Snacks
{
    /// <summary>
    /// This outcome consumes the specified resource in the desired amount. It can be a vessel resource or a roster resource.
    /// Example definition:
    /// OUTCOME 
    /// {
    ///     name  = ConsumeResource
    ///     resourceName = Stress
    ///     amount = 1
    /// }
    /// </summary>
    public class ConsumeResource : BaseOutcome
    {
        #region Housekeeping
        /// <summary>
        /// Name of the resource to produce
        /// </summary>
        public string resourceName = string.Empty;

        /// <summary>
        /// Optional minimum value of random amount to produce.
        /// </summary>
        public float randomMin = 0;

        /// <summary>
        /// Optional maximum value of random amount to produce.
        /// </summary>
        public float randomMax = 0;

        /// <summary>
        /// Amount of resource to consume. Takes presedence over randomMin and randomMax
        /// </summary>
        public double amount = 0;

        protected bool isRosterResource = false;
        #endregion

        #region Overrides
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Snacks.ConsumeResource"/> class.
        /// </summary>
        /// <param name="node">A ConfigNode containing initialization parameters. Parameters in the
        /// <see cref="T:Snacks.BaseOutcome"/> class also apply.</param>
        public ConsumeResource(ConfigNode node) : base(node)
        {
            if (node.HasValue(ResourceName))
                resourceName = node.GetValue(ResourceName);

            PartResourceDefinitionList definitions = PartResourceLibrary.Instance.resourceDefinitions;
            if (definitions.Contains(resourceName))
                isRosterResource = false;
            else
                isRosterResource = true;

            if (node.HasValue(ResourceAmount))
                double.TryParse(node.GetValue(ResourceAmount), out amount);

            if (node.HasValue(ResourceRandomMin))
                float.TryParse(node.GetValue(ResourceRandomMin), out randomMax);

            if (node.HasValue(ResourceRandomMax))
                float.TryParse(node.GetValue(ResourceRandomMax), out randomMin);
        }

        public ConsumeResource(string resourceName, double amount, bool canBeRandom, string playerMessage) : base(canBeRandom, playerMessage)
        {
            this.resourceName = resourceName;
            this.amount = amount;

            PartResourceDefinitionList definitions = PartResourceLibrary.Instance.resourceDefinitions;
            if (definitions.Contains(resourceName))
                isRosterResource = false;
            else
                isRosterResource = true;
        }

        public override void ApplyOutcome(Vessel vessel, SnacksProcessorResult result)
        {
            if (!isRosterResource && vessel != null)
            {
                List<ProtoPartResourceSnapshot> protoPartResources = new List<ProtoPartResourceSnapshot>();
                double vesselCurrentAmount = 0;
                double vesselMaxAmount = 0;
                double demand = amount;

                //Select random amount if enabled
                if (randomMin != randomMax)
                    demand = UnityEngine.Random.Range(randomMin, randomMax);

                //Get current totals
                ProcessedResource.GetResourceTotals(vessel, resourceName, out vesselCurrentAmount, out vesselMaxAmount, protoPartResources);

                //If the vessel has no resource at all then it hasn't been visited in-game yet.
                if (vesselMaxAmount <= 0)
                {
                    result.resultType = SnacksResultType.notApplicable;

                    //Call the base class
                    base.ApplyOutcome(vessel, result);

                    return;
                }

                //Multiply demand by affected crew count
                if (result.appliedPerCrew && !selectRandomCrew)
                    demand *= result.affectedKerbalCount;

                //If we have enough to support the whole crew, then we're good.
                if ((vesselCurrentAmount / demand) >= 0.999)
                {
                    //Request resource
                    ProcessedResource.RequestResource(vessel, resourceName, demand, protoPartResources);
                }

                //We don't have enough to support the whole crew. Figure out how many we can support.
                else
                {
                    int totalServed = (int)Math.Floor(vesselCurrentAmount / amount);

                    demand = totalServed * amount;
                    ProcessedResource.RequestResource(vessel, resourceName, vesselCurrentAmount, protoPartResources);
                }

                //Inform player
                if (!string.IsNullOrEmpty(playerMessage))
                    ScreenMessages.PostScreenMessage(playerMessage, 5, ScreenMessageStyle.UPPER_LEFT);
            }

            else if (result.afftectedAstronauts.Count > 0)
            {
                ProtoCrewMember[] astronauts = null;
                AstronautData astronautData = null;
                string message = string.Empty;

                //Get the crew manifest
                astronauts = result.afftectedAstronauts.ToArray();

                //Select random crew if needed
                if (selectRandomCrew)
                {
                    int randomIndex = UnityEngine.Random.Range(0, astronauts.Length - 1);
                    astronauts = new ProtoCrewMember[] { astronauts[randomIndex] };
                }

                for (int index = 0; index < astronauts.Length; index++)
                {
                    astronautData = SnacksScenario.Instance.GetAstronautData(astronauts[index]);

                    if (astronautData.rosterResources.ContainsKey(resourceName))
                    {
                        SnacksRosterResource resource = astronautData.rosterResources[resourceName];
                        resource.amount -= amount;
                        if (resource.amount <= 0)
                            resource.amount = 0;
                        astronautData.rosterResources[resourceName] = resource;

                        SnacksScenario.onRosterResourceUpdated.Fire(vessel, resource, astronautData, astronauts[index]);
                    }

                    //Inform player
                    if (!string.IsNullOrEmpty(playerMessage))
                    {
                        message = vessel.vesselName + ": " + astronauts[index].name + " " + playerMessage;
                        ScreenMessages.PostScreenMessage(message, 5.0f, ScreenMessageStyle.UPPER_LEFT);
                    }
                }

                //Call the base class
                base.ApplyOutcome(vessel, result);
            }
        }
        #endregion
    }
}
