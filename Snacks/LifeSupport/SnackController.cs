/**
The MIT License (MIT)
Copyright (c) 2014 Troy Gruetzmacher

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
    [KSPAddon(KSPAddon.Startup.EveryScene, false)]
    public class SnackController : MonoBehaviour
    {

        public static event EventHandler SnackTime;
        public static event EventHandler PreSnackTime;

        protected double snackTime = -1;
        protected System.Random random = new System.Random();
        protected int snackFrequency;

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
                GameEvents.OnGameSettingsApplied.Add(UpdateSnackConsumption);

                UpdateSnackConsumption();
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
            {
                if (SnacksScenario.Instance.ShowWelcomeMessage == true)
                {
                    SnacksScenario.Instance.ShowWelcomeMessage = false;
                    ScreenMessages.PostScreenMessage("New to Snacks Continued? Be sure to read the KSPedia", 10f, ScreenMessageStyle.UPPER_CENTER);
                }
            } 
            
            try
            {
                if (SnacksProperties.EnableRandomSnacking)
                    snackTime = random.NextDouble() * snackFrequency + Planetarium.GetUniversalTime();
                else
                    snackTime = snackFrequency + Planetarium.GetUniversalTime();
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
                HighLogic.LoadedScene != GameScenes.SPACECENTER &&
                HighLogic.LoadedScene != GameScenes.TRACKSTATION)
                return;
            try
            {

                double currentTime = Planetarium.GetUniversalTime();

                if (currentTime > snackTime)
                {
                    if (SnacksProperties.EnableRandomSnacking)
                    {
                        System.Random rand = new System.Random();
                        snackTime = rand.NextDouble() * snackFrequency + currentTime;
                    }
                    else
                    {
                        snackTime = snackFrequency + currentTime;
                    }

                    Debug.Log("Snack time!  Next Snack Time!:" + snackTime);

                    EatSnacks();

                    SnackSnapshot.Instance().RebuildSnapshot();
                }
            }
            catch (Exception ex)
            {
                Debug.Log("Snacks - FixedUpdate: " + ex.Message + ex.StackTrace);
            }
        }
        #endregion

        #region GameEvents
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
                GameEvents.OnGameSettingsApplied.Remove(UpdateSnackConsumption);
            }
            catch (Exception ex)
            {
                Debug.Log("Snacks - OnDestroy: " + ex.Message + ex.StackTrace);
            }
        }
        #endregion

        #region Helpers

        public void UpdateSnackConsumption()
        {
//            snackFrequency = 10;
            snackFrequency = 6 * 60 * 60 * 2 / SnacksProperties.MealsPerDay;
        }

        private void EatSnacks()
        {
            try
            {
                Debug.Log("EatSnacks called");
                //Post the before snack time event
                OnPreSnackTime(EventArgs.Empty);

                double snacksMissed = 0;
                double snackDeficit;
                bool enablePartialControl;

                //Consume snacks for loaded vessels
                //Debug.Log("Loaded vessles count: " + FlightGlobals.VesselsLoaded.Count);
                foreach (Vessel v in FlightGlobals.VesselsLoaded)
                {
                    enablePartialControl = false;
                    if (v.GetCrewCount() > 0)
                    {
                        snackDeficit = consumer.ConsumeAndGetDeficit(v);
                        snacksMissed += snackDeficit;
                        if (snackDeficit > 0)
                        {
                            enablePartialControl = true;
                            //Debug.Log("No snacks for: " + v.vesselName + " crew count: " + v.GetVesselCrew().Count);
                        }
                    }

                    //Set partial control for the vessel
                    if (SnacksProperties.PartialControlWhenHungry)
                    {
                        List<SnackVesselController> snackVesselControllers = v.FindPartModulesImplementing<SnackVesselController>();

                        if (snackVesselControllers.Count > 0)
                        {
                            SnackVesselController[] controllers = snackVesselControllers.ToArray();

                            for (int index = 0; index < controllers.Length; index++)
                                controllers[index].partialControlEnabled = enablePartialControl;
                        }
                    }

                }

                //Consume snacks for unloaded vessels
                //Debug.Log("Unloaded vessels count: " + FlightGlobals.VesselsUnloaded.Count);
                foreach (Vessel unloadedVessel in FlightGlobals.VesselsUnloaded)
                {
                    enablePartialControl = false;
                    if (unloadedVessel.protoVessel.GetVesselCrew().Count > 0)
                    {
                        snackDeficit = consumer.ConsumeAndGetDeficit(unloadedVessel.protoVessel);
                        snacksMissed += snackDeficit;
                       if (snackDeficit > 0)
                        {
                            enablePartialControl = true;
                            //Debug.Log("No snacks for: " + unloadedVessel.protoVessel.vesselName + " crew count: " + unloadedVessel.protoVessel.GetVesselCrew().Count);
                        }
                    }

                    //Set partial control for the vessel
                    if (SnacksProperties.PartialControlWhenHungry)
                    {
                        ProtoPartSnapshot[] partSnapshots = unloadedVessel.protoVessel.protoPartSnapshots.ToArray();
                        ProtoPartSnapshot partSnapshot;
                        ProtoPartModuleSnapshot[] moduleSnapshots;
                        ProtoPartModuleSnapshot moduleSnapshot;

                        for (int partIndex = 0; partIndex < partSnapshots.Length; partIndex++)
                        {
                            partSnapshot = partSnapshots[partIndex];
                            moduleSnapshots = partSnapshot.modules.ToArray();

                            for (int index = 0; index < moduleSnapshots.Length; index++)
                            {
                                moduleSnapshot = moduleSnapshots[index];

                                if (moduleSnapshot.moduleName == "SnackVesselController")
                                    moduleSnapshot.moduleValues.SetValue("partialControlEnabled", enablePartialControl.ToString());
                            }
                        }
                    }
                }

                //If we've missed some meals then apply the consequences
                applyPenalties(snacksMissed);

                //Post the snack time event
                OnSnackTime(EventArgs.Empty);
                
            }
            catch (Exception ex)
            {
                Debug.Log("Snacks - EatSnacks: " + ex.Message + ex.StackTrace);
            }
        }

        protected void applyPenalties(double snacksMissed)
        {
            if (snacksMissed < 0.001)
                return;

            //Let player know how many kerbals are hungry
            int hungryKerbals = Convert.ToInt32(snacksMissed / SnacksProperties.SnacksPerMeal);
            ScreenMessages.PostScreenMessage(hungryKerbals + " Kerbals are hungry for snacks.", 5, ScreenMessageStyle.UPPER_LEFT);

            //If we're not in career mode then we're done
            if (HighLogic.CurrentGame.Mode != Game.Modes.CAREER)
                return;

            //Apply funding loss
            if (SnacksProperties.LoseFundsWhenHungry)
            {
                double fine = SnacksProperties.FinePerKerbal * hungryKerbals;

                Funding.Instance.AddFunds(-fine, TransactionReasons.Any);
                ScreenMessages.PostScreenMessage("You've been fined " + Convert.ToInt32(fine) + "Funds", 5, ScreenMessageStyle.UPPER_LEFT);
            }

            //Apply reputation loss
            if (SnacksProperties.LoseRepWhenHungry)
            {
                double repLoss;

                if (Reputation.CurrentRep > 0)
                    repLoss = hungryKerbals * SnacksProperties.RepLostWhenHungry * Reputation.Instance.reputation;
                else
                    repLoss = hungryKerbals;

                Reputation.Instance.AddReputation(Convert.ToSingle(-1 * repLoss), TransactionReasons.Any);
                ScreenMessages.PostScreenMessage("Youre reputation decreased by " + Convert.ToInt32(repLoss), 5, ScreenMessageStyle.UPPER_LEFT);
            }

        }

        #endregion

        #region Events

        public virtual void OnPreSnackTime(EventArgs e)
        {
            EventHandler handler = PreSnackTime;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public virtual void OnSnackTime(EventArgs e)
        {
            EventHandler handler = SnackTime;
            if (handler != null)
            {
                handler(this, e);
            }
        }
        #endregion
    }
}
