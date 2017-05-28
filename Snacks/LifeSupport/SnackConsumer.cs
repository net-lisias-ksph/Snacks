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

namespace Snacks
{
    class SnackConsumer
    {
        private static System.Random random = new System.Random();

        public static bool hasUnownedCrew(Vessel vessel)
        {
            int crewCount = 0;
            ProtoCrewMember[] astronauts = null;

            if (vessel.loaded)
            {
                crewCount = vessel.GetCrewCount();
                if (crewCount > 0)
                    astronauts = vessel.GetVesselCrew().ToArray();
            }
            else
            {
                crewCount = vessel.protoVessel.GetVesselCrew().Count;
                if (crewCount > 0)
                    astronauts = vessel.protoVessel.GetVesselCrew().ToArray();
            }

            if (crewCount > 0)
            {
                for (int index = 0; index < astronauts.Length; index++)
                {
                    if (astronauts[index].type == ProtoCrewMember.KerbalType.Unowned)
                        return true;
                }
            }

            return false;
        }

        public double GetResource(string resourceName, Part part, double demand)
        {
            PartResource resource = part.Resources[resourceName];
            double supplied = 0;

            if (resource.amount >= demand)
            {
                resource.amount -= demand;
                supplied += demand;
            }

            else
            {
                supplied += resource.amount;
                demand -= resource.amount;
                resource.amount = 0;
            }

            return supplied;
        }

        public double GetResource(List<ProtoPartSnapshot> protoPartSnapshots, string resourceName, double demand)
        {
            double supplied = 0;
            double remaining = demand;

            foreach (ProtoPartSnapshot pps in protoPartSnapshots)
            {
                foreach (ProtoPartResourceSnapshot resource in pps.resources)
                {
                    if (resource.resourceName == resourceName)
                    {
                        if (resource.amount >= remaining)
                        {
                            supplied += remaining;
                            resource.amount -= remaining;
                            remaining = 0;
                            break;
                        }

                        else
                        {
                            supplied += resource.amount;
                            remaining -= resource.amount;
                            resource.amount = 0;
                        }
                    }
                }

                if (remaining <= 0)
                    break;
            }

            return supplied;
        }

        public double AddResource(List<ProtoPartSnapshot> protoPartSnapshots, string resourceName, double demand)
        {
            double remaining = demand;
            double added = 0;
            bool resFound = false;
            foreach (ProtoPartSnapshot pps in protoPartSnapshots)
            {
                foreach (ProtoPartResourceSnapshot resource in pps.resources)
                {
                    if (resource.resourceName == resourceName)
                    {
                        if (resource.amount + remaining <= resource.maxAmount)
                        {
                            resource.amount += remaining;
                            added += remaining;
                            return added;
                        }
                        else
                        {
                            remaining -= resource.maxAmount - resource.amount;
                            added += resource.maxAmount - resource.amount;
                            resource.amount = resource.maxAmount;
                            resFound = true;
                        }
                    }
                }
            }

            if (!resFound)
                return demand;

            return added;
        }

        public double GetSnackResource(Part part, double demand)
        {
            return GetResource(SnacksProperties.SnacksResourceName, part, demand);
        }

        public double GetSnackResource(List<ProtoPartSnapshot> protoPartSnapshots, double demand)
        {
            return GetResource(protoPartSnapshots, SnacksProperties.SnacksResourceName, demand);
        }

        public double AddSoilResource(List<ProtoPartSnapshot> protoPartSnapshots, double demand)
        {
            return AddResource(protoPartSnapshots, SnacksProperties.SoilResourceName, demand);
        }
       
        /**
        * Removes the calculated number of snacks from the vessel.
        * returns the number of snacks that were required, but missing.
        * */
        public double ConsumeAndGetDeficit(Vessel vessel)
        {
            double demand = 0;
            double fed = 0;
            double crewCount = SnacksScenario.Instance.GetNonExemptCrewCount(vessel);

            //Calculate for loaded vessel
            if (vessel.loaded)
            {
                demand = vessel.GetCrewCount() * SnacksProperties.SnacksPerMeal + calculateExtraSnacksRequired(vessel.GetVesselCrew());

                if (demand <= 0)
                    return 0;

                fed = vessel.rootPart.RequestResource(SnacksProperties.SnacksResourceName, demand, ResourceFlowMode.ALL_VESSEL);
            }

            //Calculate for proto vessel
            else
            {
                //Unloaded vessels need to run their recyclers and snack processors.
                runConverters(vessel.protoVessel);

                //Now calculate demand.
                demand = vessel.protoVessel.GetVesselCrew().Count * SnacksProperties.SnacksPerMeal + calculateExtraSnacksRequired(vessel.protoVessel.GetVesselCrew());

                if (demand <= 0)
                    return 0;

                fed = GetSnackResource(vessel.protoVessel.protoPartSnapshots, demand);
            }

            //Fire consume snacks event
            //Gives listeners a chance to alter the values.
            SnackConsumption snackConsumption = new SnackConsumption();
            snackConsumption.demand = demand;
            snackConsumption.fed = fed;
            snackConsumption.vessel = vessel;
            SnackController.onConsumeSnacks.Fire(snackConsumption);

            //Request resource (loaded vessel)
            if (vessel.loaded)
            {
                if (fed == 0)
                    return vessel.GetCrewCount() * SnacksProperties.SnacksPerMeal;

                //If recycling is enabled then produce soil.
                if (SnacksProperties.RecyclersEnabled)
                    vessel.rootPart.RequestResource(SnacksProperties.SoilResourceName, -fed, ResourceFlowMode.ALL_VESSEL);
            }

            //Request resource (unloaded vessel)
            else
            {
                if (fed == 0)
                {
                    return vessel.protoVessel.GetVesselCrew().Count * SnacksProperties.SnacksPerMeal;
                }

                //If recycling is enabled then produce soil.
                if (SnacksProperties.RecyclersEnabled)
                    AddSoilResource(vessel.protoVessel.protoPartSnapshots, fed);
            }

            return demand - fed;
        }

