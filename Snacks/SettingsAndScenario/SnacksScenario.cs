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
    /// <summary>
    /// Interface for creating and running penalties when a consumer resource runs out or has too much aboard the vessel or kerbal.
    /// </summary>
    public interface ISnacksPenalty
    {
        /// <summary>
        /// Indicates whether or not the penalty is enabled.
        /// </summary>
        /// <returns>true if inabled, false if not.</returns>
        bool IsEnabled();

        /// <summary>
        /// Indicates whether or not the penalty is always applied instead of randomly chosen.
        /// </summary>
        /// <returns>true if the penalty should always be applied, false if not.</returns>
        bool AlwaysApply();

        /// <summary>
        /// Applies the penalty to the affected kerbals
        /// </summary>
        /// <param name="affectedKerbals">An int containing the number of kerbals affected by the penalty.</param>
        /// <param name="vessel">The vessel to apply the penalty to.</param>
        void ApplyPenalty(int affectedKerbals, Vessel vessel);

        /// <summary>
        /// Removes penalty effects.
        /// </summary>
        /// <param name="vessel">The vessel to remove the penalt effects from.</param>
        void RemovePenalty(Vessel vessel);

        /// <summary>
        /// Handles changes in game settings, if any.
        /// </summary>
        void GameSettingsApplied();
    }

    [KSPScenario(ScenarioCreationOptions.AddToAllGames, GameScenes.SPACECENTER, GameScenes.EDITOR, GameScenes.FLIGHT, GameScenes.TRACKSTATION)]
    public class SnacksScenario : ScenarioModule
    {
        #region Constants
        public double secondsPerCycle = 3600.0;
        #endregion

        #region Game Events
        /// <summary>
        /// Tells listeners that snapshots were created.
        /// </summary>
        public static EventData<Vessel, Snackshot> onSnapshotsUpdated = new EventData<Vessel, Snackshot>("onSnapshotsUpdated");

        /// <summary>
        /// Tells listeners that a simulator was created. Gives mods a chance to add custom converters not covered by Snacks.
        /// </summary>
        public static EventData<SimSnacks, SimulatorContext> onSimulatorCreated = new EventData<SimSnacks, SimulatorContext>("onSimulatorCreated");

        /// <summary>
        /// Tells listeners that background converters were created. Gives mods a chance to add custom converters not covered by Snacks.
        /// </summary>
        public static EventData<Vessel> onBackgroundConvertersCreated = new EventData<Vessel>("onBackgroundConvertersCreated");
        #endregion

        #region Housekeeping
        public static SnacksScenario Instance;
        public static bool LoggingEnabled;
        private static double secondsPerDay = 0;
        private static double secondsPerYear = 0;
        public DictionaryValueList<string, int> sciencePenalties;
        public DictionaryValueList<string, AstronautData> crewData;
        public string exemptKerbals = string.Empty;
        public double cycleStartTime;
        public Dictionary<Vessel, List<SnacksBackgroundConverter>> backgroundConverters;
        public List<BaseResourceProcessor> resourceProcessors;
        public List<SnacksPartResource> snacksPartResources;
        public List<SnacksEVAResource> snacksEVAResources;
        public Dictionary<Vessel, VesselSnackshot> snapshotMap;
        public Dictionary<int, int> bodyVesselCountMap;
        public SnackSimThreadPool threadPool = null;

        public string converterWatchlist = "SnacksConverter;SnacksProcessor;SoilRecycler;WBIResourceConverter;WBIModuleResourceConverterFX;ModuleResourceConverter;ModuleFusionReactor";
        public double simulatorSecondsPerCycle = 3600;
        public int maxSimulatorCycles = 10000;
        public int maxThreads = 4;

        private Part lastEVAKerbal;
        private Part lastBoardedKerbal;
        private double elapsedTime;
        #endregion

        #region Snapshots
        /// <summary>
        /// Updates the resource snapshots for each vessel in the game that isn't Debris, a Flag, a SpaceObject, or Unknown.
        /// </summary>
        public void UpdateSnapshots()
        {
            if (threadPool == null)
                threadPool = new SnackSimThreadPool();

            StartCoroutine(updateSnapshots());
        }

        protected IEnumerator<YieldInstruction> updateSnapshots()
        {
            //Clear the snapshot map
            snapshotMap.Clear();
            bodyVesselCountMap.Clear();

            int count = FlightGlobals.Vessels.Count;
            int consumerCount = 0;
            int consumerIndex = 0;
            int resourceCount = 0;
            int resourceIndex = 0;
            List<ProcessedResource> consumerResources;
            Vessel vessel;
            Snackshot snackshot;
            string resourceName;

            for (int index = 0; index < count; index++)
            {
                vessel = FlightGlobals.Vessels[index];

                //Skip vessel types that we're not interested in.
                if (vessel.vesselType == VesselType.Debris ||
                    vessel.vesselType == VesselType.Flag ||
                    vessel.vesselType == VesselType.SpaceObject ||
                    vessel.vesselType == VesselType.Unknown)
                    continue;

                //Get crew capacity & crew count. Skip vessels that have no crew capacity or crew count.
                int crewCapacity = GetCrewCapacity(vessel);
                if (crewCapacity <= 0)
                    continue;
                int crewCount = 0;
                if (vessel.loaded)
                    crewCount = vessel.GetCrewCount();
                else
                    crewCount = vessel.protoVessel.GetVesselCrew().Count;
                if (crewCount <= 0)
                    continue;
                yield return new WaitForFixedUpdate();

                //Now create the vessel snackshot
                VesselSnackshot vesselSnackshot = new VesselSnackshot();
                vesselSnackshot.vessel = vessel;
                vesselSnackshot.bodyID = vessel.mainBody.flightGlobalsIndex;
                vesselSnackshot.vesselName = vessel.vesselName;
                vesselSnackshot.crewCount = crewCount;
                vesselSnackshot.maxCrewCount = crewCapacity;
                snapshotMap.Add(vessel, vesselSnackshot);

                //Next, update body vessel count map
                if (!bodyVesselCountMap.ContainsKey(vessel.mainBody.flightGlobalsIndex))
                    bodyVesselCountMap.Add(vessel.mainBody.flightGlobalsIndex, 0);
                bodyVesselCountMap[vessel.mainBody.flightGlobalsIndex] += 1;

                //Get simulator data: complete resource list and all the converters
                SimSnacks simSnacks = SimSnacks.CreateSimulator(vessel);
                yield return new WaitForFixedUpdate();

                //Give mods a chance to add converters that aren't covered by Snacks.
                SimulatorContext context = new SimulatorContext();
                context.vessel = vessel;
                context.simulatedVesselType = vessel.loaded ? SimulatedVesselTypes.simVesselLoaded : SimulatedVesselTypes.simVesselUnloaded;
                onSimulatorCreated.Fire(simSnacks, context);
                yield return new WaitForFixedUpdate();

                //Create initial snackshots
                consumerCount = resourceProcessors.Count;
                for (consumerIndex = 0; consumerIndex < consumerCount; consumerIndex++)
                {
                    //Get resources consumed and produced
                    resourceProcessors[consumerIndex].AddConsumedAndProducedResources(vessel, simSnacks.secondsPerCycle, simSnacks.consumedResources, simSnacks.producedResources);

                    //First check input list for resources to add to the snapshots window
                    consumerResources = resourceProcessors[consumerIndex].inputList;
                    resourceCount = consumerResources.Count;
                    for (resourceIndex = 0; resourceIndex < resourceCount; resourceIndex++)
                    {
                        resourceName = consumerResources[resourceIndex].resourceName;

                        if (consumerResources[resourceIndex].showInSnapshot && simSnacks.resources.ContainsKey(resourceName))
                        {
                            snackshot = new Snackshot();
                            snackshot.showTimeRemaining = true;
                            snackshot.resourceName = consumerResources[resourceIndex].resourceName;
                            snackshot.amount = simSnacks.resources[resourceName].amount;
                            snackshot.maxAmount = simSnacks.resources[resourceName].maxAmount;

                            //Add to snackshots
                            vesselSnackshot.snackshots.Add(snackshot);
                        }
                    }

                    //Next check outputs
                    consumerResources = resourceProcessors[consumerIndex].outputList;
                    resourceCount = consumerResources.Count;
                    for (resourceIndex = 0; resourceIndex < resourceCount; resourceIndex++)
                    {
                        resourceName = consumerResources[resourceIndex].resourceName;

                        if (consumerResources[resourceIndex].showInSnapshot && simSnacks.resources.ContainsKey(resourceName))
                        {
                            snackshot = new Snackshot();
                            snackshot.showTimeRemaining = true;
                            snackshot.resourceName = consumerResources[resourceIndex].resourceName;
                            snackshot.amount = simSnacks.resources[resourceName].amount;
                            snackshot.maxAmount = simSnacks.resources[resourceName].maxAmount;

                            //Add to snackshots
                            vesselSnackshot.snackshots.Add(snackshot);
                        }
                    }
                }

                //Finally, add the simulator to the jobs list
                threadPool.AddSimulatorJob(simSnacks);
            }
            yield return new WaitForFixedUpdate();
        }

        public int GetCrewCapacity(Vessel vessel)
        {
            if (vessel.loaded)
                return vessel.GetCrewCapacity();

            ProtoPartSnapshot[] protoParts = vessel.protoVessel.protoPartSnapshots.ToArray();
            int crewCapacity = 0;

            for (int index = 0; index < protoParts.Length; index++)
                crewCapacity += protoParts[index].partInfo.partPrefab.CrewCapacity;

            return crewCapacity;
        }
        #endregion

        #region Fixed Update
        void FixedUpdate()
        {
            //Record cycle start time if needed.
            if (cycleStartTime == 0f)
            {
                cycleStartTime = Planetarium.GetUniversalTime();
                return;
            }

            //To avoid hammering the game with updates, we only run background converters, consumers, and events once per game hour.
            elapsedTime = Planetarium.GetUniversalTime() - cycleStartTime;
            if (elapsedTime < secondsPerCycle)
                return;
            cycleStartTime = Planetarium.GetUniversalTime();

            RunSnackCyleImmediately();
        }

        public void RunSnackCyleImmediately()
        {
            //Go through all the vessels. For loaded vessels, run the consumers and events.
            //For unloaded vessels, run the background converters, consumers, and events.
            Vessel vessel;
            int count = FlightGlobals.Vessels.Count;
            for (int vesselIndex = 0; vesselIndex < count; vesselIndex++)
            {
                vessel = FlightGlobals.Vessels[vesselIndex];

                //Skip vessel types that we're not interested in.
                if (vessel.vesselType == VesselType.Debris ||
                    vessel.vesselType == VesselType.Flag ||
                    vessel.vesselType == VesselType.SpaceObject ||
                    vessel.vesselType == VesselType.Unknown)
                    continue;

                //Start snack cycle
                StartCoroutine(runSnackCycle(vessel, elapsedTime));
            }
        }
        #endregion

        #region Background Processing
        protected void runBackgroundConverters(Vessel vessel, double elapsedTime)
        {
            List<SnacksBackgroundConverter> converters;
            if (backgroundConverters.ContainsKey(vessel))
                converters = backgroundConverters[vessel];
            else
                return;
            int count = converters.Count;
            SnacksBackgroundConverter converter;

            for (int index = 0; index < count; index++)
            {
                converter = converters[index];

                if (converter.IsActivated && !converter.isMissingResources && !converter.isContainerFull)
                    runConverter(converter, elapsedTime, vessel.protoVessel);
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
            converter.ProduceyieldsList(protoVessel);
            yield return new WaitForFixedUpdate();

            //Post process
            converter.PostProcess(protoVessel);
            yield return new WaitForFixedUpdate();
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
            Instance = this;
            LoggingEnabled = SnacksProperties.DebugLoggingEnabled;

            //Get simulator settings
            ConfigNode[] simulatorSettingsNodes = GameDatabase.Instance.GetConfigNodes("SNACKS_SIMULATOR");
            if (simulatorSettingsNodes[0] != null)
            {
                if (simulatorSettingsNodes[0].HasValue("converterWatchlist"))
                    converterWatchlist = simulatorSettingsNodes[0].GetValue("converterWatchlist");

                if (simulatorSettingsNodes[0].HasValue("secondsPerCycle"))
                    double.TryParse(simulatorSettingsNodes[0].GetValue("secondsPerCycle"), out simulatorSecondsPerCycle);

                if (simulatorSettingsNodes[0].HasValue("maxSimulatorCycles"))
                    int.TryParse(simulatorSettingsNodes[0].GetValue("maxSimulatorCycles"), out maxSimulatorCycles);

                if (simulatorSettingsNodes[0].HasValue("maxThreads"))
                    int.TryParse(simulatorSettingsNodes[0].GetValue("maxThreads"), out maxThreads);
            }

            //Get seconds per day
            GetSecondsPerDay();

            //Game events
            GameEvents.onVesselLoaded.Add(onVesselLoaded);
            GameEvents.onCrewOnEva.Add(onCrewOnEva);
            GameEvents.onCrewBoardVessel.Add(onCrewBoardVessel);
            GameEvents.OnGameSettingsApplied.Add(onGameSettingsApplied);
            GameEvents.onVesselWasModified.Add(onVesselWasModified);
            GameEvents.onVesselWillDestroy.Add(onVesselWillDestroy);
            GameEvents.OnVesselRecoveryRequested.Add(onVesselRecoveryRequested);
            GameEvents.OnVesselRollout.Add(onVesselRollout);
            GameEvents.onLevelWasLoadedGUIReady.Add(onLevelLoaded);
            GameEvents.onVesselRecovered.Add(onVesselRecovered);
            GameEvents.onVesselTerminated.Add(onVesselTerminated);
            GameEvents.onKerbalAdded.Add(onKerbalAdded);
            GameEvents.onKerbalNameChanged.Add(onKerbalNameChanged);
            GameEvents.onKerbalRemoved.Add(onKerbalRemoved);
            GameEvents.OnCrewmemberHired.Add(OnCrewmemberHired);
            GameEvents.OnCrewmemberSacked.Add(OnCrewmemberSacked);
            GameEvents.OnCrewmemberLeftForDead.Add(OnCrewmemberLeftForDead);
            GameEvents.onKerbalLevelUp.Add(onKerbalLevelUp);
            GameEvents.onKerbalStatusChanged.Add(onKerbalStatusChanged);
            GameEvents.onCrewTransferred.Add(onCrewTransferred);
            GameEvents.onEditorPartPlaced.Add(onEditorPartPlaced);
            GameEvents.onEditorPartPicked.Add(onEditorPartPicked);
            GameEvents.onEditorPodPicked.Add(onEditorPartPicked);

            //Create part resource list.
            snacksPartResources = SnacksPartResource.LoadPartResources();

            //Create eva resource list.
            snacksEVAResources = SnacksEVAResource.LoadEVAResources();

            //Create resource consumers list
            resourceProcessors = BaseResourceProcessor.LoadConsumers();

            //Create housekeeping lists and such
            sciencePenalties = new DictionaryValueList<string, int>();
            snapshotMap = new Dictionary<Vessel, VesselSnackshot>();
            bodyVesselCountMap = new Dictionary<int, int>();
        }

        public void Start()
        {
            //Get background converters
            backgroundConverters = SnacksBackgroundConverter.GetBackgroundConverters();
        }

        public void OnDestroy()
        {
            GameEvents.onVesselLoaded.Remove(onVesselLoaded);
            GameEvents.onCrewOnEva.Remove(onCrewOnEva);
            GameEvents.onCrewBoardVessel.Remove(onCrewBoardVessel);
            GameEvents.OnGameSettingsApplied.Remove(onGameSettingsApplied);
            GameEvents.onVesselWasModified.Remove(onVesselWasModified);
            GameEvents.onVesselWillDestroy.Remove(onVesselWillDestroy);
            GameEvents.OnVesselRecoveryRequested.Remove(onVesselRecoveryRequested);
            GameEvents.OnVesselRollout.Remove(onVesselRollout);
            GameEvents.onLevelWasLoadedGUIReady.Remove(onLevelLoaded);
            GameEvents.onVesselRecovered.Remove(onVesselRecovered);
            GameEvents.onVesselTerminated.Remove(onVesselTerminated);
            GameEvents.onKerbalAdded.Remove(onKerbalAdded);
            GameEvents.onKerbalNameChanged.Remove(onKerbalNameChanged);
            GameEvents.onKerbalRemoved.Remove(onKerbalRemoved);
            GameEvents.OnCrewmemberHired.Add(OnCrewmemberHired);
            GameEvents.OnCrewmemberSacked.Add(OnCrewmemberSacked);
            GameEvents.OnCrewmemberLeftForDead.Add(OnCrewmemberLeftForDead);
            GameEvents.onKerbalLevelUp.Remove(onKerbalLevelUp);
            GameEvents.onKerbalStatusChanged.Remove(onKerbalStatusChanged);
            GameEvents.onCrewTransferred.Remove(onCrewTransferred);
            GameEvents.onEditorPartPlaced.Remove(onEditorPartPlaced);
            GameEvents.onEditorPartPicked.Remove(onEditorPartPicked);
            GameEvents.onEditorPodPicked.Add(onEditorPartPicked);
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
                double.TryParse(node.GetValue("cycleStartTime"), out cycleStartTime);

            //Processors
            if (node.HasNode(BaseResourceProcessor.ConfigNodeName))
            {
                ConfigNode[] consumerNodes = node.GetNodes(BaseResourceProcessor.ConfigNodeName);
                ConfigNode processorNode;
                BaseResourceProcessor processor;
                Dictionary<string, ConfigNode> persistenceMap = new Dictionary<string, ConfigNode>();
                int index;

                //Build persistence map
                for (index = 0; index < consumerNodes.Length; index++)
                {
                    processorNode = consumerNodes[index];
                    if (processorNode.HasValue("processName"))
                        persistenceMap.Add(processorNode.GetValue("processName"), processorNode);
                }

                //Load persistence data
                int consumerCount = resourceProcessors.Count;
                for (index = 0; index < consumerCount; index++)
                {
                    processor = resourceProcessors[index];

                    //Find the persistence node
                    if (persistenceMap.ContainsKey(processor.processName))
                    {
                        processor.OnLoad(persistenceMap[processor.processName]);
                    }
                }
            }

            //Load astronaut data
            crewData = AstronautData.Load(node);
        }

        public override void OnSave(ConfigNode node)
        {
            base.OnSave(node);
            ConfigNode configNode;

            //Exempt kerbals
            if (string.IsNullOrEmpty(exemptKerbals) == false)
                node.AddValue("exemptKerbals", exemptKerbals);

            //Science penalties
            foreach (string key in sciencePenalties.Keys)
            {
                configNode = new ConfigNode("SCIENCE_PENALTY");
                configNode.AddValue("vesselID", key);
                configNode.AddValue("amount", sciencePenalties[key].ToString());
                node.AddNode(configNode);
            }

            //Save astronaut data
            AstronautData.Save(crewData, node);

            //Procssors
            int consumerCount = resourceProcessors.Count;
            for (int index = 0; index < consumerCount; index++)
            {
                configNode = resourceProcessors[index].OnSave();
                if (configNode != null)
                    node.AddNode(configNode);
            }
        }
        #endregion

        #region Game Events
        private void OnCrewmemberHired(ProtoCrewMember astronaut, int activeAstronautsCount)
        {
            onKerbalAdded(astronaut);
        }

        private void OnCrewmemberSacked(ProtoCrewMember astronaut, int activeAstronautsCount)
        {
            onKerbalRemoved(astronaut);
        }

        private void OnCrewmemberLeftForDead(ProtoCrewMember astronaut, int activeAstronautsCount)
        {
            onKerbalRemoved(astronaut);
        }

        private void onKerbalLevelUp(ProtoCrewMember astronaut)
        {
            AstronautData astronautData = GetAstronautData(astronaut);

            //Give consumers a chance to update their data if needed.
            int count = resourceProcessors.Count;
            BaseResourceProcessor consumer;
            for (int index = 0; index < count; index++)
            {
                consumer = resourceProcessors[index];
                consumer.onKerbalLevelUp(astronaut);
            }
        }

        private void onKerbalStatusChanged(ProtoCrewMember astronaut, ProtoCrewMember.RosterStatus previousStatus, ProtoCrewMember.RosterStatus newStatus)
        {
            if (newStatus == ProtoCrewMember.RosterStatus.Dead)
                onKerbalRemoved(astronaut);
        }

        private void onKerbalAdded(ProtoCrewMember astronaut)
        {
            if (!crewData.Contains(astronaut.name))
            {
                AstronautData astronautData = GetAstronautData(astronaut);

                //Give consumers a chance to add their data
                int count = resourceProcessors.Count;
                BaseResourceProcessor consumer;
                for (int index = 0; index < count; index++)
                {
                    consumer = resourceProcessors[index];
                    consumer.onKerbalAdded(astronaut);
                }
            }
        }

        private void onKerbalRemoved(ProtoCrewMember astronaut)
        {
            if (crewData.Contains(astronaut.name))
            {
                crewData.Remove(astronaut.name);

                //Give consumers a chance to remove their data if needed.
                int count = resourceProcessors.Count;
                BaseResourceProcessor consumer;
                for (int index = 0; index < count; index++)
                {
                    consumer = resourceProcessors[index];
                    consumer.onKerbalRemoved(astronaut);
                }
            }
        }

        private void onKerbalNameChanged(ProtoCrewMember astronaut, string previousName, string newName)
        {
            AstronautData astronautData = null;

            if (crewData.Contains(astronaut.name))
            {
                astronautData = GetAstronautData(astronaut);
                crewData.Remove(astronaut.name);
                astronautData.name = newName;
                crewData.Add(newName, astronautData);

                //Give consumers a chance to update their data if needed.
                int count = resourceProcessors.Count;
                BaseResourceProcessor consumer;
                for (int index = 0; index < count; index++)
                {
                    consumer = resourceProcessors[index];
                    consumer.onKerbalNameChanged(astronaut, previousName, newName);
                }
            }
        }

        private void onCrewTransferred(GameEvents.HostedFromToAction<ProtoCrewMember, Part> data)
        {
            ProtoCrewMember astronaut = data.host;
            Part fromPart = data.from;
            Part toPart = data.to;
        }

        private void onCrewBoardVessel(GameEvents.FromToAction<Part, Part> data)
        {
            try
            {
                Part evaKerbal = data.from;
                Part boardedPart = data.to;

                if (lastBoardedKerbal == evaKerbal)
                    return;
                lastBoardedKerbal = evaKerbal;

                //Inform all eva resources
                int count = snacksEVAResources.Count;
                for (int index = 0; index < count; index++)
                    snacksEVAResources[index].onCrewBoardedVessel(evaKerbal, boardedPart);

                //Give consumers a chance to update their data if needed.
                ProtoCrewMember astronaut = evaKerbal.vessel.GetVesselCrew()[0];
                count = resourceProcessors.Count;
                BaseResourceProcessor consumer;
                for (int index = 0; index < count; index++)
                {
                    consumer = resourceProcessors[index];
                    consumer.onKerbalBoardedVessel(astronaut, boardedPart);
                }

            }
            catch (Exception ex)
            {
                Debug.Log("[Snacks] - OnCrewBoardVessel " + ex.Message + ex.StackTrace);
            }
        }

        private void onCrewOnEva(GameEvents.FromToAction<Part, Part> data)
        {
            try
            {
                Part evaKerbal = data.to;
                Part partExited = data.from;

                if (lastEVAKerbal == evaKerbal)
                    return;
                lastEVAKerbal = evaKerbal;

                //Update the snacks EVA resource to reflect current game settings.
                SnacksEVAResource.snacksEVAResource.amount = SnacksProperties.SnacksPerMeal;
                SnacksEVAResource.snacksEVAResource.maxAmount = SnacksProperties.SnacksPerMeal;

                //Inform all eva resources
                int count = snacksEVAResources.Count;
                for (int index = 0; index < count; index++)
                    snacksEVAResources[index].onCrewEVA(evaKerbal, partExited);

                //Give consumers a chance to update their data if needed.
                ProtoCrewMember astronaut = evaKerbal.vessel.GetVesselCrew()[0];
                count = resourceProcessors.Count;
                BaseResourceProcessor consumer;
                for (int index = 0; index < count; index++)
                {
                    consumer = resourceProcessors[index];
                    consumer.onKerbalEVA(astronaut, partExited);
                }
            }
            catch (Exception ex)
            {
                Debug.Log("[Snacks] - OnCrewOnEva " + ex.Message + ex.StackTrace);
            }
        }

        private void onEditorPartPicked(Part part)
        {
            //Inform all part resources
            int count = snacksPartResources.Count;
            for (int index = 0; index < count; index++)
                snacksPartResources[index].addResourcesIfNeeded(part);
        }

        private void onEditorPartPlaced(Part part)
        {
            //Inform all part resources
            int count = snacksPartResources.Count;
            for (int index = 0; index < count; index++)
                snacksPartResources[index].addResourcesIfNeeded(part);
        }

        private void onVesselTerminated(ProtoVessel protoVessel)
        {
        }

        private void onVesselRecovered(ProtoVessel protoVessel, bool someBool)
        {
        }

        private void onLevelLoaded(GameScenes scene)
        {
        }

        private void onVesselRollout(ShipConstruct ship)
        {
            Part part;
            for (int index = 0; index < ship.parts.Count; index++)
            {
                part = ship.parts[index];

                int count = snacksPartResources.Count;
                for (int resourceIndex = 0; resourceIndex < count; resourceIndex++)
                    snacksPartResources[resourceIndex].addResourcesIfNeeded(part);
            }
        }

        private void onVesselRecoveryRequested(Vessel vessel)
        {
        }

        private void onVesselWillDestroy(Vessel vessel)
        {
        }

        private void onVesselWasModified(Vessel vessel)
        {
        }

        private void onGameSettingsApplied()
        {
            int count = resourceProcessors.Count;
            for (int index = 0; index < count; index++)
            {
                resourceProcessors[index].OnGameSettingsApplied();
            }
        }

        private void onVesselLoaded(Vessel vessel)
        {
            if (vessel.isEVA)
            {
                //Inform all eva resources
                int count = snacksEVAResources.Count;
                for (int index = 0; index < count; index++)
                    snacksEVAResources[index].addResourcesIfNeeded(vessel);
            }
            else
            {
                //Inform all part resources
                int count = snacksPartResources.Count;
                for (int index = 0; index < count; index++)
                    snacksPartResources[index].addResourcesIfNeeded(vessel);
            }
        }
        #endregion

        #region Window Management
        List<IManagedSnackWindow> managedWindows = new List<IManagedSnackWindow>();

        public void OnGUI()
        {
            int totalWindows = managedWindows.Count;
            if (totalWindows == 0)
                return;
            IManagedSnackWindow managedWindow;

            for (int index = 0; index < totalWindows; index++)
            {
                managedWindow = managedWindows[index];
                if (managedWindow.IsVisible())
                    managedWindow.DrawWindow();
            }
        }

        public void RegisterWindow(IManagedSnackWindow managedWindow)
        {
            if (managedWindows.Contains(managedWindow) == false)
                managedWindows.Add(managedWindow);
        }

        public void UnregisterWindow(IManagedSnackWindow managedWindow)
        {
            if (managedWindows.Contains(managedWindow))
                managedWindows.Remove(managedWindow);
        }
        #endregion

        #region API
        public static string FormatTime(double secondsToFormat, bool showCompact = false)
        {
            StringBuilder timeBuilder = new StringBuilder();
            double seconds = secondsToFormat;
            double years = 0;
            double days = 0;
            double hours = 0;
            double minutes = 0;

            //Make sure we have calculated our seconds per day
            GetSecondsPerDay();

            //Years
            years = Math.Floor(seconds / secondsPerYear);
            if (years >= 1.0)
            {
                seconds -= years * secondsPerYear;
            }
            else
            {
                years = 0;
            }

            //Days
            days = Math.Floor(seconds / secondsPerDay);
            if (days >= 1.0)
            {
                seconds -= days * secondsPerDay;
            }
            else
            {
                days = 0;
            }

            //Hours
            hours = Math.Floor(seconds / 3600);
            if (hours >= 1.0)
            {
                seconds -= hours * 3600;
            }
            else
            {
                seconds = 0;
            }

            //Minutes
            minutes = Math.Floor(seconds / 60);
            if (minutes >= 1.0)
            {
                seconds -= minutes * 60;
            }
            else
            {
                minutes = 0;
            }

            if (showCompact)
            {
                if (years > 0)
                    timeBuilder.Append(string.Format("Y{0:n2}:", years));
                if (days > 0)
                    timeBuilder.Append(string.Format("D{0:n2}:", days));
                if (hours > 0)
                    timeBuilder.Append(string.Format("H{0:n2}:", hours));
                if (minutes > 0)
                    timeBuilder.Append(string.Format("M{0:n2}:", minutes));
                if (seconds > 0.0001)
                    timeBuilder.Append(string.Format("S{0:n2}", seconds));
            }
            else
            {
                if (years > 0)
                    timeBuilder.Append(string.Format(" {0:n2} Years,", years));
                if (days > 0)
                    timeBuilder.Append(string.Format(" {0:n2} Days,", days));
                if (hours > 0)
                    timeBuilder.Append(string.Format(" {0:n2} Hours,", hours));
                if (minutes > 0)
                    timeBuilder.Append(string.Format(" {0:n2} Minutes,", minutes));
                if (seconds > 0.0001)
                    timeBuilder.Append(string.Format(" {0:n2} Seconds", seconds));
            }

            return timeBuilder.ToString();
        }

        public static double GetSecondsPerDay()
        {
            if (secondsPerDay > 0)
                return secondsPerDay;

            //Find homeworld
            int count = FlightGlobals.Bodies.Count;
            CelestialBody body = null;
            for (int index = 0; index < count; index++)
            {
                body = FlightGlobals.Bodies[index];
                if (body.isHomeWorld)
                    break;
                else
                    body = null;
            }
            if (body == null)
            {
                secondsPerYear = 21600 * 426.08;
                secondsPerDay = 21600;
                return secondsPerDay;
            }

            //Also get seconds per year
            secondsPerYear = body.orbit.period;

            //Return solar day length
            secondsPerDay = body.solarDayLength;
            return secondsPerDay;
        }

        public static double GetSolarFlux(Vessel vessel)
        {
            double solarFlux = 0;

            if (vessel.loaded)
            {
                //If the vessel is not on or orbiting a planet that's orbiting a star, then find the nearest parent planet that is.
                CelestialBody body = vessel.mainBody;
                while (body.referenceBody != null && body.referenceBody.scaledBody.GetComponentsInChildren<SunShaderController>(true).Length <= 0)
                    body = body.referenceBody;

                //If the vessel itself is orbiting a star then we'l use its orbit parameters instead.
                Orbit orbit = null;
                if (vessel.mainBody.scaledBody.GetComponentsInChildren<SunShaderController>(true).Length > 0)
                {
                    orbit = vessel.orbit;
                    body = vessel.mainBody;
                }
                else
                {
                    orbit = body.orbit;
                    body = body.referenceBody;
                }

                //Get solar luminosity from physics
                double solarLuminosity = PhysicsGlobals.SolarLuminosity;

                //Calculate solar luminosity for the star that the celestial body is orbiting if Kopernicus is installed.
                //solarLuminosity = Math.Pow(homeBody.orbit.semiMajorAxis, 2.0) * 4.0 * Math.PI * localStarLuminosityFromItsConfigFile
                ConfigNode[] bodyNodes = GameDatabase.Instance.GetConfigNodes("Kopernicus");
                if (bodyNodes.Length > 0 && bodyNodes[0].HasNode("Body"))
                {
                    ConfigNode node;
                    string bodyName;

                    bodyNodes = bodyNodes[0].GetNodes("Body");
                    for (int index = 0; index < bodyNodes.Length; index++)
                    {
                        node = bodyNodes[index];

                        //Find the config that matches the name of the celestial body.
                        if (!node.HasValue("name"))
                            continue;
                        bodyName = node.GetValue("name");
                        if (bodyName != body.name)
                            continue;

                        //Now get luminostiy
                        if (!node.HasNode("ScaledVersion"))
                            continue;
                        node = node.GetNode("ScaledVersion");

                        if (!node.HasNode("Light"))
                            continue;
                        node = node.GetNode("Light");

                        if (!node.HasValue("luminosity"))
                            continue;
                        double.TryParse(node.GetValue("luminosity"), out solarLuminosity);

                        //Find the homeworld. We need it's orbit parameters.
                        CelestialBody homeBody = null;
                        int bodyCount = FlightGlobals.Bodies.Count;
                        for (int bodyIndex = 0; bodyIndex < bodyCount; bodyIndex++)
                        {
                            homeBody = FlightGlobals.Bodies[bodyIndex];
                            if (homeBody.isHomeWorld)
                                break;
                        }

                        //Account for homeworlds that are moons of some planet orbiting some star.
                        while (homeBody.referenceBody != null && homeBody.referenceBody.scaledBody.GetComponentsInChildren<SunShaderController>(true).Length <= 0)
                            homeBody = homeBody.referenceBody;

                        //Now calculate solar luminosity based on homeworld's orbit
                        solarLuminosity = solarLuminosity * 4.0 * Math.PI * Math.Pow(homeBody.orbit.semiMajorAxis, 2.0);
                        break;
                    }
                }

                //Now calculate solar flux.
                solarFlux = solarLuminosity / (Math.PI * 4.0 * Math.Pow(orbit.semiMajorAxis, 2.0));
            }

            return solarFlux;
        }
        #endregion

        #region Helpers
        private IEnumerator<YieldInstruction> runSnackCycle(Vessel vessel, double elapsedTime)
        {
            yield return new WaitForFixedUpdate();

            int crewCount = 0;
            int crewCapacity = 0;
            int count;
            List<SnacksBackgroundConverter> converters;
            SnacksBackgroundConverter converter;

            //Get crew count and crew capacity
            if (vessel.loaded)
            {
                crewCount = vessel.GetCrewCount();
                crewCapacity = vessel.GetCrewCapacity();
            }
            else
            {
                ProtoVessel protoVessel = vessel.protoVessel;
                ProtoPartSnapshot protoPart;
                ConfigNode node;
                int partCapacity = 0;

                crewCount = protoVessel.GetVesselCrew().Count;

                count = protoVessel.protoPartSnapshots.Count;
                for (int index = 0; index < count; index++)
                {
                    protoPart = protoVessel.protoPartSnapshots[index];
                    node = protoPart.partInfo.partConfig;
                    if (!node.HasValue("CrewCapacity"))
                        continue;
                    partCapacity = 0;
                    int.TryParse(node.GetValue("CrewCapacity"), out partCapacity);
                    crewCapacity += partCapacity;
                }

                yield return new WaitForFixedUpdate();
            }

            //Continue processing if we have crew
            if (crewCount > 0)
            {
                //Run background converers
                if (backgroundConverters.ContainsKey(vessel) && !vessel.loaded)
                {
                    converters = backgroundConverters[vessel];
                    count = converters.Count;

                    for (int index = 0; index < count; index++)
                    {
                        converter = converters[index];

                        if (converter.IsActivated && !converter.isMissingResources && !converter.isContainerFull)
                        {
                            runConverter(converter, elapsedTime, vessel.protoVessel);
                            yield return new WaitForFixedUpdate();
                        }
                    }
                }
                yield return new WaitForFixedUpdate();

                //Run processors
                count = resourceProcessors.Count;
                for (int index = 0; index < count; index++)
                {
                    resourceProcessors[index].ProcessResources(vessel, elapsedTime, crewCount, crewCapacity);
                    yield return new WaitForFixedUpdate();
                }

                //Process events
            }

            yield return new WaitForFixedUpdate();
        }
    #endregion
}
}
