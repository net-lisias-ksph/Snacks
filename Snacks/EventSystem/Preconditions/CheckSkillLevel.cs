using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Snacks
{
    /// <summary>
    /// This precondition checks to see if a kerbal's experience level matches the desired value and type of check to make. For instance you could check to see if a kerbal is above 3 stars.
    /// Example definition:
    /// PRECONDITION 
    /// {
    ///     name  = CheckSkillLevel
    ///     valueToCheck = 3
    ///     checkType = checkGreaterOrEqual //Default value
    /// }
    /// </summary> 
    public class CheckSkillLevel: BasePrecondition
    {
        #region Housekeeping
        /// <summary>
        /// The value to check for
        /// </summary>
        public int valueToCheck;

        /// <summary>
        /// Type of check to make
        /// Default: checkGreaterOrEqual
        /// </summary>
        public CheckValueConditionals checkType = CheckValueConditionals.checkGreaterOrEqual;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Snacks.CheckSkillLevel"/> class.
        /// </summary>
        /// <param name="node">A ConfigNode containing initialization parameters. parameters from the 
        /// <see cref="T:Snacks.BasePrecondition"/> class also apply.</param>
        public CheckSkillLevel(ConfigNode node) : base(node)
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
                    return astronaut.experienceTrait.CrewMemberExperienceLevel() == valueToCheck;

                case CheckValueConditionals.checkGreaterOrEqual:
                    return astronaut.experienceTrait.CrewMemberExperienceLevel() >= valueToCheck;

                case CheckValueConditionals.checkGreaterThan:
                    return astronaut.experienceTrait.CrewMemberExperienceLevel() > valueToCheck;

                case CheckValueConditionals.checkLesserOrEqual:
                    return astronaut.experienceTrait.CrewMemberExperienceLevel() <= valueToCheck;

                case CheckValueConditionals.checkLessThan:
                    return astronaut.experienceTrait.CrewMemberExperienceLevel() < valueToCheck;

                case CheckValueConditionals.checkNotEqual:
                    return astronaut.experienceTrait.CrewMemberExperienceLevel() != valueToCheck;
            }
        }
        #endregion
    }
}
