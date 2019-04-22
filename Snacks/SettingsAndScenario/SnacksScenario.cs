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
    [KSPScenario(ScenarioCreationOptions.AddToAllGames, GameScenes.SPACECENTER, GameScenes.EDITOR, GameScenes.FLIGHT, GameScenes.TRACKSTATION)]
    public class SnacksScenario : ScenarioModule
    {
        #region Constants
        public double secondsPerCycle = 3600.0;
        #endregion

        #region Housekeeping
        public static SnacksScenario Instance;
        public static bool LoggingEnabled;
        public DictionaryValueList<string, int> sciencePenalties = new DictionaryValueList<string, int>();
        public DictionaryValueList<string, AstronautData> crewData;
        public string exemptKerbals = string.Empty;
        public double cycleStartTime;
        public Dictionary<string, SnacksBackgroundConverter> backgroundConverters;
        public List<Part> createdParts;
        #endregion

        public static double GetHoursPerDay()
        {
            return GameSettings.KERBIN_TIME == true ? 21600 : 86400;
        }

        void FixedUpdate()
        {
            if (cycleStartTime == 0f)
            {
                cycleStartTime = Planetarium.GetUniversalTime();
                return;
            }
            double elapsedTime = Planetarium.GetUniversalTime() - cycleStartTime;
            if (elapsedTime < secondsPerCycle)
                return;
            cycleStartTime = Planetarium.GetUniversalTime();

            //Go through all the vessels. For loaded vessels, run the consumers and events.
            //For unloaded vessels, run the background converters, consumers, and events.
            foreach (Vessel vessel in FlightGlobals.Vessels)
            {
                if (vessel.loaded)
                {
                    //Run consumers

                    //Handle events
                }
                else
                {
                    //Run background converters
                    runBackgroundConverters(vessel, elapsedTime);

                    //Run consumers

                    //Handle events
                }
            }
        }

        #region Background Processing
        protected void runBackgroundConverters(Vessel vessel, double elapsedTime)
        {
            string vesselID = vessel.id.ToString();
            SnacksBackgroundConverter[] converters = backgroundConverters.Values.ToArray();
            int count = converters.Length;
            SnacksBackgroundConverter converter;

            for (int index = 0; index < count; index++)
            {
                converter = converters[index];
                if (converter.vesselID != vesselID)
                    continue;

                if (converter.IsActivated && !converter.isMissingResources && !converter.isContainerFull)
                    StartCoroutine(runConverter(converter, elapsedTime, vessel.protoVessel));
            }
        }

        protected IEnumerator<YieldInstruction> runConverter(SnacksBackgroundConverter converter, double elapsedTime, ProtoVessel protoVessel)
        {
            //Get ready to process
            converter.PrepareToProcess(protoVessel);
            yield return new WaitForFixedUpdate();

            //Check required
            converter.CheckRequiredResources(protoVessel, elapsedTime);
            yield return new WaitForFixedUpdate();

            //Consume inputs
            converter.ConsumeInputResources(protoVessel, elapsedTime);
            yield return new WaitForFixedUpdate();

            //Produce outputs
            converter.ProduceOutputResources(protoVessel, elapsedTime);
            yield return new WaitForFixedUpdate();

            //Produce yields
            converter.ProduceYieldResources(protoVessel);
            yield return new WaitForFixedUpdate();

            //Post process
            converter.PostProcess(protoVessel);
            yield return new WaitForFixedUpdate();
        }

        public SnacksBackgroundConverter GetBackgroundConverter(SnacksConverter converter)
        {
            if (backgroundConverters.ContainsKey(converter.ID))
                return backgroundConverters[converter.ID];

            return null;
        }

        public void UpdateBackgroundConverter(SnacksBackgroundConverter converter)
        {
            if (backgroundConverters.ContainsKey(converter.converterID))
                backgroundConverters[converter.converterID] = converter;
        }

        public void RegisterBackgroundConverter(SnacksConverter converter)
        {
            SnacksBackgroundConverter backgroundConverter = new SnacksBackgroundConverter();

            if (IsRegistered(converter))
            {
                backgroundConverter = GetBackgroundConverter(converter);
                backgroundConverter.GetConverterData(converter);
                backgroundConverter.IsActivated = converter.IsActivated;
                backgroundConverter.isMissingResources = false;
                backgroundConverter.isContainerFull = false;
                backgroundConverter.vesselID = converter.part.vessel.id.ToString();
                backgroundConverter.inputEfficiency = converter.inputEfficiency;
                backgroundConverter.outputEfficiency = converter.outputEfficiency;
                backgroundConverter.moduleName = converter.ClassName;

                UpdateBackgroundConverter(backgroundConverter);
                return;
            }

            backgroundConverter.vesselID = converter.part.vessel.id.ToString();
            backgroundConverter.GetConverterData(converter);

            backgroundConverters.Add(backgroundConverter.converterID, backgroundConverter);
        }

        public void UnregisterBackgroundConverter(SnacksConverter converter)
        {
            if (!IsRegistered(converter))
                return;

            if (backgroundConverters.ContainsKey(converter.ID))
                backgroundConverters.Remove(converter.ID);
        }

        public bool IsRegistered(SnacksConverter converter)
        {
            return backgroundConverters.ContainsKey(converter.ID);
        }

        public bool WasRecentlyCreated(Part part)
        {
            return createdParts.Contains(part);
        }
        #endregion

        #region Astronaut API
        public void SetExemptCrew(string exemptedCrew)
        {
            if (string.IsNullOrEmpty(exemptedCrew))
                return;
            string[] exemptedKerbals = exemptKerbals.Split(new char[] { ';' });
            AstronautData[] astronautData = crewData.Values.ToArray();
            AstronautData data;

            for (int index = 0; index < astronautData.Length; index++)
            {
                data = astronautData[index];
                if (exemptedCrew.Contains(data.name))
                    data.isExempt = true;
                else
                    data.isExempt = false;
            }
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
            GetAstronautData(astronaut);
        }

        public void UnregisterCrew(ProtoCrewMember astronaut)
        {
            if (crewData.Contains(astronaut.name))
                crewData.Remove(astronaut.name);
        }

        public void UnregisterCrew(AstronautData data)
        {
            if (crewData.Contains(data.name))
                crewData.Remove(data.name);
        }

        public int GetNonExemptCrewCount(Vessel vessel)
        {
            ProtoCrewMember[] astronauts;
            int nonExemptCrew = 0;
            AstronautData data;

            if (vessel.loaded)
                astronauts = vessel.GetVesselCrew().ToArray();
            else
                astronauts = vessel.protoVessel.GetVesselCrew().ToArray();

            for (int index = 0; index < astronauts.Length; index++)
            {
                data = SnacksScenario.Instance.GetAstronautData(astronauts[index]);
                if (data.isExempt == false)
                    nonExemptCrew = nonExemptCrew + 1;
            }
            
            return nonExemptCrew;
        }

        public int AddMissedMeals(ProtoCrewMember astronaut, int mealsMissed)
        {
            AstronautData data = GetAstronautData(astronaut);

            //Handle exemptions
            if (data.isExempt)
                return 0;

            data.mealsMissed += mealsMissed;

            return data.mealsMissed;
        }

        public int GetMealsMissed(ProtoCrewMember astronaut)
        {
            AstronautData data = GetAstronautData(astronaut);

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
            AstronautData data = GetAstronautData(astronaut);

            data.mealsMissed = mealsMissed;
        }

        public AstronautData GetAstronautData(ProtoCrewMember astronaut)
        {
            if (crewData.Contains(astronaut.name) == false)
            {
                AstronautData data = new AstronautData();
                data.name = astronaut.name;
                data.mealsMissed = 0;
                data.experienceTrait = astronaut.experienceTrait.Title;
                data.lastUpdated = Planetarium.GetUniversalTime();
                data.keyValuePairs = new DictionaryValueList<string, string>();

                //Don't forget about exemptions

                crewData.Add(data.name, data);
            }

            return crewData[astronaut.name];
        }

        public void SetAstronautData(AstronautData data)
        {
            if (crewData.Contains(data.name))
                crewData[data.name] = data;
        }
        #endregion

        #region Overrides
        public override void OnAwake()
        {
            base.OnAwake();
            if (HighLogic.LoadedSceneIsEditor)
            {
                GameEvents.onEditorPartEvent.Add(onEditorPartEvent);
                createdParts = new List<Part>();
            }
            Instance = this;
            LoggingEnabled = SnacksProperties.DebugLoggingEnabled;
        }

        public void Destroy()
        {
            GameEvents.onEditorPartEvent.Remove(onEditorPartEvent);
        }

        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);

            if (node.HasValue("exemptkerbals"))
                exemptKerbals = node.GetValue("exemptKerbals");

            ConfigNode[] penalties = node.GetNodes("SCIENCE_PENALTY");
            foreach (ConfigNode penaltyNode in penalties)
            {
                sciencePenalties.Add(penaltyNode.GetValue("vesselID"), int.Parse(penaltyNode.GetValue("amount")));
            }

            //Converters
            if (node.HasValue("cycleStartTime"))
            {
                double.TryParse(node.GetValue("cycleStartTime"), out cycleStartTime);
                backgroundConverters = SnacksBackgroundConverter.BuildBackgroundConvertersMap(node);
            }

            //Load astronaut data
            crewData = AstronautData.Load(node);
        }

        public override void OnSave(ConfigNode node)
        {
            base.OnSave(node);
            ConfigNode configNode;

            if (string.IsNullOrEmpty(exemptKerbals) == false)
                node.AddValue("exemptKerbals", exemptKerbals);

            foreach (string key in sciencePenalties.Keys)
            {
                configNode = new ConfigNode("SCIENCE_PENALTY");
                configNode.AddValue("vesselID", key);
                configNode.AddValue("amount", sciencePenalties[key].ToString());
                node.AddNode(configNode);
            }

            //Save astronaut data
            AstronautData.Save(crewData, node);

            //Converters
            node.AddValue("cycleStartTime", cycleStartTime);
            SnacksBackgroundConverter.SaveBackgroundConvertersMap(backgroundConverters, node);
        }
        #endregion

        #region Game Events
        public void onEditorPartEvent(ConstructionEventType eventType, Part part)
        {
            if (!HighLogic.LoadedSceneIsEditor)
                return;
            if (createdParts == null)
                createdParts = new List<Part>();

            switch (eventType)
            {
                case ConstructionEventType.PartCreated:
                    if (!createdParts.Contains(part))
                        createdParts.Add(part);
                    break;

                case ConstructionEventType.PartDeleted:
                    if (createdParts.Contains(part))
                        createdParts.Remove(part);
                    break;
            }
        }
        #endregion
    }
}
