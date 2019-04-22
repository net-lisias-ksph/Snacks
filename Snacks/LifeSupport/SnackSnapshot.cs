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
using KSP;
using KSP.UI;

namespace Snacks
{
    public class SnackSnapshot
    {

        private SnackSnapshot()
        {}

        private static SnackSnapshot snapshot;

        public static SnackSnapshot Instance()
        {
            if (snapshot == null)
            {
                snapshot = new SnackSnapshot();
                snapshot.TakeSnapshot();
            }
            return snapshot;
        }

        private Dictionary<int, List<ShipSupply>> vessels;
        private Dictionary<Guid, bool> outOfSnacks;

        public ShipSupply TakeEditorSnapshot()
        {
            ShipSupply shipSupply = new ShipSupply();
            double snackProduction = 0;
            double snackConsumption = 0;
            double snackConsumptionMax = 0;
            double currentSnacks = 0;
            double maxSnacks = 0;
            double recycleCapacity = 0;
            double snacksPerKerbal = SnacksProperties.MealsPerDay * SnacksProperties.SnacksPerMeal;
            int maxCrew = 0;
            Part[] parts = EditorLogic.fetch.ship.parts.ToArray();
            Part part;
            SoilRecycler[] recyclers;
            VesselCrewManifest manifest = CrewAssignmentDialog.Instance.GetManifest();

            //No parts? Then we're done
            if (parts.Length == 0)
                return shipSupply;
            if (manifest == null)
                return shipSupply;
            if (manifest.CrewCount == 0)
                return shipSupply;
            int crewCount = manifest.CrewCount;

            for (int index = 0; index < parts.Length; index++)
            {
                part = parts[index];

                //Update max crew
                maxCrew += part.CrewCapacity;

                //Make sure the crewed part has snacks
                if (!part.Resources.Contains(SnacksProperties.SnacksResourceName) && part.CrewCapacity >= 1)
                {
                    ConfigNode node = new ConfigNode("RESOURCE");
                    double amount = 0;
                    node.AddValue("name", SnacksProperties.SnacksResourceName);
                    if (part.FindModuleImplementing<ModuleCommand>() != null)
                        amount = SnacksProperties.SnacksPerCommand * part.CrewCapacity;
                    else
                        amount = SnacksProperties.SnacksPerCrewModule * part.CrewCapacity;
                    node.AddValue("amount", amount.ToString());
                    node.AddValue("maxAmount", amount.ToString());
                    part.AddResource(node);
                }
                
                //Update snack resource values
                if (part.Resources.Contains(SnacksProperties.SnacksResourceName))
                {
                    if (part.Resources[SnacksProperties.SnacksResourceName].flowState)
                    {
                        currentSnacks += part.Resources[SnacksProperties.SnacksResourceName].amount;
                        maxSnacks += part.Resources[SnacksProperties.SnacksResourceName].maxAmount;
                    }
                }

                //Compute snack production
                if (SnacksProperties.RecyclersEnabled)
                {
                    recyclers = part.FindModulesImplementing<SoilRecycler>().ToArray();
                    for (int recyclerIndex = 0; recyclerIndex < recyclers.Length; recyclerIndex++)
                    {
                        snackProduction += recyclers[recyclerIndex].GetDailySnacksOutput();
                        recycleCapacity += recyclers[recyclerIndex].RecyclerCapacity;
                    }
                }
            }

            //Calculate consumption estimates. Account for crew being less than max recycling capacity
            snackConsumption = snacksPerKerbal * crewCount;
            if (crewCount >= recycleCapacity)
                snackConsumption -= snackProduction;

            else
                snackConsumption -= snackProduction * (crewCount / recycleCapacity);

            snackConsumptionMax = snacksPerKerbal * maxCrew;
            if (maxCrew >= recycleCapacity)
                snackConsumptionMax -= snackProduction;
            else
                snackConsumptionMax -= snackProduction * (maxCrew / recycleCapacity);

            if (snackConsumption == 0 || snackConsumptionMax == 0)
                return shipSupply;

            //Setup the supply snapshot
            shipSupply.SnackAmount = Convert.ToInt32(currentSnacks);
            shipSupply.SnackMaxAmount = Convert.ToInt32(maxSnacks);
            shipSupply.CrewCount = crewCount;
            shipSupply.DayEstimate = Convert.ToInt32(currentSnacks / snackConsumption);
            shipSupply.MaxCrewCount = maxCrew;
            shipSupply.MaxDayEstimate = Convert.ToInt32(maxSnacks / snackConsumptionMax);

            return shipSupply;
        }

