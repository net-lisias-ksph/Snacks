using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP;
using KSP.UI;

namespace Snacks
{
    class SnackSnapshot
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
            double snacksPerKerbal = SnacksProperties.MealsPerDay * SnacksProperties.SnacksPerMeal;
            int maxCrew = 0;
            Part[] parts = EditorLogic.fetch.ship.parts.ToArray();
            Part part;
            SnacksRecycler[] recyclers;
            VesselCrewManifest manifest = CrewAssignmentDialog.Instance.GetManifest();

            //No parts? Then we're done
            if (parts.Length == 0)
                return shipSupply;

            for (int index = 0; index < parts.Length; index++)
            {
                part = parts[index];

                //Update max crew
                maxCrew += part.CrewCapacity;

                //Update snack resource values
                if (part.Resources.Contains(SnacksProperties.SnacksResourceName))
                {
                    currentSnacks += part.Resources[SnacksProperties.SnacksResourceName].amount;
                    maxSnacks += part.Resources[SnacksProperties.SnacksResourceName].maxAmount;
                }

                //Compute snack production
                if (SnacksProperties.RecyclersEnabled)
                {
                    recyclers = part.FindModulesImplementing<SnacksRecycler>().ToArray();
                    for (int recyclerIndex = 0; recyclerIndex < recyclers.Length; recyclerIndex++)
                        snackProduction += recyclers[recyclerIndex].GetDailySnacksRecycled();
                }
            }

            //Calculate consumption estimates
            snackConsumption = (snacksPerKerbal * manifest.CrewCount) - snackProduction;
            snackConsumptionMax = (snacksPerKerbal * maxCrew) - snackProduction;
            if (snackConsumption == 0 || snackConsumptionMax == 0)
                return shipSupply;

            //Setup the supply snapshot
            shipSupply.SnackAmount = Convert.ToInt32(currentSnacks);
            shipSupply.SnackMaxAmount = Convert.ToInt32(maxSnacks);
            shipSupply.CrewCount = manifest.CrewCount;
            shipSupply.DayEstimate = Convert.ToInt32(currentSnacks / snackConsumption);
            shipSupply.MaxCrewCount = maxCrew;
            shipSupply.MaxDayEstimate = Convert.ToInt32(maxSnacks / snackConsumptionMax);

            return shipSupply;
        }

        public Dictionary<int, List<ShipSupply>> TakeSnapshot()
        {
            try
            {
                //Debug.Log("rebuilding snapshot");
                vessels = new Dictionary<int, List<ShipSupply>>();
                outOfSnacks = new Dictionary<Guid, bool>();
                double snacksPerKerbal = SnacksProperties.MealsPerDay * SnacksProperties.SnacksPerMeal;
                Vessel v;
                ProtoVessel pv;
                List<Guid> activeVessels = new List<Guid>();
                Vessel[] loadedVessels;
                ProtoVessel[] protoVessels;

                loadedVessels = FlightGlobals.VesselsLoaded.ToArray();
                for (int index = 0; index < loadedVessels.Length; index++)
                {
                    v = loadedVessels[index];
                    //Debug.Log("processing v:" + v.vesselName);
                    if (v.GetVesselCrew().Count > 0)
                    {
                        activeVessels.Add(v.id);
                        double snackAmount = 0;
                        double snackMax = 0;
                        v.resourcePartSet.GetConnectedResourceTotals(SnacksProperties.SnackResourceID, out snackAmount, out snackMax, true);

                        //Calculate snack consumption
                        double snackConsumption = v.GetVesselCrew().Count * snacksPerKerbal;
                        double snackProduction = 0;
                        if (SnacksProperties.RecyclersEnabled)
                        {
                            SnacksRecycler[] recyclers = v.FindPartModulesImplementing<SnacksRecycler>().ToArray();
                            for (int recyclerIndex = 0; recyclerIndex < recyclers.Length; recyclerIndex++)
                            {
                                if (recyclers[recyclerIndex].IsActivated)
                                    snackProduction += recyclers[recyclerIndex].GetDailySnacksRecycled();
                            }

                            //Account for the recyclers
                            snackConsumption -= snackProduction;
                        }

                        ShipSupply supply = new ShipSupply();
                        supply.VesselName = v.vesselName;
                        supply.BodyName = v.mainBody.name;
                        supply.SnackAmount = Convert.ToInt32(snackAmount);
                        supply.SnackMaxAmount = Convert.ToInt32(snackMax);
                        supply.CrewCount = v.GetVesselCrew().Count;
                        supply.DayEstimate = Convert.ToInt32(snackAmount / snackConsumption);
                        supply.Percent = snackMax == 0 ? 0 : Convert.ToInt32(snackAmount / snackMax * 100);
                        AddShipSupply(supply, v.protoVessel.orbitSnapShot.ReferenceBodyIndex);
                        outOfSnacks.Add(v.id, snackAmount != 0.0 ? false : true);
                    }
                }

                protoVessels = HighLogic.CurrentGame.flightState.protoVessels.ToArray();
                for (int index = 0; index < protoVessels.Length; index++)
                {
                    pv = protoVessels[index];
                    //Debug.Log("processing pv:" + pv.vesselName);
                    if (!pv.vesselRef.loaded && !activeVessels.Contains(pv.vesselID))
                    {
                        if (pv.GetVesselCrew().Count < 1)
                            continue;
                        double snackAmount = 0;
                        double snackMax = 0;
                        double snackConsumption = pv.GetVesselCrew().Count * snacksPerKerbal;
                        double snackProduction = 0;
                        foreach (ProtoPartSnapshot pps in pv.protoPartSnapshots)
                        {
                            foreach (ProtoPartResourceSnapshot resource in pps.resources)
                            {
                                if (resource.resourceName == SnacksProperties.SnacksResourceName)
                                {
                                    snackAmount += resource.amount;
                                    snackMax += resource.maxAmount;
                                }
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
                                    if (snapModules[moduleIndex].GetValue("name") == "SnacksRecycler")
                                    {
                                        Debug.Log("found recycler");
                                        snapModule = snapModules[moduleIndex];
                                        break;
                                    }
                                }

                                //Check for activation
                                for (int snapModIndex = 0; snapModIndex < modules.Length; snapModIndex++)
                                {
                                    if (modules[snapModIndex].moduleName == "SnacksRecycler")
                                    {
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
                                    ConfigNode[] outputs = snapModule.GetNodes("OUTPUT_RESOURCE");
                                    for (int outputIndex = 0; outputIndex < outputs.Length; outputIndex++)
                                    {
                                        if (outputs[outputIndex].GetValue("ResourceName") == SnacksProperties.SnacksResourceName)
                                        {
                                            snackProduction += double.Parse(outputs[outputIndex].GetValue("Ratio")) * SnacksProperties.RecyclerEfficiency * 21600;
                                        }
                                    }
                                }

                                //Account for recyclers
                                if (snapModule != null)
                                {
                                    Debug.Log("snackConsumption: " + snackConsumption);
                                    Debug.Log("snackProduction: " + snackProduction);
                                }
                                snackConsumption -= snackProduction;
                            }
                        }

                        //Debug.Log(pv.vesselName + "1");
                        ShipSupply supply = new ShipSupply();
                        supply.VesselName = pv.vesselName;
                        supply.BodyName = pv.vesselRef.mainBody.name;
                        supply.SnackAmount = Convert.ToInt32(snackAmount);
                        supply.SnackMaxAmount = Convert.ToInt32(snackMax);
                        supply.CrewCount = pv.GetVesselCrew().Count;
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
