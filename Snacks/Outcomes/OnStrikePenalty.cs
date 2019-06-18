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
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.IO;

namespace Snacks
{
    public class OnStrikePenalty: BaseOutcome
    {
        #region Constants
        const string ValueConditionName = "conditionName";
        #endregion

        #region Housekeeping
        public string conditionName = "On Strike";
        #endregion

        #region Constructors
        public OnStrikePenalty(string conditionName, bool canBeRandom, string playerMessage) : base(canBeRandom, playerMessage)
        {
            this.conditionName = conditionName;
        }

        public OnStrikePenalty(ConfigNode node) : base(node)
        {
            if (node.HasValue(ValueConditionName))
                conditionName = node.GetValue(ValueConditionName);
        }

        #endregion

        #region API
        public override void ApplyOutcome(Vessel vessel, SnacksProcessorResult result)
        {
            ProtoCrewMember[] astronauts = null;
            AstronautData astronautData = null;
            string message = string.Empty;

            //Get the crew manifest
            if (result.afftectedAstronauts != null)
                astronauts = result.afftectedAstronauts.ToArray();
            else if (vessel.loaded)
                astronauts = vessel.GetVesselCrew().ToArray();
            else
                astronauts = vessel.protoVessel.GetVesselCrew().ToArray();

            //Go through each kerbal and set their condition
            for (int index = 0; index < astronauts.Length; index++)
            {
                astronautData = SnacksScenario.Instance.GetAstronautData(astronauts[index]);

                astronautData.SetCondition(conditionName);

                //If the vessel is loaded then remove skills
                if (vessel.loaded)
                    SnacksScenario.Instance.RemoveSkills(astronauts[index]);

                //Inform player
                if (!string.IsNullOrEmpty(playerMessage))
                {
                    message = vessel.vesselName + ": " + astronauts[index].name + " " + playerMessage;
                    ScreenMessages.PostScreenMessage(message, 5.0f, ScreenMessageStyle.UPPER_LEFT);
                }
            }
        }

        public override void RemoveOutcome(Vessel vessel)
        {
            ProtoCrewMember[] astronauts = null;
            AstronautData astronautData = null;

            //Get the crew manifest
            if (vessel.loaded)
                astronauts = vessel.GetVesselCrew().ToArray();
            else
                astronauts = vessel.protoVessel.GetVesselCrew().ToArray();

            //Go through each kerbal and set their condition
            for (int index = 0; index < astronauts.Length; index++)
            {
                astronautData = SnacksScenario.Instance.GetAstronautData(astronauts[index]);

                //If the vessel is loaded then restore skills
                if (vessel.loaded)
                    SnacksScenario.Instance.RestoreSkillsIfNeeded(astronauts[index]);

                astronautData.ClearCondition(conditionName);
            }
        }

        public override bool IsEnabled()
        {
            return true;
        }
        #endregion
    }
}