        protected void getResourceRatios(string className, ProtoPartSnapshot protoPartSnapshot, Dictionary<string, double> inputs, Dictionary<string, double> outputs, out double recyclerCapacity)
        {
            recyclerCapacity = 0f;
            ConfigNode[] nodes = protoPartSnapshot.partInfo.partConfig.GetNodes("MODULE");
            ConfigNode processorNode = null;
            ConfigNode node = null;
            string moduleName;
            string resourceName;
            double resourceRatio;

            //Get the processor config node.
            for (int index = 0; index < nodes.Length; index++)
            {
                node = nodes[index];
                if (node.HasValue("name"))
                {
                    moduleName = node.GetValue("name");
                    if (moduleName == className)
                    {
                        processorNode = node;
                        break;
                    }
                }
            }
            if (processorNode == null)
                return;

            //Get the nodes we're interested in
            nodes = processorNode.GetNodes("INPUT_RESOURCE");
            for (int index = 0; index < nodes.Length; index++)
            {
                resourceName = nodes[index].GetValue("ResourceName");
                resourceRatio = double.Parse(nodes[index].GetValue("Ratio"));
                inputs.Add(resourceName, resourceRatio);
            }

            nodes = processorNode.GetNodes("OUTPUT_RESOURCE");
            for (int index = 0; index < nodes.Length; index++)
            {
                resourceName = nodes[index].GetValue("ResourceName");
                resourceRatio = double.Parse(nodes[index].GetValue("Ratio"));
                outputs.Add(resourceName, resourceRatio);
            }

            //Recycler: get capacity
            if (processorNode.HasValue("RecyclerCapacity"))
                recyclerCapacity = double.Parse(processorNode.GetValue("RecyclerCapacity"));
        }

