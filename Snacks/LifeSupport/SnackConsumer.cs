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

        public double GetSnackResource(Part part, double demand)
        {
            PartResource resource = part.Resources[SnacksProperties.SnacksResourceName];
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

        public double GetSnackResource(List<ProtoPartSnapshot> protoPartSnapshots, double demand)
        {
            double supplied = 0;
            double remaining = demand;

            foreach (ProtoPartSnapshot pps in protoPartSnapshots)
            {
                foreach (ProtoPartResourceSnapshot resource in pps.resources)
                {
                    if (resource.resourceName == SnacksProperties.SnacksResourceName)
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

        public double AddSoilResource(List<ProtoPartSnapshot> protoPartSnapshots, double demand)
        {
            double remaining = demand;
            double added = 0;
            bool resFound = false;
            foreach (ProtoPartSnapshot pps in protoPartSnapshots)
            {
                foreach (ProtoPartResourceSnapshot resource in pps.resources)
                {
                    if (resource.resourceName == SnacksProperties.SoilResourceName)
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
