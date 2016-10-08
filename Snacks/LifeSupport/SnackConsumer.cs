/**
The MIT License (MIT)
Copyright (c) 2014 Troy Gruetzmacher

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
            bool resFound = false;
            foreach (ProtoPartSnapshot pps in protoPartSnapshots)
            {
                foreach (ProtoPartResourceSnapshot resource in pps.resources)
                {
                    if (resource.resourceName == SnacksProperties.SnacksResourceName)
                    {
                         if (resource.amount >= demand)
                        {
                            resource.amount -= demand;
                            return supplied;
                        }
                        else
                        {
                            supplied += resource.amount;
                            demand -= resource.amount;
                            resFound = true;
                        }
                    }
                }
            }

            //No resources found? Too bad!
            if (!resFound)
                return 0;

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
        public double ConsumeAndGetDeficit(ProtoVessel pv)
        {
            double demand = pv.GetVesselCrew().Count * SnacksProperties.SnacksPerMeal;
            double extra = calculateExtraSnacksRequired(pv.GetVesselCrew());

            if ((demand + extra) <= 0)
                return 0;

            double fed = GetSnackResource(pv.protoPartSnapshots, demand + extra);
            if (fed == 0)//unable to feed, no skipping or extra counted
                return pv.GetVesselCrew().Count * SnacksProperties.SnacksPerMeal;

            //If recycling is enabled then produce soil.
            if (SnacksProperties.RecyclersEnabled)
                AddSoilResource(pv.protoPartSnapshots, fed);

           return demand + extra - fed;
        }

        /**
        * Removes the calculated number of snacks from the vessel.
        * returns the number of snacks that were required, but missing.
        * */
        public double ConsumeAndGetDeficit(Vessel v)
        {
            double demand = CalculateDemand(v);

            //Debug.Log("SnackDemand(" + v.vesselName + "): e: " + extra + " r:" + demand);
            if ((demand) <= 0)
                return 0;

            double fed = v.rootPart.RequestResource(SnacksProperties.SnacksResourceName, demand, ResourceFlowMode.ALL_VESSEL);
            if (fed == 0)//unable to feed, no skipping or extra counted
                return v.GetCrewCount() * SnacksProperties.SnacksPerMeal;

            //If recycling is enabled then produce soil.
            if (SnacksProperties.RecyclersEnabled)
                v.rootPart.RequestResource(SnacksProperties.SoilResourceName, -fed, ResourceFlowMode.ALL_VESSEL);

            return demand - fed;
        }

        public static double CalculateDemand(ProtoVessel pv)
        {
            double demand = pv.GetVesselCrew().Count * SnacksProperties.SnacksPerMeal;
            double extra = calculateExtraSnacksRequired(pv.GetVesselCrew());

            return demand + extra;
        }

        public static double CalculateDemand(Vessel v)
        {
            double demand = v.GetCrewCount() * SnacksProperties.SnacksPerMeal;
            double extra = calculateExtraSnacksRequired(v.GetVesselCrew());

            return demand + extra;
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
