/**
The MIT License (MIT)
Copyright (c) 2014 Troy Gruetzmacher, Michael Billard

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
 * 
 * 
 * */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.IO;

namespace Snacks
{
    public struct SnackConsumption
    {
        public double demand;
        public double fed;
        public Vessel vessel;
    }

    public interface ISnacksPenalty
    {
        bool IsEnabled();
        bool AlwaysApply();
        void ApplyPenalty(int hungryKerbals, Vessel vessel);
        void RemovePenalty(Vessel vessel);
        void GameSettingsApplied();
    }

    [KSPAddon(KSPAddon.Startup.EveryScene, false)]
    public class SnackController : MonoBehaviour
    {
        public static SnackController Instance;
        public static EventVoid onSnackTime = new EventVoid("OnSnackTime");
        public static EventVoid onBeforeSnackTime = new EventVoid("OnBeforeSnackTime");
        public static EventVoid onSnackTick = new EventVoid("OnSnackTick");
        public static EventData<Vessel, int> onKerbalsMissedMeal = new EventData<Vessel, int>("OnKerbalsMissedMeal");
        public static EventData<SnackConsumption> onConsumeSnacks = new EventData<SnackConsumption>("OnConsumeSnacks");

        public  double nextSnackTime = -1;

        protected System.Random random = new System.Random();
        protected int snackFrequency;
        protected List<ISnacksPenalty> penaltyHandlers = new List<ISnacksPenalty>();

        private SnackConsumer consumer = new SnackConsumer();

        #region Lifecycle
        void Awake()
        {
            if (HighLogic.LoadedScene != GameScenes.FLIGHT &&
                HighLogic.LoadedScene != GameScenes.SPACECENTER &&
                HighLogic.LoadedScene != GameScenes.TRACKSTATION)
                return;

            try
            {
                GameEvents.onCrewOnEva.Add(OnCrewOnEva);
                GameEvents.onCrewBoardVessel.Add(OnCrewBoardVessel);
                GameEvents.onGameStateLoad.Add(onLoad);
                GameEvents.onVesselRename.Add(OnRename);
                GameEvents.onVesselChange.Add(OnVesselChange);
                GameEvents.onVesselWasModified.Add(OnVesselWasModified);
                GameEvents.OnGameSettingsApplied.Add(GameSettingsApplied);
                GameEvents.onVesselLoaded.Add(onVesselLoaded);
                GameEvents.onVesselRecovered.Add(onVesselRecovered);
                GameEvents.onVesselWillDestroy.Add(onVesselWillDestroy);
                GameEvents.onVesselGoOffRails.Add(onVesselLoaded);
                GameEvents.onVesselPartCountChanged.Add(OnVesselWasModified);
                Instance = this;
            }
            catch (Exception ex)
            {
                Debug.Log("Snacks - Awake error: " + ex.Message + ex.StackTrace);
            }
            
        }

        void Start()
        {
            if (HighLogic.LoadedScene != GameScenes.FLIGHT &&
                HighLogic.LoadedScene != GameScenes.SPACECENTER &&
                HighLogic.LoadedScene != GameScenes.TRACKSTATION)
                return;

            if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
                checkAndShowWelcomeMessage();
            
            try
            {
                //register penalty handlers
                penaltyHandlers.Add(new MissingMealsMonitor());
                penaltyHandlers.Add(new FundingPenalty());
                penaltyHandlers.Add(new RepPenalty());
                penaltyHandlers.Add(new SciencePenalty());
//                penaltyHandlers.Add(new VesselControlPenalty());
                penaltyHandlers.Add(new FaintPenalty());

                //Setup the inital settings.
                GameSettingsApplied();

                //Calculate next snack time
                if (SnacksProperties.EnableRandomSnacking)
                    nextSnackTime = random.NextDouble() * snackFrequency + Planetarium.GetUniversalTime();
                else
                    nextSnackTime = snackFrequency + Planetarium.GetUniversalTime();

                //To handle installations to existing saves, be sure to register the crew of existing but unloaded vessels.
                Vessel[] unloadedVessels = FlightGlobals.VesselsUnloaded.ToArray();
                for (int index = 0; index < unloadedVessels.Length; index++)
                    SnacksScenario.Instance.RegisterCrew(unloadedVessels[index]);
            }
            catch (Exception ex)
            {
                Debug.Log("Snacks - Start error: " + ex.Message + ex.StackTrace);
            }
        }

        void FixedUpdate()
        {
            if (HighLogic.LoadedScene != GameScenes.SPACECENTER &&
                HighLogic.LoadedScene != GameScenes.FLIGHT &&
                HighLogic.LoadedScene != GameScenes.TRACKSTATION)
                return;
            try
            {

                double currentTime = Planetarium.GetUniversalTime();

                if (currentTime > nextSnackTime)
                {
                    //Setup next snacking time.
                    if (SnacksProperties.EnableRandomSnacking)
                    {
                        System.Random rand = new System.Random();
                        nextSnackTime = rand.NextDouble() * snackFrequency + currentTime;
                    }
                    else
                    {
                        nextSnackTime = snackFrequency + currentTime;
                    }

                    Debug.Log("Snack time!  Next Snack Time!:" + nextSnackTime);

                    //Eat snacks!
                    EatSnacks();

                    //Update the snapshot
                    SnackSnapshot.Instance().RebuildSnapshot();

                    //Fire snack tick event
                    onSnackTick.Fire();
                }
            }
            catch (Exception ex)
            {
                Debug.Log("[Snacks] - FixedUpdate: " + ex.Message + ex.StackTrace);
            }
        }
        #endregion