        protected void takeSnapshotProto(Dictionary<int, List<ShipSupply>> vessels)
        {
            double snacksPerKerbal = SnacksProperties.MealsPerDay * SnacksProperties.SnacksPerMeal;
            ProtoVessel pv;
            ProtoVessel[] protoVessels;
            double snackAmount = 0;
            double snackMax = 0;
            double snackConsumption = 0;
            double snackProduction = 0;
            double recycleCapacity = 0;
            int crewCount;
            int partCrewCount;
            bool foundSnackResource = false;

            protoVessels = HighLogic.CurrentGame.flightState.protoVessels.ToArray();
            for (int index = 0; index < protoVessels.Length; index++)
            {
                pv = protoVessels[index];

                //Skip vessels with unowned crew
                if (SnackConsumer.hasUnownedCrew(pv.vesselRef))
                    continue;

                //Debug.Log("processing pv:" + pv.vesselName);
                if (!pv.vesselRef.loaded)
                {
                    crewCount = pv.GetVesselCrew().Count;
                    if (crewCount < 1)
                        continue;
                    snackAmount = 0;
                    snackMax = 0;
                    snackConsumption = crewCount * snacksPerKerbal;
                    foreach (ProtoPartSnapshot pps in pv.protoPartSnapshots)
                    {
                        foundSnackResource = false;
                        partCrewCount = pps.partInfo.partPrefab.CrewCapacity;

                        //Tally the snack and max snack amounts
                        foreach (ProtoPartResourceSnapshot resource in pps.resources)
                        {
                            if (resource.resourceName == SnacksProperties.SnacksResourceName)
                            {
                                if (resource.flowState)
                                {
//                                    Debug.Log("Found snacks in " + pps.partName);
                                    snackAmount += resource.amount;
                                    snackMax += resource.maxAmount;
                                }
                                foundSnackResource = true;
                            }
                        }

                        //Add Snacks if we don't find any
                        if (!foundSnackResource && partCrewCount > 0)
                        {
//                            Debug.Log("Part should have Snacks but doesn't. Trying to add Snacks to " + pps.partName);
                            int snacksPer = SnacksProperties.SnacksPerCrewModule;

                            //See if we have a command module. If so, that will affect how many snacks we have in the part.
                            ProtoPartModuleSnapshot[] modules = pps.modules.ToArray();
                            for (int moduleIndex = 0; moduleIndex < modules.Length; moduleIndex++)
                            {
                                if (modules[moduleIndex].moduleName == "ModuleCommand")
                                {
                                    snacksPer = SnacksProperties.SnacksPerCommand;
                                    break;
                                }
                            }

                            ConfigNode snackNode = new ConfigNode("RESOURCE");
                            int totalSnacks = pps.partInfo.partPrefab.CrewCapacity * snacksPer;
                            snackNode.AddValue("name", SnacksProperties.SnacksResourceName);
                            snackNode.AddValue("amount", totalSnacks);
                            snackNode.AddValue("maxAmount", totalSnacks);
                            pps.resources.Add(new ProtoPartResourceSnapshot(snackNode));
                            snackAmount += totalSnacks;
                            snackMax += totalSnacks;
                        }

                        //Check for recyclers
                        if (SnacksProperties.RecyclersEnabled)
                        {
                            bool isActivated = false;
                            ConfigNode[] snapModules = pps.partInfo.partConfig.GetNodes("MODULE");
                            ConfigNode snapModule = null;
                            ProtoPartModuleSnapshot[] modules = pps.modules.ToArray();

                            //See if the part has a recycler
                            for (int moduleIndex = 0; moduleIndex < snapModules.Length; moduleIndex++)
                            {
                                if (snapModules[moduleIndex].GetValue("name") == "SoilRecycler")
                                {
                                    snapModule = snapModules[moduleIndex];
                                    break;
                                }
                            }

                            //Check for activation
                            for (int snapModIndex = 0; snapModIndex < modules.Length; snapModIndex++)
                            {
                                if (modules[snapModIndex].moduleName == "SoilRecycler")
                                {
                                    //Activation status
                                    string activated = modules[snapModIndex].moduleValues.GetValue("IsActivated").ToLower();
                                    if (activated == "true")
                                    {
                                        isActivated = true;
                                        break;
                                    }
                                }
                            }

                            //If it has a recycler then calculate the snack production if the recycler is active
                            if (snapModule != null && isActivated)
                            {
                                //Capacity
                                if (snapModule.HasValue("RecyclerCapacity"))
                                    recycleCapacity += int.Parse(snapModule.GetValue("RecyclerCapacity"));

                                ConfigNode[] outputs = snapModule.GetNodes("OUTPUT_RESOURCE");
                                for (int outputIndex = 0; outputIndex < outputs.Length; outputIndex++)
                                {
                                    if (outputs[outputIndex].GetValue("ResourceName") == SnacksProperties.SnacksResourceName)
                                    {
                                        snackProduction += double.Parse(outputs[outputIndex].GetValue("Ratio")) * SnacksProperties.RecyclerEfficiency * 21600;
                                    }
                                }
                            }
                        }
                    }

                   if (SnacksProperties.RecyclersEnabled)
                    {
                        //Account for recyclers
                        if (crewCount >= recycleCapacity)
                            snackConsumption -= snackProduction;
                        else
                            snackConsumption -= snackProduction * (crewCount / recycleCapacity);
                    }

                    //Debug.Log(pv.vesselName + "1");
                    ShipSupply supply = new ShipSupply();
                    supply.VesselName = pv.vesselName;
                    supply.BodyName = pv.vesselRef.mainBody.name;
                    supply.SnackAmount = Convert.ToInt32(snackAmount);
                    supply.SnackMaxAmount = Convert.ToInt32(snackMax);
                    supply.CrewCount = crewCount;
                    //Debug.Log(pv.vesselName + supply.CrewCount);
                    supply.DayEstimate = Convert.ToInt32(snackAmount / snackConsumption);
                    //Debug.Log(pv.vesselName + supply.DayEstimate);
                    //Debug.Log("sa:" + snackAmount + " sm:" + snackMax);
                    supply.Percent = snackMax == 0 ? 0 : Convert.ToInt32(snackAmount / snackMax * 100);
                    //Debug.Log(pv.vesselName + supply.Percent);
                    AddShipSupply(supply, pv.orbitSnapShot.ReferenceBodyIndex);
                    outOfSnacks.Add(pv.vesselID, snackAmount != 0.0 ? false : true);
                }

            }
        }

