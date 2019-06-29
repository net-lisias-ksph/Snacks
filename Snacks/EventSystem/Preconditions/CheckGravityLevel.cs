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

namespace Snacks
{
    /// <summary>
    /// This precondition checks to see if a vessel or roster resource meets the supplied parameters. Gravity checks can be negated by setting CheckGravityLevel.checkType, where checkType is one
    /// of the conditional qualifiers. For instance, CheckGravityLevel.checkLesserOrEqual will disqualify any microgravity event checks and is useful for centrifuges.
    /// Example definition:
    /// PRECONDITION 
    /// {
    ///     name  = CheckGravityLevel
    ///     valueToCheck = 0.1
    ///     checkType = checkLesserOrEqual //Default value
    /// }
    /// </summary> 
    public class CheckGravityLevel : BasePrecondition
    {
        #region Constants
        public const string CheckGravityConditionName = "CheckGravityLevel";
        #endregion

        #region Housekeeping
        /// <summary>
        /// The value to check for
        /// </summary>
        public double valueToCheck;

        /// <summary>
        /// The conditional type to use during the validation.
        /// </summary>
        public CheckValueConditionals checkType = CheckValueConditionals.checkLesserOrEqual;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Snacks.CheckGravityLevel"/> class.
        /// </summary>
        /// <param name="node">A ConfigNode containing initialization parameters. parameters from the 
        /// <see cref="T:Snacks.BasePrecondition"/> class also apply.</param>
        public CheckGravityLevel(ConfigNode node): base(node)
        {
            if (node.HasValue(CheckValue))
                double.TryParse(node.GetValue(CheckValue), out valueToCheck);

            if (node.HasValue(CheckTypeValue))
                checkType = (CheckValueConditionals)Enum.Parse(typeof(CheckValueConditionals), node.GetValue(CheckTypeValue));
        }
        #endregion

        #region Overrides
        public override bool IsValid(ProtoCrewMember astronaut, Vessel vessel)
        {
            //Check precondition disqualifier. Some parts like centrifuges
            //disqualify the low-gee precondition check
            AstronautData astronautData = SnacksScenario.Instance.GetAstronautData(astronaut);
            if (astronautData == null)
                return true;
            string preconditionName = CheckGravityConditionName + "." + checkType.ToString();
            if (astronautData.disqualifiedPreconditions.Contains(preconditionName))
                return false;

            //Get gee force
            double vesselGeeForce;
            if (vessel.loaded)
            {
                vesselGeeForce = vessel.graviticAcceleration.magnitude;
            }
            else
            {
                if (vessel.protoVessel.vesselModules.HasNode("SnacksVesselModule"))
                {
                    ConfigNode node = vessel.protoVessel.vesselModules.GetNode("SnacksVesselModule");
                    if (node.HasValue(SnacksVesselModule.ValueGeeForce))
                        double.TryParse(node.GetValue(SnacksVesselModule.ValueGeeForce), out vesselGeeForce);
                    else
                        return false;
                }
                else
                {
                    return false;
                }
            }
            //Adjust for microgravity
            if (vessel.situation == Vessel.Situations.ESCAPING || vessel.situation == Vessel.Situations.ORBITING || vessel.situation == Vessel.Situations.SUB_ORBITAL)
                vesselGeeForce = 0;

            //Now make the comparison
            switch (checkType)
            {
                case CheckValueConditionals.checkEquals:
                    return vesselGeeForce.Equals(valueToCheck);

                case CheckValueConditionals.checkNotEqual:
                    return !vesselGeeForce.Equals(valueToCheck);

                case CheckValueConditionals.checkGreaterOrEqual:
                    return vesselGeeForce >= valueToCheck;

                case CheckValueConditionals.checkGreaterThan:
                    return vesselGeeForce > valueToCheck;

                case CheckValueConditionals.checkLesserOrEqual:
                    return vesselGeeForce <= valueToCheck;

                case CheckValueConditionals.checkLessThan:
                    return vesselGeeForce < valueToCheck;
            }

            return false;
        }

        public override bool IsValid(ProtoCrewMember astronaut)
        {
            //Find the astronaut's vessel
            if (astronaut.rosterStatus != ProtoCrewMember.RosterStatus.Assigned)
                return false;

            Vessel vessel = SnacksScenario.Instance.FindVessel(astronaut);
            if (vessel == null)
                return false;

            return IsValid(astronaut, vessel);
        }
        #endregion
    }
}
