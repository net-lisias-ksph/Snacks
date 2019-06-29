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
    /// This outcome causes affected kerbals to die.
    /// Example definition:
    /// OUTCOME 
    /// {
    ///     name  = DeathPenalty
    ///     resourceName = Snacks
    ///     cyclesBeforeDeath = 10
    /// }
    /// </summary>   
    public class DeathPenalty : BaseOutcome
    {
        #region Constants
        const string ValueCyclesBeforeDeath = "cyclesBeforeDeath";
        #endregion

        #region Housekeeping
        /// <summary>
        /// The name of the resource to check for failed processor cycles.
        /// </summary>
        public string resourceName = string.Empty;

        /// <summary>
        /// The number of cycles that must fail before the kerbal dies.
        /// </summary>
        public int cyclesBeforeDeath = 0;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Snacks.DeathPenalty"/> class.
        /// </summary>
        /// <param name="node">A ConfigNode containing initialization parameters. Parameters in the
        /// <see cref="T:Snacks.BaseOutcome"/> class also apply.</param>
        public DeathPenalty(ConfigNode node) : base (node)
        {
            if (node.HasValue(ResourceName))
                resourceName = node.GetValue(ResourceName);

            if (node.HasValue(ValueCyclesBeforeDeath))
                int.TryParse(node.GetValue(ValueCyclesBeforeDeath), out cyclesBeforeDeath);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Snacks.DeathPenalty"/> class.
        /// </summary>
        /// <param name="resourceName">The name of the resource to check. If no processor has a failed cycle with the resource
        /// then the outcome is invalidated.</param>
        /// <param name="cyclesBeforeDeath">The number of failed processor cycles required before applying the outcome.</param>
        /// <param name="playerMessage">A string containing the bad news for the player.</param>
        public DeathPenalty(string resourceName, int cyclesBeforeDeath, string playerMessage)
        {
            this.resourceName = resourceName;
            this.cyclesBeforeDeath = cyclesBeforeDeath;
            this.playerMessage = playerMessage;
        }
        #endregion

        #region Overrides

        public override void ApplyOutcome(Vessel vessel, SnacksProcessorResult result)
        {
            ProtoCrewMember[] astronauts;
            AstronautData astronautData;

            //Get the astronauts
            if (vessel.loaded)
                astronauts = vessel.GetVesselCrew().ToArray();
            else
                astronauts = vessel.protoVessel.GetVesselCrew().ToArray();

            //If crew member has failed too many cycles then it's time to die.
            List<ProtoCrewMember> doomed = new List<ProtoCrewMember>();
            KerbalRoster roster = HighLogic.CurrentGame.CrewRoster;
            for (int index = 0; index < astronauts.Length; index++)
            {
                astronautData = SnacksScenario.Instance.GetAstronautData(astronauts[index]);

                //Handle exemptions
                if (astronautData.isExempt)
                    continue;

                //Add to our cleanup list
                if (astronautData.processedResourceFailures.ContainsKey(resourceName) && astronautData.processedResourceFailures[resourceName] >= cyclesBeforeDeath)
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
                message = astronauts[index].name + " " + playerMessage;
                Debug.Log("[DeathPenalty] - " + message);
                ScreenMessages.PostScreenMessage(message, 5.0f, ScreenMessageStyle.UPPER_CENTER);
            }

            //Call the base class
            base.ApplyOutcome(vessel, result);
        }

        public static void CheckDeadKerbals(Vessel vessel)
        {
        }

        public override bool IsEnabled()
        {
            return SnacksProperties.CanStarveToDeath;
        }
        #endregion
    }
}
