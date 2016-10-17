using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KSP;
using KSP.IO;

namespace Snacks
{
    public class AstronautData
    {
        public string name;
        public string experienceTrait;
        public double lastUpdated;
        public int mealsMissed;
        public DictionaryValueList<string, string> keyValuePairs;
    }

    [KSPScenario(ScenarioCreationOptions.AddToAllGames, GameScenes.SPACECENTER, GameScenes.EDITOR, GameScenes.FLIGHT, GameScenes.TRACKSTATION)]
    public class SnacksScenario : ScenarioModule
    {
        public static SnacksScenario Instance;
        public DictionaryValueList<string, int> sciencePenalties = new DictionaryValueList<string, int>();
        public List<string> knownVessels = new List<string>();
        public DictionaryValueList<string, AstronautData> crewData = new DictionaryValueList<string, AstronautData>();

        public override void OnAwake()
        {
            base.OnAwake();
            Instance = this;
        }

        public void RegisterCrew(Vessel vessel)
        {
            List<ProtoCrewMember> crewManifest;
            int crewCount = 0;

            if (vessel.loaded)
            {
                crewManifest = vessel.GetVesselCrew();
                crewCount = crewManifest.Count;
            }

            else
            {
                crewManifest = vessel.protoVessel.GetVesselCrew();
                crewCount = crewManifest.Count;
            }

            for (int index = 0; index < crewCount; index++)
                RegisterCrew(crewManifest[index]);

        }

        public void UnregisterCrew(ProtoVessel protoVessel)
        {
            List<ProtoCrewMember> crewManifest;
            int crewCount = 0;

            crewManifest = protoVessel.GetVesselCrew();
            crewCount = crewManifest.Count;

            for (int index = 0; index < crewCount; index++)
                UnregisterCrew(crewManifest[index]);
        }

        public void UnregisterCrew(Vessel vessel)
        {
            List<ProtoCrewMember> crewManifest;
            int crewCount = 0;

            if (vessel.loaded)
            {
                crewManifest = vessel.GetVesselCrew();
                crewCount = crewManifest.Count;
            }

            else
            {
                crewManifest = vessel.protoVessel.GetVesselCrew();
                crewCount = crewManifest.Count;
            }

            for (int index = 0; index < crewCount; index++)
                UnregisterCrew(crewManifest[index]);
        }

        public void RegisterCrew(ProtoCrewMember astronaut)
        {
            getAstronautData(astronaut);
        }

        public void UnregisterCrew(ProtoCrewMember astronaut)
        {
            if (crewData.Contains(astronaut.name))
                crewData.Remove(astronaut.name);
        }

        public int AddMissedMeals(ProtoCrewMember astronaut, int mealsMissed)
        {
            AstronautData data = getAstronautData(astronaut);

            data.mealsMissed += mealsMissed;

            return data.mealsMissed;
        }

        public int GetMealsMissed(ProtoCrewMember astronaut)
        {
            AstronautData data = getAstronautData(astronaut);

            return data.mealsMissed;
        }

        public void ClearMissedMeals(ProtoVessel protoVessel)
        {
            if (protoVessel.GetVesselCrew().Count == 0)
                return;

            ProtoCrewMember[] crewMembers = protoVessel.GetVesselCrew().ToArray();

            for (int index = 0; index < crewMembers.Length; index++)
            {
                crewMembers[index].inactive = false;
                SetMealsMissed(crewMembers[index], 0);
            }
        }

        public void ClearMissedMeals(Vessel vessel)
        {
            if (vessel.GetVesselCrew().Count == 0)
                return;

            ProtoCrewMember[] crewMembers = vessel.GetVesselCrew().ToArray();

            for (int index = 0; index < crewMembers.Length; index++)
            {
                crewMembers[index].inactive = false;
                SetMealsMissed(crewMembers[index], 0);
            }
        }

        public void SetMealsMissed(ProtoCrewMember astronaut, int mealsMissed)
        {
            AstronautData data = getAstronautData(astronaut);

            data.mealsMissed = mealsMissed;
        }

        public AstronautData GetAstronautData(ProtoCrewMember astronaut)
        {
            return getAstronautData(astronaut);
        }

        public void SetAstronautData(AstronautData data)
        {
            if (crewData.Contains(data.name))
                crewData[data.name] = data;
        }

        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);

            ConfigNode[] penalties = node.GetNodes("SCIENCE_PENALTY");
            foreach (ConfigNode penaltyNode in penalties)
            {
                sciencePenalties.Add(penaltyNode.GetValue("vesselID"), int.Parse(penaltyNode.GetValue("amount")));
            }

            ConfigNode[] KnownVessels = node.GetNodes("KNOWN_VESSEL");
            foreach (ConfigNode vesselNode in KnownVessels)
            {
                if (knownVessels.Contains(vesselNode.GetValue("vesselID")) == false)
                    knownVessels.Add(vesselNode.GetValue("vesselID"));
            }

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
                    data.keyValuePairs = new DictionaryValueList<string, string>();

                    ConfigNode[] keyValuePairs = astronaut.GetNodes("KEYVALUE");
                    foreach (ConfigNode keyValue in keyValuePairs)
                        data.keyValuePairs.Add(keyValue.GetValue("key"), keyValue.GetValue("value"));

                    crewData.Add(data.name, data);
                }
                catch //(Exception ex)
                {
                    //Debug.Log("[Snacks] - error encountered: " + ex + " skipping kerbal.");
                    continue;
                }
            }
        }

        public override void OnSave(ConfigNode node)
        {
            base.OnSave(node);
            ConfigNode configNode;

            foreach (string key in sciencePenalties.Keys)
            {
                configNode = new ConfigNode("SCIENCE_PENALTY");
                configNode.AddValue("vesselID", key);
                configNode.AddValue("amount", sciencePenalties[key].ToString());
                node.AddNode(configNode);
            }

            foreach (string vesselID in knownVessels)
            {
                configNode = new ConfigNode("KNOWN_VESSEL");
                configNode.AddValue("vesselID", vesselID);
                node.AddNode(configNode);
            }

            List<AstronautData>.Enumerator dataValues = crewData.GetListEnumerator();
            List<string>.Enumerator keyValueEnumerator;
            string keyValueKey;
            AstronautData data;
            ConfigNode keyValueNode;
            while (dataValues.MoveNext())
            {
                data = dataValues.Current;
                configNode = new ConfigNode("ASTRONAUT");
                configNode.AddValue("name", data.name);
                configNode.AddValue("experienceTrait", data.experienceTrait);
                configNode.AddValue("mealsMissed", data.mealsMissed);
                configNode.AddValue("lastUpdated", data.lastUpdated);

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

        protected AstronautData getAstronautData(ProtoCrewMember astronaut)
        {
            if (crewData.Contains(astronaut.name) == false)
            {
                AstronautData data = new AstronautData();
                data.name = astronaut.name;
                data.mealsMissed = 0;
                data.experienceTrait = astronaut.experienceTrait.Title;
                data.lastUpdated = Planetarium.GetUniversalTime();
                data.keyValuePairs = new DictionaryValueList<string, string>();

                crewData.Add(data.name, data);
            }

            return crewData[astronaut.name];
        }
    }
}
