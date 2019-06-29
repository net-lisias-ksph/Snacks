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
using System.Collections.Generic;

namespace Snacks
{
    /// <summary>
    /// This outcome sets the desired condition on the affected kerbals
    /// Example definition:
    /// OUTCOME 
    /// {
    ///     name  = SetCondition
    ///     conditionSummary = Sick
    /// }
    /// </summary>   
    public class SetCondition : BaseOutcome
    {
        #region Housekeeping
        /// <summary>
        /// Name of the condition to set
        /// </summary>
        public string conditionName = string.Empty;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Snacks.SetCondition"/> class.
        /// </summary>
        /// <param name="node">A ConfigNode containing initialization parameters. Parameters in the
        /// <see cref="T:Snacks.BaseOutcome"/> class also apply.</param>
        public SetCondition(ConfigNode node) : base (node)
        {
            if (node.HasValue(ConditionName))
                conditionName = node.GetValue(ConditionName);
        }
        #endregion

        #region Overrides
        public override void ApplyOutcome(Vessel vessel, SnacksProcessorResult result)
        {
            ProtoCrewMember[] astronauts;
            AstronautData astronautData;

            //Get affected astronauts
            if (result.afftectedAstronauts != null)
                astronauts = result.afftectedAstronauts.ToArray();
            else if (vessel.loaded)
                astronauts = vessel.GetVesselCrew().ToArray();
            else
                astronauts = vessel.protoVessel.GetVesselCrew().ToArray();

            //Get valid astronauts
            List<ProtoCrewMember> validAstronauts = new List<ProtoCrewMember>();
            for (int index = 0; index < astronauts.Length; index++)
            {
                if (astronauts[index].type == ProtoCrewMember.KerbalType.Unowned)
                    continue;
                validAstronauts.Add(astronauts[index]);
            }
            if (validAstronauts.Count == 0)
                return;
            else
                astronauts = validAstronauts.ToArray();

            //Select random crew if needed
            if (selectRandomCrew)
            {
                int randomIndex = UnityEngine.Random.Range(0, astronauts.Length - 1);
                astronauts = new ProtoCrewMember[] { astronauts[randomIndex] };
            }

            for (int index = 0; index < astronauts.Length; index++)
            {
                if (astronauts[index].type == ProtoCrewMember.KerbalType.Unowned)
                    continue;
                astronautData = SnacksScenario.Instance.GetAstronautData(astronauts[index]);
                if (astronautData == null)
                    continue;

                astronautData.SetCondition(conditionName);
                SnacksScenario.Instance.SetAstronautData(astronautData);

                SnacksScenario.Instance.RemoveSkillsIfNeeded(astronauts[index]);
            }

            //Inform player
            if (!string.IsNullOrEmpty(playerMessage))
            {
                string message = playerMessage;
                ScreenMessages.PostScreenMessage(message, 5, ScreenMessageStyle.UPPER_LEFT);
            }

            //Call the base class
            base.ApplyOutcome(vessel, result);
        }
        #endregion
    }
}
