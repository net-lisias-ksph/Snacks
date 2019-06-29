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
    /// This precondition rolls a random die between a minimum and maximum value and compares it to a target number. If the roll meets or exceeds the target number then the precondition passes.
    /// Example definition:
    /// PRECONDITION 
    /// {
    ///     name  = CheckRandomChance
    ///     dieRollMin = 1
    ///     dieRollMax = 1000
    ///     targetNumber = 999
    /// }
    /// </summary> 
    public class CheckRandomChance: BasePrecondition
    {
        #region Constants
        public const string CheckMinDie = "dieRollMin";
        public const string CheckMaxDie = "dieRollMax";
        public const string CheckTarget = "targetNumber";
        #endregion

        #region Housekeeping
        /// <summary>
        /// Minimum value on the die roll
        /// </summary>
        public int dieRollMin;

        /// <summary>
        /// Maximum value on the die roll
        /// </summary>
        public int dieRollMax;

        /// <summary>
        /// Target number required to declare the precondition valid.
        /// </summary>
        public int targetNumber;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Snacks.CheckRandomChance"/> class.
        /// </summary>
        /// <param name="node">A ConfigNode containing initialization parameters. parameters from the 
        /// <see cref="T:Snacks.BasePrecondition"/> class also apply.</param>
        public CheckRandomChance(ConfigNode node): base(node)
        {
            if (node.HasValue(CheckMinDie))
                int.TryParse(node.GetValue(CheckMinDie), out dieRollMin);

            if (node.HasValue(CheckMaxDie))
                int.TryParse(node.GetValue(CheckMaxDie), out dieRollMax);

            if (node.HasValue(CheckTarget))
                int.TryParse(node.GetValue(CheckTarget), out targetNumber);
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

            int rollResult = UnityEngine.Random.Range(dieRollMin, dieRollMax);

            return rollResult >= targetNumber;
        }
        #endregion
    }
}
