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
using KSP.IO;

namespace Snacks
{
    public class DeathPenalty : ISnacksPenalty
    {
        public const string kDeathMessage = " has died from a lack of Snacks!";

        public static void CheckDeadKerbals(Vessel vessel)
        {
            if (!SnacksProperties.CanStarveToDeath)
                return;

            int mealsBeforeDeath = SnacksProperties.MealsSkippedBeforeDeath;
            ProtoCrewMember[] astronauts;
            AstronautData data;

            //Get the astronauts
            if (vessel.loaded)
                astronauts = vessel.GetVesselCrew().ToArray();
            else
                astronauts = vessel.protoVessel.GetVesselCrew().ToArray();

            //If crew member has gone hungry too many times then it's time to die.
            List<ProtoCrewMember> doomed = new List<ProtoCrewMember>();
            KerbalRoster roster = HighLogic.CurrentGame.CrewRoster;
            for (int index = 0; index < astronauts.Length; index++)
            {
                data = SnacksScenario.Instance.GetAstronautData(astronauts[index]);

                //Handle exemptions
                if (data.isExempt)
                    continue;

                //Add to our cleanup list
                if (data.mealsMissed >= mealsBeforeDeath)
                    doomed.Add(astronauts[index]);
            }

            //Remove the dead crew
            int count = doomed.Count;
            string message = "";
            for (int index = 0; index < count; index++)
            {
                //Unregister the crew member
                SnacksScenario.Instance.UnregisterCrew(doomed[index]);

                //Remove from ship
                doomed[index].seat.part.RemoveCrewmember(doomed[index]);
                if (vessel.loaded)
                {
                    vessel.RemoveCrew(doomed[index]);
                    vessel.CrewListSetDirty();
                }
                else
                {
                    vessel.protoVessel.RemoveCrew(doomed[index]);
                    vessel.CrewListSetDirty();
                }

                //Mark status
                astronauts[index].rosterStatus = ProtoCrewMember.RosterStatus.Dead;

                //Give player the bad news
                message = astronauts[index].name + kDeathMessage;
                Debug.Log("[DeathPenalty] - " + message);
                ScreenMessages.PostScreenMessage(message, 5.0f, ScreenMessageStyle.UPPER_CENTER);
            }
        }

        public bool IsEnabled()
        {
            return SnacksProperties.CanStarveToDeath;
        }

        public bool AlwaysApply()
        {
            return true;
        }

        public void ApplyPenalty(int hungryKerbals, Vessel vessel)
        {
            CheckDeadKerbals(vessel);
        }

        public void RemovePenalty(Vessel vessel)
        {
            //NOP, can't make a kerbal undead...
        }

        public void GameSettingsApplied()
        {
            //NOP
        }
    }
}
