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

namespace Snacks
{
    /// <summary>
    /// A precondition is a check of some type that prevents outcomes from being applied unless the precondition's check suceeds.
    /// </summary>
    public class BasePrecondition
    {
        #region Constants
        public const string PRECONDITION = "PRECONDITION";
        public const string PreconditionName = "name";
        public const string ValueExists = "mustExist";
        public const string CheckTypeValue = "checkType";
        public const string CheckValue = "valueToCheck";
        public const string ValueResourceName = "resourceName";
        #endregion

        #region Housekeeping
        /// <summary>
        /// Name of the precondition.
        /// </summary>
        public string name = string.Empty;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Snacks.BasePrecondition"/> class.
        /// </summary>
        /// <param name="node">A ConfigNode specifying the initialization parameters.</param>
        public BasePrecondition(ConfigNode node)
        {
            if (node.HasValue(PreconditionName))
                name = node.GetValue(PreconditionName);
        }
        #endregion

        #region API
        /// <summary>
        /// Determines if the precondition is valid.
        /// </summary>
        /// <param name="astronaut">The ProtoCrewModule to check.</param>
        /// <param name="vessel">The Vessel to check</param>
        /// <returns></returns>
        public virtual bool IsValid(ProtoCrewMember astronaut, Vessel vessel)
        {
            //Check the astronaut's disqualified conditions to see if the precondition is on the list. If so, then the precondition isn't valid.
            AstronautData astronautData = SnacksScenario.Instance.GetAstronautData(astronaut);
            if (astronautData == null)
                return true;
            if (astronautData.disqualifiedPreconditions.Contains(name))
                return false;

            return true;
        }

        /// <summary>
        /// Determines if the precondition is valid.
        /// </summary>
        /// <param name="astronaut">The ProtoCrewModule to check.</param>
        /// <returns></returns>
        public virtual bool IsValid(ProtoCrewMember astronaut)
        {
            //Check the astronaut's disqualified conditions to see if the precondition is on the list. If so, then the precondition isn't valid.
            AstronautData astronautData = SnacksScenario.Instance.GetAstronautData(astronaut);
            if (astronautData == null)
                return true;
            if (astronautData.disqualifiedPreconditions.Contains(name))
                return false;

            return true;
        }
        #endregion
    }
}
