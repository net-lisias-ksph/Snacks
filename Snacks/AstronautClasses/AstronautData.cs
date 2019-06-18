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
using System.Linq;
using UnityEngine;

namespace Snacks
{
    public class AstronautData
    {
        #region Constants
        const string AstronautNode = "ASTRONAUT";
        const string AstronautNodeName = "name";
        const string AstronautNodeTrait = "experienceTrait";
        const string AstronautNodeUpdated = "lastUpdated";
        const string AstronautNodeExempt = "isExempt";
        const string AstronautNodeCondition = "conditionSummary";

        const string ResourceCounterNode = "RESOURCE_COUNT";
        const string ResourceCounterIsSuccess = "isSuccess";
        const string ResourceCounterName = "resourceName";
        const string ResourceCounterValue = "count";

        const string KeyValueNode = "KEYVALUE";
        const string KeyValuePairKey = "key";
        const string KeyValuePairValue = "value";
        #endregion

        #region Housekeeping
        public string name;
        public string experienceTrait;
        public double lastUpdated;
        public int mealsMissed;
        public bool isExempt;
        public string conditionSummary = string.Empty;
        public DictionaryValueList<string, string> keyValuePairs;
        public Dictionary<string, int> processedResourceSuccesses;
        public Dictionary<string, int> processedResourceFailures;
        public Dictionary<string, SnacksRosterResource> rosterResources;
        #endregion

        #region Constructors
        public AstronautData()
        {
            keyValuePairs = new DictionaryValueList<string, string>();
            processedResourceFailures = new Dictionary<string, int>();
            processedResourceSuccesses = new Dictionary<string, int>();
            rosterResources = new Dictionary<string, SnacksRosterResource>();
        }
        #endregion

        #region Load & Save
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
            ConfigNode[] astronautNodess = node.GetNodes(AstronautNode);

            foreach (ConfigNode astronautNode in astronautNodess)
            {
                try
                {
                    AstronautData astronautData = new AstronautData();

                    astronautData.name = astronautNode.GetValue(AstronautNodeName);
                    astronautData.experienceTrait = astronautNode.GetValue(AstronautNodeTrait);
                    astronautData.lastUpdated = double.Parse(astronautNode.GetValue(AstronautNodeUpdated));
                    astronautData.isExempt = bool.Parse(astronautNode.GetValue(AstronautNodeExempt));

                    if (astronautNode.HasValue(AstronautNodeCondition))
                        astronautData.conditionSummary = astronautNode.GetValue(AstronautNodeCondition);

                    //Key value pairs
                    astronautData.keyValuePairs = new DictionaryValueList<string, string>();
                    if (astronautNode.HasNode(KeyValueNode))
                    {
                        ConfigNode[] keyValuePairs = astronautNode.GetNodes(KeyValueNode);
                        foreach (ConfigNode keyValue in keyValuePairs)
                            astronautData.keyValuePairs.Add(keyValue.GetValue(KeyValuePairKey), keyValue.GetValue(KeyValuePairValue));
                    }

                    //Success/fail counters
                    if (astronautNode.HasNode(ResourceCounterNode))
                    {
                        ConfigNode[] resourceCounterNodes = astronautNode.GetNodes(ResourceCounterNode);
                        ConfigNode counterNode;
                        string resourceName = string.Empty;
                        int count = 0;
                        bool isSuccess = false;
                        for (int index = 0; index < resourceCounterNodes.Length; index++)
                        {
                            counterNode = resourceCounterNodes[index];
                            resourceName = counterNode.GetValue(ResourceCounterName);
                            int.TryParse(counterNode.GetValue(ResourceCounterValue), out count);
                            isSuccess = false;
                            bool.TryParse(counterNode.GetValue(ResourceCounterIsSuccess), out isSuccess);

                            if (isSuccess)
                                astronautData.processedResourceSuccesses.Add(resourceName, count);
                            else
                                astronautData.processedResourceFailures.Add(resourceName, count);
                        }
                    }

                    //Roster resources
                    if (astronautNode.HasNode(SnacksRosterResource.RosterResourceNode))
                        astronautData.rosterResources = SnacksRosterResource.LoadFromAstronautData(astronautNode);

                    crewData.Add(astronautData.name, astronautData);
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
            AstronautData astronautData;
            ConfigNode configNode;
            ConfigNode astronautNode;
            string[] keys;

            while (dataValues.MoveNext())
            {
                astronautData = dataValues.Current;
                astronautNode = new ConfigNode(AstronautNode);
                astronautNode.AddValue(AstronautNodeName, astronautData.name);
                astronautNode.AddValue(AstronautNodeTrait, astronautData.experienceTrait);
                astronautNode.AddValue(AstronautNodeUpdated, astronautData.lastUpdated);
                astronautNode.AddValue(AstronautNodeExempt, astronautData.isExempt);

                if (!string.IsNullOrEmpty(astronautData.conditionSummary))
                    astronautNode.AddValue(AstronautNodeCondition, astronautData.conditionSummary);

                //Save keyvalue pairs
                keys = astronautData.keyValuePairs.Keys.ToArray();
                for (int index = 0; index < keys.Length; index++)
                {
                    configNode = new ConfigNode(KeyValueNode);
                    configNode.AddValue(KeyValuePairKey, keys[index]);
                    configNode.AddValue(KeyValuePairValue, astronautData.keyValuePairs[keys[index]]);
                    astronautNode.AddNode(configNode);
                }

                //Save resource process results
                keys = astronautData.processedResourceSuccesses.Keys.ToArray();
                for (int index = 0; index < keys.Length; index++)
                {
                    configNode = new ConfigNode(ResourceCounterNode);
                    configNode.AddValue(ResourceCounterName, keys[index]);
                    configNode.AddValue(ResourceCounterIsSuccess, true);
                    configNode.AddValue(ResourceCounterValue, astronautData.processedResourceSuccesses[keys[index]]);
                    astronautNode.AddNode(configNode);
                }

                keys = astronautData.processedResourceFailures.Keys.ToArray();
                for (int index = 0; index < keys.Length; index++)
                {
                    configNode = new ConfigNode(ResourceCounterNode);
                    configNode.AddValue(ResourceCounterName, keys[index]);
                    configNode.AddValue(ResourceCounterIsSuccess, false);
                    configNode.AddValue(ResourceCounterValue, astronautData.processedResourceFailures[keys[index]]);
                    astronautNode.AddNode(configNode);
                }

                //Save roster resources
                SnacksRosterResource.SaveToAstronautData(astronautData.rosterResources, astronautNode);

                node.AddNode(astronautNode);
            }
        }
        #endregion

        #region API
        public void SetCondition(string condition)
        {
            if (string.IsNullOrEmpty(conditionSummary))
                conditionSummary = condition;

            else if (!conditionSummary.Contains(condition))
                conditionSummary += ", " + condition;
        }

        public void ClearCondition(string condition)
        {
            if (conditionSummary == condition)
                conditionSummary = string.Empty;

            else if (conditionSummary.Contains(condition))
            {
                conditionSummary = conditionSummary.Replace(", " + condition, "");
                conditionSummary = conditionSummary.Replace(condition + ", ", "");
            }
        }
        #endregion
    }
}
