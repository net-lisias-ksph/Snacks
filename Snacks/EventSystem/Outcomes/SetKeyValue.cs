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
    /// This outcome sets the desired key-vale on the affected kerbals
    /// Example definition:
    /// OUTCOME 
    /// {
    ///     name  = SetKeyValue
    ///     keyValueName = DaysBored
    ///     intValue = 1
    /// }
    /// </summary>  
    public class SetKeyValue: BaseOutcome
    {
        #region Constants
        public const string NameKeyValue = "keyValueName";
        public const string KeyValueStringValue = "stringValue";
        public const string KeyValueIntValue = "intValue";
        public const string KeyValueAddIntValue = "addIntValue";
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
        /// Integer value to add to the existing key value. If key doesn't exist then it will be set to this value instead. Taks precedence over intValue.
        /// </summary>
        public int addIntValue;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Snacks.SetKeyValue"/> class.
        /// </summary>
        /// <param name="node">A ConfigNode containing initialization parameters. Parameters in the
        /// <see cref="T:Snacks.BaseOutcome"/> class also apply.</param>
        public SetKeyValue(ConfigNode node): base(node)
        {
            if (node.HasValue(NameKeyValue))
                keyValueName = node.GetValue(NameKeyValue);

            if (node.HasValue(KeyValueStringValue))
                stringValue = node.GetValue(KeyValueStringValue);

            if (node.HasValue(KeyValueIntValue))
                int.TryParse(node.GetValue(KeyValueIntValue), out intValue);

            if (node.HasValue(KeyValueAddIntValue))
                int.TryParse(node.GetValue(KeyValueAddIntValue), out addIntValue);
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
            if (astronauts.Length == 0)
            {
                //Call the base class
                base.ApplyOutcome(vessel, result);

                return;
            }

            //Get valid astronauts
            List<ProtoCrewMember> validAstronauts = new List<ProtoCrewMember>();
            for (int index = 0; index < astronauts.Length; index++)
            {
                if (astronauts[index].type == ProtoCrewMember.KerbalType.Unowned)
                    continue;
                validAstronauts.Add(astronauts[index]);
            }
            if (validAstronauts.Count == 0)
            {
                //Call the base class
                base.ApplyOutcome(vessel, result);

                return;
            }
            else
            {
                astronauts = validAstronauts.ToArray();
            }

            //Select random crew if needed
            if (selectRandomCrew)
            {
                int randomIndex = UnityEngine.Random.Range(0, astronauts.Length - 1);
                astronauts = new ProtoCrewMember[] { astronauts[randomIndex] };
            }

            //Now set the key-value
            for (int index = 0; index < astronauts.Length; index++)
            {
                if (astronauts[index].type == ProtoCrewMember.KerbalType.Unowned)
                    continue;
                astronautData = SnacksScenario.Instance.GetAstronautData(astronauts[index]);
                if (astronautData == null)
                    continue;
                setKeyValue(astronautData);
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

        #region Helpers
        protected void setKeyValue(AstronautData astronautData)
        {
            int value = 0;

            //Strings before ints
            if (!string.IsNullOrEmpty(stringValue))
            {
                if (!astronautData.keyValuePairs.ContainsKey(keyValueName))
                    astronautData.keyValuePairs.Add(keyValueName, stringValue);
                else
                    astronautData.keyValuePairs[keyValueName] = stringValue;
            }

            //Add ints before ints
            else if (addIntValue != 0)
            {
                if (!astronautData.keyValuePairs.ContainsKey(keyValueName))
                {
                    astronautData.keyValuePairs.Add(keyValueName, addIntValue.ToString());
                }
                else
                {
                    if (int.TryParse(astronautData.keyValuePairs[keyValueName], out value))
                    {
                        value += addIntValue;
                        astronautData.keyValuePairs[keyValueName] = value.ToString();
                    }
                }
            }

            else
            {
                if (!astronautData.keyValuePairs.ContainsKey(keyValueName))
                    astronautData.keyValuePairs.Add(keyValueName, intValue.ToString());
                else
                    astronautData.keyValuePairs[keyValueName] = intValue.ToString();
            }

            SnacksScenario.Instance.SetAstronautData(astronautData);

            //Player message
            if (!string.IsNullOrEmpty(playerMessage))
            {
                string message = playerMessage;
            }
        }
        #endregion
    }
}
