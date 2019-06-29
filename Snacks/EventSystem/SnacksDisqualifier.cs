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
using System.Collections;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine;
using KSP.IO;

namespace Snacks
{
    /// <summary>
    /// This part module is designed to negate one or more preconditions so long as the kerbal resides in the part.
    /// An example would be a centrifuge
    /// </summary>
    public class SnacksDisqualifier: PartModule
    {
        #region Fields
        /// <summary>
        /// Contains the disqualified preconditions such as CheckGravityLevel.checkLesserOrEqual for low gravity checks. Separate disqualified preconditions by semicolon.
        /// Most of the preconditions can be disqualified simply by stating their name. If a precondition requires something different, be sure to check its documentation.
        /// </summary>
        [KSPField(isPersistant = true)]
        public string disqualifiedPreconditions = string.Empty;
        #endregion

        #region Overrides
        private void OnDestroy()
        {
            GameEvents.onCrewBoardVessel.Remove(onCrewBoardVessel);
            GameEvents.onCrewOnEva.Remove(onCrewOnEva);
            GameEvents.onCrewTransferred.Remove(onCrewTransferred);
            GameEvents.OnVesselRecoveryRequested.Remove(OnVesselRecoveryRequested);
        }

        public override void OnStart(StartState state)
        {
            base.OnStart(state);
            GameEvents.onCrewOnEva.Add(onCrewOnEva);
            GameEvents.onCrewBoardVessel.Add(onCrewBoardVessel);
            GameEvents.onCrewTransferred.Add(onCrewTransferred);
            GameEvents.OnVesselRecoveryRequested.Add(OnVesselRecoveryRequested);

            //Set disqualifier
            ProtoCrewMember[] astronauts = this.part.protoModuleCrew.ToArray();
            for (int index = 0; index < astronauts.Length; index++)
            {
                AstronautData astronautData = SnacksScenario.Instance.GetAstronautData(astronauts[index]);
                astronautData.SetDisqualifier(disqualifiedPreconditions);
            }
        }
        #endregion

        #region Game Event Handlers
        private void onCrewOnEva(GameEvents.FromToAction<Part, Part> data)
        {
            Part evaKerbal = data.to;
            Part partExited = data.from;

            if (!string.IsNullOrEmpty(disqualifiedPreconditions) && partExited == this.part)
            {
                ProtoCrewMember astronaut = evaKerbal.vessel.GetVesselCrew()[0];
                AstronautData astronautData = SnacksScenario.Instance.GetAstronautData(astronaut);

                //Remove disqualifier
                astronautData.ClearDisqualifier(disqualifiedPreconditions);
            }
        }

        private void onCrewBoardVessel(GameEvents.FromToAction<Part, Part> data)
        {
            Part evaKerbal = data.from;
            Part boardedPart = data.to;

            if (!string.IsNullOrEmpty(disqualifiedPreconditions) && boardedPart == this.part)
            {
                ProtoCrewMember astronaut = evaKerbal.vessel.GetVesselCrew()[0];
                AstronautData astronautData = SnacksScenario.Instance.GetAstronautData(astronaut);

                //Set disqualifier
                astronautData.SetDisqualifier(disqualifiedPreconditions);
            }
        }

        private void onCrewTransferred(GameEvents.HostedFromToAction<ProtoCrewMember, Part> data)
        {
            ProtoCrewMember astronaut = data.host;
            Part fromPart = data.from;
            Part toPart = data.to;

            if (!string.IsNullOrEmpty(disqualifiedPreconditions) && toPart == this.part)
            {
                AstronautData astronautData = SnacksScenario.Instance.GetAstronautData(astronaut);

                //Set disqualifier
                astronautData.SetDisqualifier(disqualifiedPreconditions);
            }
        }

        private void OnVesselRecoveryRequested(Vessel vessel)
        {
            if (string.IsNullOrEmpty(disqualifiedPreconditions))
                return;

            ProtoCrewMember[] astronauts = this.part.protoModuleCrew.ToArray();
            AstronautData astronautData;

            for (int index = 0; index < astronauts.Length; index++)
            {
                astronautData = SnacksScenario.Instance.GetAstronautData(astronauts[index]);
                astronautData.ClearDisqualifier(disqualifiedPreconditions);
            }
        }
        #endregion

    }
}
