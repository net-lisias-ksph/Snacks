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

namespace Snacks
{
    /// <summary>
    /// This precondition checks to see if a vessel's crew count matches the desired parameter.
    /// Example definition:
    /// PRECONDITION 
    /// {
    ///     name  = CheckCrewCount
    ///     valueToCheck = 1
    ///     checkType = checkEquals //Default value
    /// }
    /// </summary> 
    public class CheckCrewCount: BasePrecondition
    {
        #region Housekeeping
        /// <summary>
        /// The value to check for
        /// </summary>
        public int valueToCheck;

        /// <summary>
        /// Type of check to make
        /// </summary>
        public CheckValueConditionals checkType;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Snacks.CheckCrewCount"/> class.
        /// </summary>
        /// <param name="node">A ConfigNode containing initialization parameters. parameters from the 
        /// <see cref="T:Snacks.BasePrecondition"/> class also apply.</param>
        public CheckCrewCount(ConfigNode node): base(node)
        {
            if (node.HasValue(CheckValue))
                int.TryParse(node.GetValue(CheckValue), out valueToCheck);

            if (node.HasValue(CheckTypeValue))
                checkType = (CheckValueConditionals)Enum.Parse(typeof(CheckValueConditionals), node.GetValue(CheckTypeValue));
        }
        #endregion

        #region Overrides
        public override bool IsValid(ProtoCrewMember astronaut, Vessel vessel)
        {
            if (!base.IsValid(astronaut, vessel))
                return false;

            int crewCount;
            if (vessel.loaded)
                crewCount = vessel.GetVesselCrew().Count;
            else
                crewCount = vessel.protoVessel.GetVesselCrew().Count;

            switch (checkType)
            {
                default:
                case CheckValueConditionals.checkEquals:
                    return crewCount == valueToCheck;

                case CheckValueConditionals.checkGreaterOrEqual:
                    return crewCount >= valueToCheck;

                case CheckValueConditionals.checkGreaterThan:
                    return crewCount > valueToCheck;

                case CheckValueConditionals.checkLesserOrEqual:
                    return crewCount <= valueToCheck;

                case CheckValueConditionals.checkLessThan:
                    return crewCount < valueToCheck;

                case CheckValueConditionals.checkNotEqual:
                    return crewCount != valueToCheck;
            }
        }

        public override bool IsValid(ProtoCrewMember astronaut)
        {
            Vessel vessel = SnacksScenario.Instance.FindVessel(astronaut);
            if (vessel == null)
                return false;

            return base.IsValid(astronaut, vessel);
        }
        #endregion
    }
}
