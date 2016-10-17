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
    internal class MissingMealsMonitor : ISnacksPenalty
    {
        public void GameSettingsApplied()
        {
        }

        public bool IsEnabled()
        {
            return true;
        }

        public bool AlwaysApply()
        {
            return true;
        }

        public void ApplyPenalty(int hungryKerbals, Vessel vessel)
        {
            ProtoCrewMember[] crewManifest;
            List<ProtoCrewMember> pilots = new List<ProtoCrewMember>();
            int kerbalsProcessed = 0;

            //Get the crew manifest
            if (vessel.loaded)
                crewManifest = vessel.GetVesselCrew().ToArray();
            else
                crewManifest = vessel.protoVessel.GetVesselCrew().ToArray();

            //If all of the crew missed a meal, then update all of them
            if (hungryKerbals >= crewManifest.Length)
            {
                for (int index = 0; index < crewManifest.Length; index++)
                    SnacksScenario.Instance.AddMissedMeals(crewManifest[index], 1);
            }

            //If the number of hungry kerbals < total crew size, then
            //Pick as many non-pilots as possible to avoid bricking the mission.
            else
            {
                for (int index = 0; index < hungryKerbals; index++)
                {
                    if (crewManifest[index].HasEffect("FullVesselControlSkill") == false)
                    {
                        SnacksScenario.Instance.AddMissedMeals(crewManifest[index], 1);
                        kerbalsProcessed += 1;
                    }
                    else
                    {
                        pilots.Add(crewManifest[index]);
                    }
                }

                //If we haven't processed all the hungry kerbals then star working the pilot's list.
                if (kerbalsProcessed < hungryKerbals)
                {
                    foreach (ProtoCrewMember pilot in pilots)
                    {
                        SnacksScenario.Instance.AddMissedMeals(pilot, 1);
                        kerbalsProcessed += 1;
                        if (kerbalsProcessed >= hungryKerbals)
                            break;
                    }
                }
            }
        }

        public void RemovePenalty(Vessel vessel)
        {
            ProtoCrewMember[] crewManifest;
            List<ProtoCrewMember> pilots = new List<ProtoCrewMember>();

            //Get the crew manifest
            if (vessel.loaded)
                crewManifest = vessel.GetVesselCrew().ToArray();
            else
                crewManifest = vessel.protoVessel.GetVesselCrew().ToArray();

            for (int index = 0; index < crewManifest.Length; index++)
                SnacksScenario.Instance.SetMealsMissed(crewManifest[index], 0);
        }
    }
}