        #region GameEvents
        private void onVesselLoaded(Vessel vessel)
        {
        }

        private void onVesselRecovered(ProtoVessel protoVessel, bool someBool)
        {
            //Unregister the vessel
            if (SnacksScenario.Instance.knownVessels.Contains(protoVessel.vesselID.ToString()))
                SnacksScenario.Instance.knownVessels.Remove(protoVessel.vesselID.ToString());

            //Unregister the crew
            SnacksScenario.Instance.ClearMissedMeals(protoVessel);
            SnacksScenario.Instance.UnregisterCrew(protoVessel);
        }

        private void onVesselWillDestroy(Vessel vessel)
        {
            //Unregister the vessel
            if (SnacksScenario.Instance.knownVessels.Contains(vessel.id.ToString()))
                SnacksScenario.Instance.knownVessels.Remove(vessel.id.ToString());

            //Unregister the crew
            SnacksScenario.Instance.ClearMissedMeals(vessel);
            SnacksScenario.Instance.UnregisterCrew(vessel);
        }

        private void OnVesselWasModified(Vessel data)
        {
            //Debug.Log("OnVesselWasModified");
            SnackSnapshot.Instance().RebuildSnapshot();
        }

        private void OnVesselChange(Vessel data)
        {
            //Debug.Log("OnVesselChange");
            SnackSnapshot.Instance().RebuildSnapshot();
        }

        private void OnRename(GameEvents.HostedFromToAction<Vessel, string> data)
        {
            //Debug.Log("OnRename");
            SnackSnapshot.Instance().RebuildSnapshot();
        }

        private void onLoad(ConfigNode node)
        {
            //Debug.Log("onLoad");
            SnackSnapshot.Instance().RebuildSnapshot();
        }

        private void OnCrewBoardVessel(GameEvents.FromToAction<Part, Part> data)
        {
            try
            {
                Part evaKerbal = data.from;
                Part boardedPart = data.to;
                double kerbalSnacks = consumer.GetSnackResource(evaKerbal, 1.0);
                boardedPart.RequestResource(SnacksProperties.SnackResourceID, -kerbalSnacks, ResourceFlowMode.ALL_VESSEL);
                SnackSnapshot.Instance().RebuildSnapshot();
            }
            catch (Exception ex)
            {
                Debug.Log("Snacks - OnCrewBoardVessel: " + ex.Message + ex.StackTrace);
            }
        }

        private void OnCrewOnEva(GameEvents.FromToAction<Part, Part> data)
        {
            try
            {
                Part evaKerbal = data.to;
                Part partExited = data.from;
                double snacksAmount = consumer.GetSnackResource(partExited, 1.0);

                if (evaKerbal.Resources.Contains(SnacksProperties.SnackResourceID) == false)
                {
                    ConfigNode node = new ConfigNode("RESOURCE");
                    node.AddValue("name", "Snacks");
                    node.AddValue("maxAmount", "1");
                    evaKerbal.Resources.Add(node);
                }
                evaKerbal.Resources[SnacksProperties.SnacksResourceName].amount = snacksAmount;
                SnackSnapshot.Instance().RebuildSnapshot();
            }
            catch (Exception ex)
            {
                Debug.Log("Snacks - OnCrewOnEva " + ex.Message + ex.StackTrace);
            }

        }

        void OnDestroy()
        {
            try
            {
                GameEvents.onCrewOnEva.Remove(OnCrewOnEva);
                GameEvents.onCrewBoardVessel.Remove(OnCrewBoardVessel);
                GameEvents.onGameStateLoad.Remove(onLoad);
                GameEvents.onVesselRename.Remove(OnRename);
                GameEvents.onVesselChange.Remove(OnVesselChange);
                GameEvents.onVesselWasModified.Remove(OnVesselWasModified);
                GameEvents.OnGameSettingsApplied.Remove(GameSettingsApplied);
                GameEvents.onVesselLoaded.Remove(onVesselLoaded);
                GameEvents.onVesselRecovered.Remove(onVesselRecovered);
                GameEvents.onVesselWillDestroy.Remove(onVesselWillDestroy);
                GameEvents.onVesselGoOffRails.Remove(onVesselLoaded);
            }
            catch (Exception ex)
            {
                Debug.Log("Snacks - OnDestroy: " + ex.Message + ex.StackTrace);
            }
        }
        #endregion

        #region API
        public void RegisterPenaltyHandler(ISnacksPenalty handler)
        {
            if (penaltyHandlers.Contains(handler) == false)
                penaltyHandlers.Add(handler);
        }

