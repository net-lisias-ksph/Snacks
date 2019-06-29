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
    #region Enums
    /// <summary>
    /// This enum represents the key-value conditionals to check.
    /// </summary>
    public enum CheckValueConditionals
    {
        /// <summary>
        /// Key-value must be equal to the supplied value.
        /// </summary>
        checkEquals,

        /// <summary>
        /// Key-value must not be equal to the supplied value.
        /// </summary>
        checkNotEqual,

        /// <summary>
        /// Key-value must be greater than the supplied value.
        /// </summary>
        checkGreaterThan,

        /// <summary>
        /// Key-value must be less than the supplied value.
        /// </summary>
        checkLessThan,

        /// <summary>
        /// Key-value must be greater than or equal to the supplied value.
        /// </summary>
        checkGreaterOrEqual,

        /// <summary>
        /// Key-value must be less than or equal to the supplied value.
        /// </summary>
        checkLesserOrEqual
    }
    #endregion

    /// <summary>
    /// This precondition Checks a kerbal's key-value and validates it against the supplied parameters.
    /// Example definition:
    /// PRECONDITION 
    /// {
    ///     name  = CheckKeyValue
    ///     keyValueName = State
    ///     checkType = checkEquals
    ///     stringValue = Bored
    /// }
    /// </summary> 
    public class CheckKeyValue: BasePrecondition
    {
        #region Constants
        public const string NameKeyValue = "keyValueName";
        public const string KeyValueStringValue = "stringValue";
        public const string KeyValueIntValue = "intValue";
        #endregion

        #region Housekeeping
        /// <summary>
        /// Name of the key-value
        /// </summary>
        public string keyValueName = string.Empty;

        /// <summary>
        /// String value of the key. Takes precedence over the int values.
        /// </summary>
        public string stringValue = string.Empty;

        /// <summary>
        /// Integer value of the key
        /// </summary>
        public int intValue;

        /// <summary>
        /// Type of check to make
        /// </summary>
        public CheckValueConditionals checkType;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Snacks.CheckKeyValue"/> class.
        /// </summary>
        /// <param name="node">A ConfigNode containing initialization parameters. parameters from the 
        /// <see cref="T:Snacks.BasePrecondition"/> class also apply.</param>
        public CheckKeyValue(ConfigNode node) : base(node)
        {
            if (node.HasValue(NameKeyValue))
                keyValueName = node.GetValue(NameKeyValue);

            if (node.HasValue(KeyValueStringValue))
                stringValue = node.GetValue(KeyValueStringValue);

            if (node.HasValue(KeyValueIntValue))
                int.TryParse(node.GetValue(KeyValueIntValue), out intValue);

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
            if (string.IsNullOrEmpty(keyValueName))
                return false;

            AstronautData astronautData = SnacksScenario.Instance.GetAstronautData(astronaut);
            if (astronautData == null)
                return false;

            if (!astronautData.keyValuePairs.ContainsKey(keyValueName))
                return false;

            int value = 0;
            bool valueParsed = int.TryParse(astronautData.keyValuePairs[keyValueName], out value);

            switch (checkType)
            {
                case CheckValueConditionals.checkEquals:
                    if (!string.IsNullOrEmpty(stringValue))
                    {
                        return astronautData.keyValuePairs[keyValueName] == stringValue;
                    }
                    else if (valueParsed)
                    {
                        return value == intValue;
                    }
                    else
                    {
                        return false;
                    }

                case CheckValueConditionals.checkNotEqual:
                    if (!string.IsNullOrEmpty(stringValue))
                    {
                        return astronautData.keyValuePairs[keyValueName] != stringValue;
                    }
                    else if (valueParsed)
                    {
                        return value != intValue;
                    }
                    else
                    {
                        return false;
                    }

                case CheckValueConditionals.checkGreaterOrEqual:
                    if (valueParsed)
                        return value >= intValue;
                    else
                        return false;

                case CheckValueConditionals.checkGreaterThan:
                    if (valueParsed)
                        return value > intValue;
                    else
                        return false;

                case CheckValueConditionals.checkLesserOrEqual:
                    if (valueParsed)
                        return value <= intValue;
                    else
                        return false;

                case CheckValueConditionals.checkLessThan:
                    if (valueParsed)
                        return value < intValue;
                    else
                        return false;
            }

            return false;
        }
        #endregion
    }
    }
