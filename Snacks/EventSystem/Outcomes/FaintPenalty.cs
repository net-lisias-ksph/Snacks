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
    /// <summary>
    /// This outcome causes affected kerbals to faint.
    /// Example definition:
    /// OUTCOME 
    /// {
    ///     name  = FaintPenalty
    ///     resourceName = Snacks
    ///     cyclesBeforeFainting = 3
    ///     faintDurationSeconds = 180
    /// }
    /// </summary>   
    public class FaintPenalty : BaseOutcome
    {
        #region Constants
        const string FaintDurationKey = "faintDuration";
        const string ValueCyclesBeforeFainting = "cyclesBeforeFainting";
        const string ValueFaintDurationSeconds = "faintDurationSeconds";
        const string FaintMessageKey = "faintMessage";
        #endregion

        #region Housekeeping
        /// <summary>
        /// The name of the resource to check for failed processor cycles.
        /// </summary>
        public string resourceName = string.Empty;

        /// <summary>
        /// The number of cycles that must fail before the kerbal faints.
        /// </summary>
        public int cyclesBeforeFainting = 0;

        /// <summary>
        /// The number of seconds that the kerbal will faint for.
        /// </summary>
        public float faintDurationSeconds = 0;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Snacks.FaintPenalty"/> class.
        /// </summary>
        /// <param name="node">A ConfigNode containing initialization parameters. Parameters in the
        /// <see cref="T:Snacks.BaseOutcome"/> class also apply.</param>
        public FaintPenalty(ConfigNode node) : base(node)
        {
            if (node.HasValue(ResourceName))
                resourceName = node.GetValue(ResourceName);

            if (node.HasValue(ResourceName))
                resourceName = node.GetValue(ResourceName);

            if (node.HasValue(ValueCyclesBeforeFainting))
                int.TryParse(node.GetValue(ValueCyclesBeforeFainting), out cyclesBeforeFainting);

            if (node.HasValue(ValueFaintDurationSeconds))
                float.TryParse(node.GetValue(ValueFaintDurationSeconds), out faintDurationSeconds);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Snacks.FaintPenalty"/> class.
        /// </summary>
        /// <param name="resourceName">The name of the resource to check. If none of the resource processors have failed cycles
        /// containing the resource then the outcome is invalidated.</param>
        /// <param name="cyclesBeforeFainting">The number of failed cycles required before applying the outcome..</param>
        /// <param name="faintDurationSeconds">Faint duration seconds.</param>
        /// <param name="playerMessage">A string containing the bad news for the player.</param>
        public FaintPenalty(string resourceName, int cyclesBeforeFainting, float faintDurationSeconds, string playerMessage)
        {
            this.resourceName = resourceName;
            this.cyclesBeforeFainting = cyclesBeforeFainting;
            this.faintDurationSeconds = faintDurationSeconds;
            this.playerMessage = playerMessage;
        }
        #endregion

        #region Overrides
        public override bool IsEnabled()
        {
            return SnacksProperties.FaintWhenHungry;
        }

        public override void ApplyOutcome(Vessel vessel, SnacksProcessorResult result)
        {
            ProtoCrewMember[] astronauts;
            AstronautData astronautData;

            //Get vessel crew
            if (vessel.loaded)
                astronauts = vessel.GetVesselCrew().ToArray();
            else
                astronauts = vessel.protoVessel.GetVesselCrew().ToArray();

            //Clear out exempt crew
            List<ProtoCrewMember> nonExemptCrew = new List<ProtoCrewMember>();
            for (int index = 0; index < astronauts.Length; index++)
            {
                astronautData = SnacksScenario.Instance.GetAstronautData(astronauts[index]);
                if (astronautData.isExempt)
                    continue;
                nonExemptCrew.Add(astronauts[index]);
            }
            if (nonExemptCrew.Count == 0)
            {
                base.ApplyOutcome(vessel, result);
                return;
            }
            astronauts = nonExemptCrew.ToArray();

            //Select random crew if needed
            if (selectRandomCrew)
            {
                int randomIndex = UnityEngine.Random.Range(0, astronauts.Length - 1);
                astronauts = new ProtoCrewMember[] { astronauts[randomIndex] };
            }

            for (int index = 0; index < astronauts.Length; index++)
            {
                astronautData = SnacksScenario.Instance.GetAstronautData(astronauts[index]);
                if (astronautData.isExempt)
                    continue;
                applyOutcome(vessel, astronauts[index], astronautData);
            }

            //Call the base class
            base.ApplyOutcome(vessel, result);
        }

        public override void RemoveOutcome(Vessel vessel)
        {
            ProtoCrewMember[] astronauts;

            if (vessel.loaded)
                astronauts = vessel.GetVesselCrew().ToArray();
            else
                astronauts = vessel.protoVessel.GetVesselCrew().ToArray();

            for (int index = 0; index < astronauts.Length; index++)
                astronauts[index].inactive = false;

            //Call base class
            base.RemoveOutcome(vessel);
        }
        #endregion

        #region Helpers
        protected void applyOutcome(Vessel vessel, ProtoCrewMember astronaut, AstronautData astronautData)
        {
            if (string.IsNullOrEmpty(resourceName))
                causeFainting(vessel, astronaut, astronautData);

            else if (astronautData.processedResourceFailures.ContainsKey(resourceName) && astronautData.processedResourceFailures[resourceName] >= cyclesBeforeFainting)
                causeFainting(vessel, astronaut, astronautData);
        }

        protected void causeFainting(Vessel vessel, ProtoCrewMember astronaut, AstronautData astronautData)
        {
            string message;

            //Apply fainting immediately if the vessel is loaded
            if (vessel.loaded)
            {
                astronaut.SetInactive(faintDurationSeconds, true);
                if (!string.IsNullOrEmpty(playerMessage))
                    ScreenMessages.PostScreenMessage(astronaut.name + " " + playerMessage, 5.0f, ScreenMessageStyle.UPPER_LEFT);
            }

            //Vessel isn't loaded, record the info we need so we can make kerbals faint when vessel is loaded.
            else
            {
                if (!astronautData.keyValuePairs.ContainsKey(FaintDurationKey))
                {
                    astronautData.keyValuePairs.Add(FaintDurationKey, faintDurationSeconds.ToString());
                }
                else
                {
                    //Add to existing duration
                    float currentDuration = 0;
                    float.TryParse(astronautData.keyValuePairs[FaintDurationKey], out currentDuration);
                    currentDuration += faintDurationSeconds;
                    astronautData.keyValuePairs[FaintDurationKey] = currentDuration.ToString();
                }

                if (!astronautData.keyValuePairs.ContainsKey(FaintMessageKey))
                {
                    astronautData.keyValuePairs.Add(FaintMessageKey, astronautData.name + " " + playerMessage);
                }
                else
                {
                    //Add to existing message
                    message = astronautData.name + " " + playerMessage;
                    if (!astronautData.keyValuePairs[FaintMessageKey].Contains(message))
                        astronautData.keyValuePairs[FaintMessageKey] += (";" + message);

                    //Inform player
                    string[] messages = astronautData.keyValuePairs[FaintMessageKey].Split(';');
                    for (int messageIndex = 0; messageIndex < messages.Length; messageIndex++)
                        ScreenMessages.PostScreenMessage(messages[messageIndex], 5.0f, ScreenMessageStyle.UPPER_LEFT);
                }
            }
        }
        #endregion

        #region API
        public static void CheckFaintKerbals(Vessel vessel)
        {
            if (!SnacksProperties.FaintWhenHungry)
                return;
            if (vessel.loaded == false)
                return;

            float faintDuration = 0;
            string faintMessage = string.Empty;
            ProtoCrewMember[] astronauts;
            AstronautData astronautData;

            astronauts = vessel.GetVesselCrew().ToArray();

            //If crew member has gone hungry too many times then it's time
            //for the crew member to pass out.
            for (int index = 0; index < astronauts.Length; index++)
            {
                astronautData = SnacksScenario.Instance.GetAstronautData(astronauts[index]);

                //Get faint duration
                if (astronautData.keyValuePairs.ContainsKey(FaintDurationKey))
                {
                    float.TryParse(astronautData.keyValuePairs[FaintDurationKey], out faintDuration);

                    //Make kerbal faint
                    astronauts[index].SetInactive(faintDuration, true);

                    //Clear duration
                    astronautData.keyValuePairs.Remove(FaintDurationKey);
                }

                //Get faint message
                if (astronautData.keyValuePairs.ContainsKey(FaintMessageKey))
                {
                    faintMessage = astronautData.keyValuePairs[FaintMessageKey];

                    //Inform player
                    string[] messages = faintMessage.Split(';');
                    for (int messageIndex = 0; messageIndex < messages.Length; messageIndex++)
                        ScreenMessages.PostScreenMessage(messages[messageIndex], 5.0f, ScreenMessageStyle.UPPER_LEFT);

                    //Clear message
                    astronautData.keyValuePairs.Remove(FaintMessageKey);
                }
            }
        }
        #endregion
    }
}