        protected void runConverters(ProtoVessel vessel)
        {
            if (!SnacksProperties.RecyclersEnabled)
                return;
            ProtoPartModuleSnapshot[] modules = null;
            ProtoPartModuleSnapshot recycler = null;
            ProtoPartModuleSnapshot snackProcessor = null;
            string activated = string.Empty;
            double currentTime = Planetarium.GetUniversalTime();
            double lastUpdateTime, elapsedTime;
            Dictionary<string, double> inputs = new Dictionary<string, double>();
            Dictionary<string, double> outputs = new Dictionary<string, double>();
            double recyclerCapacity = 0f;
            double baseDemand, demand = 0f;
            double percentAcquired = 0f;
            double totalDemand = 0f;
            double totalObtained = 0f;
            double baseOutput = 0f;
            double output = 0f;

            //For each recycler and snack processor, run them manually.
            foreach (ProtoPartSnapshot pps in vessel.protoPartSnapshots)
            {
                modules = pps.modules.ToArray();
                recycler = null;
                snackProcessor = null;
                activated = string.Empty;
                recyclerCapacity = 0f;
                baseDemand = 0f;
                demand = 0f;
                percentAcquired = 1.0f;
                totalDemand = 0f;
                totalObtained = 0f;
                baseOutput = 0f;
                inputs.Clear();
                outputs.Clear();

                //See if the part has a recycler or snack processor that's active.
                for (int snapModIndex = 0; snapModIndex < modules.Length; snapModIndex++)
                {
                    if (modules[snapModIndex].moduleName == "SoilRecycler")
                    {
                        //Activation status
                        activated = modules[snapModIndex].moduleValues.GetValue("IsActivated").ToLower();
                        if (activated == "true")
                        {
                            recycler = modules[snapModIndex];
                        }
                    }

                    else if (modules[snapModIndex].moduleName == "SnackProcessor")
                    {
                        //Activation status
                        activated = modules[snapModIndex].moduleValues.GetValue("IsActivated").ToLower();
                        if (activated == "true")
                        {
                            snackProcessor = modules[snapModIndex];
                        }
                    }

                    if (recycler != null || snackProcessor != null)
                        break;
                }

                if (recycler != null)
                {
                    lastUpdateTime = double.Parse(recycler.moduleValues.GetValue("lastUpdateTime"));
                    elapsedTime = currentTime - lastUpdateTime;

                    //Get the resource ratios
                    getResourceRatios("SoilRecycler", pps, inputs, outputs, out recyclerCapacity);

                    //Calculate base demand
                    baseDemand = elapsedTime * recyclerCapacity * SnacksProperties.SnacksPerMeal * SnacksProperties.MealsPerDay;
                    Debug.Log("elapsedTime: " + elapsedTime + " baseDemand: " + baseDemand + " recyclerCapacity: " + recyclerCapacity + " SnacksPerMeal: " + SnacksProperties.SnacksPerMeal);

                    //Now request all the input resources
                    foreach (string resourceName in inputs.Keys)
                    {
                        //Skip ElectricCharge. There's no way to know how much is produced while you're away...
                        if (resourceName == "ElectricCharge")
                            continue;

                        //Calculate demand
                        demand = inputs[resourceName] * baseDemand;
                        totalDemand += demand;

                        //Request the resource
                        totalObtained += GetResource(vessel.protoPartSnapshots, resourceName, demand);
                        Debug.Log("Requested " + demand + " units of " + resourceName + " and total received thus far: " + totalObtained);
                    }

                    //Calculate base output
                    percentAcquired = totalObtained / totalDemand;
                    if (percentAcquired > 0.999)
                        percentAcquired = 1.0f;
                    baseOutput = baseDemand * percentAcquired * SnacksProperties.RecyclerEfficiency;
                    if (baseOutput < 0.00000000f)
                        baseOutput = 0f;
                    Debug.Log("baseOutput: " + baseOutput + " RecyclerEfficiency: " + SnacksProperties.RecyclerEfficiency);

                    //Get the output ratios and process them.
                    foreach (string resourceName in outputs.Keys)
                    {
                        output = outputs[resourceName] * baseOutput;
                        if (output > 0f)
                            AddResource(vessel.protoPartSnapshots, resourceName, output);
                        Debug.Log("Added " + outputs[resourceName] * baseOutput + " units of " + resourceName);
                    }

                    //Update the last update time.
                    recycler.moduleValues.SetValue("lastUpdateTime", Planetarium.GetUniversalTime());
                }

                if (snackProcessor != null)
                {
                    lastUpdateTime = double.Parse(recycler.moduleValues.GetValue("lastUpdateTime"));
                    elapsedTime = currentTime - lastUpdateTime;

                    //Get the resource ratios
                    recyclerCapacity = 0f;
                    inputs.Clear();
                    outputs.Clear();
                    getResourceRatios("SnackProcessor", pps, inputs, outputs, out recyclerCapacity);

                    //Calculate base demand
                    baseDemand = elapsedTime * SnacksProperties.ProductionEfficiency;

                    //Now request all the input resources
                    percentAcquired = 1.0f;
                    totalDemand = 0f;
                    totalObtained = 0f;
                    foreach (string resourceName in inputs.Keys)
                    {
                        //Skip ElectricCharge. There's no way to know how much is produced while you're away...
                        if (resourceName == "ElectricCharge")
                            continue;

                        //Calculate demand
                        demand = inputs[resourceName] * baseDemand;
                        totalDemand += demand;

                        //Request the resource
                        totalObtained += GetResource(vessel.protoPartSnapshots, resourceName, demand);
                    }

                    //Calculate base output
                    percentAcquired = totalObtained / totalDemand;
                    if (percentAcquired > 0.999)
                        percentAcquired = 1.0f;
                    baseOutput = baseDemand * recyclerCapacity * percentAcquired;
                    if (baseOutput < 0.00000000f)
                        baseOutput = 0f;

                    //Get the output ratios and process them.
                    foreach (string resourceName in outputs.Keys)
                    {
                        output = outputs[resourceName] * baseOutput;
                        if (output > 0f)
                            AddResource(vessel.protoPartSnapshots, resourceName, output);
                    }

                    //Update the last update time.
                    snackProcessor.moduleValues.SetValue("lastUpdateTime", Planetarium.GetUniversalTime());
                }
            }
        }

        private static bool getRandomChance(double prob)
        {
            if (random.NextDouble() < prob)
                return true;
            return false;
        }

        private static double calculateExtraSnacksRequired(List<ProtoCrewMember> crew)
        {
            if (SnacksProperties.EnableRandomSnacking == false)
                return 0;

            double extra = 0;
            foreach (ProtoCrewMember pc in crew)
            {
                AstronautData data = SnacksScenario.Instance.GetAstronautData(pc);
                if (data.isExempt)
                    continue;

                if (getRandomChance(pc.courage / 2.0))
                    extra += SnacksProperties.SnacksPerMeal;
                if (getRandomChance(pc.stupidity / 2.0))
                    extra -= SnacksProperties.SnacksPerMeal;
                if (pc.isBadass && getRandomChance(.2))
                    extra -= SnacksProperties.SnacksPerMeal;
            }
            return extra;
        }

    }
}
