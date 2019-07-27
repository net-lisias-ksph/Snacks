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
    /// This precondition checks to see if a vessel or roster resource meets the supplied parameters.
    /// Example definition:
    /// PRECONDITION 
    /// {
    ///     name  = CheckResource
    ///     resourceName = Stress
    ///     checkType = checkEquals
    ///     valueToCheck = 3.0
    /// }
    /// </summary> 
    public class CheckResource: BasePrecondition
    {
        #region Constants
        public const string CheckConditionalMaxAmount = "checkMaxAmount";
        #endregion

        #region Housekeeping
        /// <summary>
        /// Name of the resource to check
        /// </summary>
        public string resourceName = string.Empty;

        /// <summary>
        /// The conditional type to use during the validation.
        /// </summary>
        public CheckValueConditionals checkType;

        /// <summary>
        /// The value to check for
        /// </summary>
        public double valueToCheck;

        /// <summary>
        /// Flag to indicate whether or not to check the resource's max amount instead of the curren amount;
        /// </summary>
        public bool checkMaxAmount;

        /// <summary>
        /// Flag to indicate whether or not to check the resource levels as a percentage.
        /// </summary>
        public bool checkAsPercentage;

        protected bool isRosterResource;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Snacks.CheckResource"/> class.
        /// </summary>
        /// <param name="node">A ConfigNode containing initialization parameters. parameters from the 
        /// <see cref="T:Snacks.BasePrecondition"/> class also apply.</param>
        public CheckResource(ConfigNode node): base(node)
        {
            if (node.HasValue(ValueResourceName))
                resourceName = node.GetValue(ValueResourceName);

            if (node.HasValue(CheckTypeValue))
                checkType = (CheckValueConditionals)Enum.Parse(typeof(CheckValueConditionals), node.GetValue(CheckTypeValue));

            if (node.HasValue(CheckValue))
                double.TryParse(node.GetValue(CheckValue), out valueToCheck);

            if (node.HasValue(CheckConditionalMaxAmount))
                bool.TryParse(node.GetValue(CheckConditionalMaxAmount), out checkMaxAmount);

            //Now determine if the resource is a roster resource
            PartResourceDefinitionList definitions = PartResourceLibrary.Instance.resourceDefinitions;
            if (definitions.Contains(resourceName))
                isRosterResource = false;
            else
                isRosterResource = true;

        }
        #endregion

        #region Overrides
        public override bool IsValid(ProtoCrewMember astronaut, Vessel vessel)
        {
            if (!base.IsValid(astronaut))
                return false;
            if (string.IsNullOrEmpty(resourceName))
                return false;

            double amount = 0;
            double maxAmount = 0;
            double percentage = 0;
            //Get vessel resource
            if (!isRosterResource)
            {
                List<ProtoPartResourceSnapshot> protoPartResources = new List<ProtoPartResourceSnapshot>();
                ProcessedResource.GetResourceTotals(vessel, resourceName, out amount, out maxAmount, protoPartResources);
                percentage = amount / maxAmount;
            }

            //Get roster resource
            else
            {
                AstronautData astronautData = SnacksScenario.Instance.GetAstronautData(astronaut);
                if (astronautData == null)
                    return false;
                if (!astronautData.rosterResources.ContainsKey(resourceName))
                    return false;

                SnacksRosterResource rosterResource = astronautData.rosterResources[resourceName];
                amount = rosterResource.amount;
                maxAmount = rosterResource.maxAmount;
                percentage = amount / maxAmount;
            }

            //Now perform the check
            if (checkMaxAmount)
                amount = maxAmount;
            else if (checkAsPercentage)
                amount = percentage;
            switch (checkType)
            {
                case CheckValueConditionals.checkEquals:
                    return amount.Equals(valueToCheck);

                case CheckValueConditionals.checkGreaterOrEqual:
                    return amount.Equals(valueToCheck) || amount > valueToCheck;

                case CheckValueConditionals.checkGreaterThan:
                    return amount > valueToCheck;

                case CheckValueConditionals.checkLesserOrEqual:
                    return amount.Equals(valueToCheck) || amount < valueToCheck;

                case CheckValueConditionals.checkLessThan:
                    return amount < valueToCheck;
            }

            return false;
        }

        public override bool IsValid(ProtoCrewMember astronaut)
        {
            if (!base.IsValid(astronaut))
                return false;
            if (string.IsNullOrEmpty(resourceName))
                return false;

            //Get roster resource
            double amount = 0;
            double maxAmount = 0;
            double percentage = 0;
            if (isRosterResource)
            {
                AstronautData astronautData = SnacksScenario.Instance.GetAstronautData(astronaut);
                if (astronautData == null)
                    return false;
                if (!astronautData.rosterResources.ContainsKey(resourceName))
                    return false;

                SnacksRosterResource rosterResource = astronautData.rosterResources[resourceName];
                amount = rosterResource.amount;
                maxAmount = rosterResource.maxAmount;
                percentage = amount / maxAmount;
            }

            //Try to get vessel resource
            else
            {
                if (astronaut.KerbalRef == null || astronaut.KerbalRef.InVessel == null)
                    return false;

                List<ProtoPartResourceSnapshot> protoPartResources = new List<ProtoPartResourceSnapshot>();
                ProcessedResource.GetResourceTotals(astronaut.KerbalRef.InVessel, resourceName, out amount, out maxAmount, protoPartResources);
                percentage = amount / maxAmount;
            }

            //Now perform the check
            if (checkMaxAmount)
                amount = maxAmount;
            else if (checkAsPercentage)
                amount = percentage;
            switch (checkType)
            {
                case CheckValueConditionals.checkEquals:
                    return amount.Equals(valueToCheck);

                case CheckValueConditionals.checkGreaterOrEqual:
                    return amount.Equals(valueToCheck) || amount > valueToCheck;

                case CheckValueConditionals.checkGreaterThan:
                    return amount > valueToCheck;

                case CheckValueConditionals.checkLesserOrEqual:
                    return amount.Equals(valueToCheck) || amount < valueToCheck;

                case CheckValueConditionals.checkLessThan:
                    return amount < valueToCheck;
            }

            return false;
        }
        #endregion
    }
}
