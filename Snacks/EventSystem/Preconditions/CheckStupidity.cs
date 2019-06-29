using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Snacks
{
    /// <summary>
    /// This precondition checks to see if a kerbal's stupidity matches the desired value and type of check to make.
    /// Example definition:
    /// PRECONDITION 
    /// {
    ///     name  = CheckStupidity
    ///     valueToCheck = 0.5
    ///     checkType = checkEquals //Default value
    /// }
    /// </summary> 
    public class CheckStupidity: BasePrecondition
    {
        #region Housekeeping
        /// <summary>
        /// The value to check for
        /// </summary>
        public float valueToCheck;

        /// <summary>
        /// Type of check to make
        /// Default: checkEquals
        /// </summary>
        public CheckValueConditionals checkType = CheckValueConditionals.checkEquals;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Snacks.CheckStupidity"/> class.
        /// </summary>
        /// <param name="node">A ConfigNode containing initialization parameters. parameters from the 
        /// <see cref="T:Snacks.BasePrecondition"/> class also apply.</param>
        public CheckStupidity(ConfigNode node) : base(node)
        {
            if (node.HasValue(CheckValue))
                float.TryParse(node.GetValue(CheckValue), out valueToCheck);

            if (node.HasValue(CheckTypeValue))
                checkType = (CheckValueConditionals)Enum.Parse(typeof(CheckValueConditionals), node.GetValue(CheckTypeValue));
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

            switch (checkType)
            {
                default:
                case CheckValueConditionals.checkEquals:
                    return astronaut.stupidity.Equals(valueToCheck);

                case CheckValueConditionals.checkGreaterOrEqual:
                    return astronaut.stupidity.Equals(valueToCheck) || astronaut.stupidity > valueToCheck;

                case CheckValueConditionals.checkGreaterThan:
                    return astronaut.stupidity > valueToCheck;

                case CheckValueConditionals.checkLesserOrEqual:
                    return astronaut.stupidity.Equals(valueToCheck) || astronaut.stupidity < valueToCheck;

                case CheckValueConditionals.checkLessThan:
                    return astronaut.stupidity < valueToCheck;

                case CheckValueConditionals.checkNotEqual:
                    return !astronaut.stupidity.Equals(valueToCheck);
            }
        }
        #endregion
    }
}
