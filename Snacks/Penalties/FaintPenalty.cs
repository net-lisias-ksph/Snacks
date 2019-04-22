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
    public class FaintPenalty : ISnacksPenalty
    {
        public const string FaintMessage = " has fainted from a lack of Snacks!";

        public static void CheckFaintKerbals(Vessel vessel)
        {
            if (!SnacksProperties.FaintWhenHungry)
                return;
            if (vessel.loaded == false)
                return;

            int mealsBeforeFainting = SnacksProperties.MealsBeforeFainting;
            float faintDuration = SnacksProperties.NapTime * 60f;
            ProtoCrewMember[] astronauts;
            AstronautData data;

            astronauts = vessel.GetVesselCrew().ToArray();

            //If crew member has gone hungry too many times then it's time
            //for the crew member to pass out.
            for (int index = 0; index < astronauts.Length; index++)
            {
                data = SnacksScenario.Instance.GetAstronautData(astronauts[index]);

                //Handle exemptions
                if (data.isExempt)
                    continue;

                if (data.mealsMissed >= mealsBeforeFainting)
                {
                    astronauts[index].SetInactive(faintDuration);
                    ScreenMessages.PostScreenMessage(astronauts[index].name + FaintMessage, 5.0f, ScreenMessageStyle.UPPER_LEFT);
                }
            }
        }

        public void GameSettingsApplied()
        {
            bool faintWhenHungry = SnacksProperties.FaintWhenHungry;

            foreach (Vessel vessel in FlightGlobals.Vessels)
            {
                if (faintWhenHungry)
                {
                    if (vessel.loaded)
                        CheckFaintKerbals(vessel);
                }

                else
                    ClearFainting(vessel);
            }
        }

        #region ISnacksPenalty
        public bool IsEnabled()
        {
            return SnacksProperties.FaintWhenHungry;
        }

        public bool AlwaysApply()
        {
            return !SnacksProperties.RandomPenaltiesEnabled;
        }

        public void ApplyPenalty(int hungryKerbals, Vessel vessel)
        {
            CheckFaintKerbals(vessel);
        }

        public static void ClearFainting(Vessel vessel)
        {
            ProtoCrewMember[] astronauts;

            if (vessel.loaded)
                astronauts = vessel.GetVesselCrew().ToArray();
            else
                astronauts = vessel.protoVessel.GetVesselCrew().ToArray();

            for (int index = 0; index < astronauts.Length; index++)
            {
                astronauts[index].inactive = false;
                SnacksScenario.Instance.SetMealsMissed(astronauts[index], 0);
            }
        }

        public void RemovePenalty(Vessel vessel)
        {
            ClearFainting(vessel);
        }
        #endregion

    }
}
