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
    /// This precondition Checks a kerbal's condition summary to see if it exists or not. The precondition is valid if the kerbal's condition summary matches the parameters of the precondition.
    /// Example definition:
    /// PRECONDITION 
    /// {
    ///     name  = CheckCondition
    ///     conditionSummary = Sleepy
    ///     mustExist = true
    /// }
    /// </summary> 
    public class CheckCondition: BasePrecondition
    {
        #region Constants
        public const string PreconditionSummary = "conditionSummary";
        #endregion

        #region Housekeeping
        /// <summary>
        /// Name of the condition to check
        /// </summary>
        public string conditionSummary = string.Empty;

        /// <summary>
        /// Flag to indicate pressence (true) or absence (false) of the value to check.
        /// </summary>
        public bool mustExist = true;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Snacks.CheckCondition"/> class.
        /// </summary>
        /// <param name="node">A ConfigNode containing initialization parameters. parameters from the 
        /// <see cref="T:Snacks.BasePrecondition"/> class also apply.</param>
        public CheckCondition(ConfigNode node) : base(node)
        {
            if (node.HasValue(PreconditionSummary))
                conditionSummary = node.GetValue(PreconditionSummary);

            if (node.HasValue(ValueExists))
                bool.TryParse(node.GetValue(ValueExists), out mustExist);
        }
        #endregion

        #region Overrides
        public override bool IsValid(ProtoCrewMember astronaut, Vessel vessel)
        {
            return IsValid(astronaut);
        }

        public override bool IsValid(ProtoCrewMember astronaut)
        {
            if (!base.IsValid(astronaut))
                return false;

            AstronautData astronautData = SnacksScenario.Instance.GetAstronautData(astronaut);
            if (astronautData == null)
                return false;

            if (string.IsNullOrEmpty(conditionSummary))
                return false;

            if (astronautData.conditionSummary.Contains(conditionSummary) && mustExist)
                return true;
            else if (!astronautData.conditionSummary.Contains(conditionSummary) && !mustExist)
                return true;

            return false;
        }
        #endregion
    }
}