        protected void TakeSnapshotLoaded(Dictionary<int, List<ShipSupply>> vessels)
        {
            vessels = new Dictionary<int, List<ShipSupply>>();
            double snacksPerKerbal = SnacksProperties.MealsPerDay * SnacksProperties.SnacksPerMeal;
            Vessel v;
            Vessel[] loadedVessels;
            double snackAmount = 0;
            double snackMax = 0;
            double snackConsumption = 0;
            double snackProduction = 0;
            double recycleCapacity = 0;
            int crewCount;

            loadedVessels = FlightGlobals.VesselsLoaded.ToArray();
            for (int index = 0; index < loadedVessels.Length; index++)
            {
                v = loadedVessels[index];

                //Skip vessels with unowned crew
                if (SnackConsumer.hasUnownedCrew(v))
                    continue;

                crewCount = v.GetVesselCrew().Count;
                //Debug.Log("processing v:" + v.vesselName);
                if (crewCount > 0)
                {
                    v.resourcePartSet.GetConnectedResourceTotals(SnacksProperties.SnackResourceID, out snackAmount, out snackMax, true);

                    //Calculate snack consumption
                    snackConsumption = crewCount * snacksPerKerbal;
                    snackProduction = 0;
                    if (SnacksProperties.RecyclersEnabled)
                    {
                        SoilRecycler[] recyclers = v.FindPartModulesImplementing<SoilRecycler>().ToArray();
                        for (int recyclerIndex = 0; recyclerIndex < recyclers.Length; recyclerIndex++)
                        {
                            if (recyclers[recyclerIndex].IsActivated)
                            {
                                snackProduction += recyclers[recyclerIndex].GetDailySnacksOutput();
                                recycleCapacity += recyclers[recyclerIndex].RecyclerCapacity;
                            }
                        }

                        //Account for the recyclers
                        if (crewCount >= recycleCapacity)
                            snackConsumption -= snackProduction;
                        else
                            snackConsumption -= snackProduction * (crewCount / recycleCapacity);
                    }

                    ShipSupply supply = new ShipSupply();
                    supply.VesselName = v.vesselName;
                    supply.BodyName = v.mainBody.name;
                    supply.SnackAmount = Convert.ToInt32(snackAmount);
                    supply.SnackMaxAmount = Convert.ToInt32(snackMax);
                    supply.CrewCount = crewCount;
                    supply.DayEstimate = Convert.ToInt32(snackAmount / snackConsumption);
                    supply.Percent = snackMax == 0 ? 0 : Convert.ToInt32(snackAmount / snackMax * 100);
                    AddShipSupply(supply, v.protoVessel.orbitSnapShot.ReferenceBodyIndex);
                    outOfSnacks.Add(v.id, snackAmount != 0.0 ? false : true);
                }
            }
        }

        public Dictionary<int, List<ShipSupply>> TakeSnapshot()
        {
            try
            {
                vessels = new Dictionary<int, List<ShipSupply>>();
                outOfSnacks = new Dictionary<Guid, bool>();

                TakeSnapshotLoaded(vessels);
                takeSnapshotProto(vessels);
            }
            catch (Exception ex)
            {
                Debug.Log("building snapshot failed: " + ex.Message + ex.StackTrace);
            }
            return vessels;
        }

        private void AddShipSupply(ShipSupply supply, int planet)
        {
            if (!vessels.ContainsKey(planet))
                vessels.Add(planet, new List<ShipSupply>());

            List<ShipSupply> ships;
            bool suc = vessels.TryGetValue(planet, out ships);
            ships.Add(supply);
        }

        public void RebuildSnapshot()
        {
            vessels = null;
        }

    }

}