        public void UnregisterPenaltyHandler(ISnacksPenalty handler)
        {
            if (penaltyHandlers.Contains(handler))
                penaltyHandlers.Remove(handler);
        }
        #endregion

        #region Helpers

        protected void checkAndShowWelcomeMessage()
        {
            string settingsPath = AssemblyLoader.loadedAssemblies.GetPathByType(typeof(SnackController)) + "/Settings.cfg";
            ConfigNode nodeSettings = ConfigNode.Load(settingsPath);
            bool haveShownWelcome = false;

            if (nodeSettings == null)
            {
                nodeSettings = new ConfigNode("SNACKS");
                nodeSettings.AddValue("haveShownWelcome", "false");
                nodeSettings.Save(settingsPath);
            }

            if (nodeSettings.HasValue("haveShownWelcome"))
                haveShownWelcome = bool.Parse(nodeSettings.GetValue("haveShownWelcome"));

            if (!haveShownWelcome)
            {
                nodeSettings.SetValue("haveShownWelcome", "true");
                nodeSettings.Save(settingsPath);
                ScreenMessages.PostScreenMessage("New to Snacks Continued? Be sure to read the KSPedia", 10f, ScreenMessageStyle.UPPER_CENTER);
            }
        }

        public void GameSettingsApplied()
        {
//            snackFrequency = 5;
            snackFrequency = 6 * 60 * 60 * 2 / SnacksProperties.MealsPerDay;

            //Make sure that the penalties know about the update
            foreach (ISnacksPenalty handler in penaltyHandlers)
                handler.GameSettingsApplied();
        }

        private void EatSnacks()
        {
            try
            {
                double snackDeficit;
                int crewCount = 0;

                //Post the before snack time event
                onBeforeSnackTime.Fire();

                //Consume snacks for loaded vessels
                //Debug.Log("Loaded vessles count: " + FlightGlobals.VesselsLoaded.Count);
                foreach (Vessel vessel in FlightGlobals.Vessels)
                {
                    snackDeficit = 0;

                    //Consume snacks and get the deficit if any.
                    if (vessel.loaded)
                        crewCount = vessel.GetCrewCount();
                    else
                        crewCount = vessel.protoVessel.GetVesselCrew().Count;

                    if (crewCount > 0)
                    {
                        snackDeficit = consumer.ConsumeAndGetDeficit(vessel);

                        //Apply penalties if we have a deficit
                        if (snackDeficit > 0)
                            applyPenalties(snackDeficit, vessel);

                        //Make sure to remove all penalties, the kerbals are all fed.
                        else
                            removePenalties(vessel);
                    }
                }

                //Post the snack time event
                onSnackTime.Fire();
                
            }
            catch (Exception ex)
            {
                Debug.Log("Snacks - EatSnacks: " + ex.Message + ex.StackTrace);
            }
        }

        protected void removePenalties(Vessel vessel)
        {
            ISnacksPenalty[] penalties = penaltyHandlers.ToArray();

            for (int index = 0; index < penalties.Length; index++)
                penalties[index].RemovePenalty(vessel);
        }

        protected void applyPenalties(double snacksMissed, Vessel vessel)
        {
            if (snacksMissed < 0.001)
                return;

            int hungryKerbals = Convert.ToInt32(snacksMissed / SnacksProperties.SnacksPerMeal);
            List<ISnacksPenalty> randomPenalties = new List<ISnacksPenalty>();
            ISnacksPenalty[] penalties;

            //Let player know how many kerbals are hungry
            if (vessel.loaded)
                ScreenMessages.PostScreenMessage(vessel.vesselName + ": " + hungryKerbals + " Kerbals are hungry for snacks.", 5, ScreenMessageStyle.UPPER_LEFT);
            else
                ScreenMessages.PostScreenMessage(vessel.protoVessel.vesselName + ": " + hungryKerbals + " Kerbals are hungry for snacks.", 5, ScreenMessageStyle.UPPER_LEFT);

            //Apply the penalties
            penalties = penaltyHandlers.ToArray();
            for (int index = 0; index < penalties.Length; index++)
            {
                //If we should always apply the penalty then do so
                if (!SnacksProperties.RandomPenaltiesEnabled || penalties[index].AlwaysApply())
                    penalties[index].ApplyPenalty(hungryKerbals, vessel);

                //Add the penalty to the list of random penalties
                else
                    randomPenalties.Add(penalties[index]);
            }

            //If we have random penalties to apply then do so
            if (randomPenalties.Count > 0)
            {
                int penaltyIndex = random.Next(0, randomPenalties.Count);
                if (penaltyIndex == randomPenalties.Count)
                    penaltyIndex = randomPenalties.Count - 1;

                randomPenalties[penaltyIndex].ApplyPenalty(hungryKerbals, vessel);
            }

            //Send the bad news
            onKerbalsMissedMeal.Fire(vessel, hungryKerbals);
        }

        public static void Log(string info)
        {
            Debug.Log(info);
        }

        #endregion
    }
}
