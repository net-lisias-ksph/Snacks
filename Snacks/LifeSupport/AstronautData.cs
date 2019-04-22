/**
The MIT License (MIT)
Copyright (c) 2014-2019 by Michael Billard
Original concept by Troy Gruetzmacher

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
using System.Collections;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine;
using KSP.IO;

namespace Snacks
{
    public class AstronautData
    {
        public string name;
        public string experienceTrait;
        public double lastUpdated;
        public int mealsMissed;
        public bool isExempt;
        public DictionaryValueList<string, string> keyValuePairs;

        protected static void Log(string message)
        {
            if (SnacksScenario.LoggingEnabled)
            {
                Debug.Log("[AstronautData] - " + message);
            }
        }

        public static DictionaryValueList<string, AstronautData> Load(ConfigNode node)
        {
            DictionaryValueList<string, AstronautData> crewData = new DictionaryValueList<string, AstronautData>();
            ConfigNode[] astronauts = node.GetNodes("ASTRONAUT");

            foreach (ConfigNode astronaut in astronauts)
            {
                try
                {
                    AstronautData data = new AstronautData();

                    data.name = astronaut.GetValue("name");
                    data.experienceTrait = astronaut.GetValue("experienceTrait");
                    data.mealsMissed = int.Parse(astronaut.GetValue("mealsMissed"));
                    data.lastUpdated = double.Parse(astronaut.GetValue("lastUpdated"));
                    data.isExempt = bool.Parse(astronaut.GetValue("isExempt"));
                    data.keyValuePairs = new DictionaryValueList<string, string>();

                    ConfigNode[] keyValuePairs = astronaut.GetNodes("KEYVALUE");
                    foreach (ConfigNode keyValue in keyValuePairs)
                        data.keyValuePairs.Add(keyValue.GetValue("key"), keyValue.GetValue("value"));

                    crewData.Add(data.name, data);
                }
                catch (Exception ex)
                {
                    Log("error encountered: " + ex + " skipping kerbal.");
                    continue;
                }
            }

            return crewData;
        }

        public static void Save(DictionaryValueList<string, AstronautData> crewData, ConfigNode node)
        {
            List<AstronautData>.Enumerator dataValues = crewData.GetListEnumerator();
            List<string>.Enumerator keyValueEnumerator;
            string keyValueKey;
            AstronautData data;
            ConfigNode keyValueNode;
            ConfigNode configNode;

            while (dataValues.MoveNext())
            {
                data = dataValues.Current;
                configNode = new ConfigNode("ASTRONAUT");
                configNode.AddValue("name", data.name);
                configNode.AddValue("experienceTrait", data.experienceTrait);
                configNode.AddValue("mealsMissed", data.mealsMissed);
                configNode.AddValue("lastUpdated", data.lastUpdated);
                configNode.AddValue("isExempt", data.isExempt);

                keyValueEnumerator = data.keyValuePairs.GetListEnumerator();
                while (keyValueEnumerator.MoveNext())
                {
                    keyValueKey = keyValueEnumerator.Current;

                    keyValueNode = new ConfigNode("KEYVALUE");
                    keyValueNode.AddValue(keyValueKey, data.keyValuePairs[keyValueKey]);
                    configNode.AddNode(keyValueNode);
                }

                node.AddNode(configNode);
            }
        }
    }
}
