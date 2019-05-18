using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.IO;

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
namespace Snacks
{
    public class SnacksConverter: ModuleResourceConverter
    {
        #region constants
        private const float kminimumSuccess = 80f;
        private const float kCriticalSuccess = 95f;
        private const float kCriticalFailure = 33f;
        private const float kDefaultHoursPerCycle = 1.0f;

        //Summary messages for lastAttempt
        protected string attemptCriticalFail = "Critical Failure";
        protected string attemptCriticalSuccess = "Critical Success";
        protected string attemptFail = "Fail";
        protected string attemptSuccess = "Success";

        //User messages for last attempt
        public float kMessageDuration = 5.0f;
        public string criticalFailMessage = "Production yield lost!";
        public string criticalSuccessMessage = "Production yield higher than expected.";
        public string failMessage = "Production yield lower than expected.";
        public string successMessage = "Production completed.";
        #endregion

        #region FX fields
        /// <summary>
        /// Name of the effect to play when the converter starts.
        /// </summary>
        [KSPField]
        public string startEffect = string.Empty;

        /// <summary>
        /// Name of the effect to play when the converter stops.
        /// </summary>
        [KSPField]
        public string stopEffect = string.Empty;

        /// <summary>
        /// Name of the effect to play while the converter is running.
        /// </summary>
        [KSPField]
        public string runningEffect = string.Empty;
        #endregion

        #region Fields
        [KSPField]
        public int minimumVesselPercentEC = 5;
        #endregion

        #region Background Processing Fields
        /// <summary>
        /// Unique ID of the converter. Used to identify it during background processing.
        /// </summary>
        [KSPField(isPersistant = true)]
        public string ID;
        #endregion

        #region Timed Resource Fields
        /// <summary>
        /// On a roll of 1 - 100, the minimum roll required to declare a successful resource yield. Set to 0 if you don't want to roll for success.
        /// </summary>
        [KSPField]
        public float minimumSuccess;

        /// <summary>
        /// On a roll of 1 - 100, minimum roll for a resource yield to be declared a critical success.
        /// </summary>
        [KSPField]
        public float criticalSuccess;

        /// <summary>
        /// On a roll of 1 - 100, the maximum roll for a resource yield to be declared a critical failure.
        /// </summary>
        [KSPField]
        public float criticalFail;

        /// <summary>
        /// How many hours to wait before producing resources defined by YIELD_RESOURCE nodes.
        /// </summary>
        [KSPField]
        public double hoursPerCycle;

        /// <summary>
        /// The time at which we started a new resource production cycle.
        /// </summary>
        [KSPField(isPersistant = true)]
        public double cycleStartTime;

        /// <summary>
        /// Current progress of the production cycle
        /// </summary>
        [KSPField(guiActive = true, guiName = "Progress", isPersistant = true)]
        public string progress = string.Empty;

        /// <summary>
        /// Results of the last production cycle attempt.
        /// </summary>
        [KSPField(guiActive = true, guiName = "Last Attempt", isPersistant = true)]
        public string lastAttempt = string.Empty;

        /// <summary>
        /// If the yield check is a critical success, multiply the units produced by this number. Default is 1.0.
        /// </summary>
        [KSPField]
        public double criticalSuccessMultiplier = 1.0;

        /// <summary>
        /// If the yield check is a failure, multiply the units produced by this number. Default is 1.0.
        /// </summary>
        [KSPField]
        public double failureMultiplier = 1.0;
        #endregion

        #region Housekeeping
        //Timekeeping for producing resources after a set amount of time.
        public double elapsedTime;
        public double secondsPerCycle = 0f;
        public List<ResourceRatio> yieldResources = new List<ResourceRatio>();
        public double inputEfficiency = 1f;
        public double outputEfficiency = 1f;
        protected bool missingResources;
        #endregion

        #region Overrides
        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);

            //Resources specific to astronauts
            //Flow mode applies
            if (node.HasNode("ASTRONAUT_INPUT"))
            {

            }
            if (node.HasNode("ASTRONAUT_OUTPUT"))
            {

            }
        }

        private void OnDestroy()
        {
            GameEvents.onVesselGoOnRails.Remove(onVesselGoOnRails);
            GameEvents.onVesselDestroy.Remove(onVesselDestroy);
            GameEvents.onGameSceneLoadRequested.Add(onGameSceneLoadRequested);
        }

        public override void OnStart(StartState state)
        {
            base.OnStart(state);
            GameEvents.onVesselGoOnRails.Add(onVesselGoOnRails);
            GameEvents.onVesselDestroy.Add(onVesselDestroy);
            GameEvents.onGameSceneLoadRequested.Add(onGameSceneLoadRequested);

            //Create unique ID if needed
            if (string.IsNullOrEmpty(ID))
                ID = Guid.NewGuid().ToString();
            else if (HighLogic.LoadedSceneIsEditor && SnacksScenario.Instance.WasRecentlyCreated(this.part))
                ResetSettings();

            //Load yield resources if needed
            loadYieldResources();
            if (yieldResources.Count == 0)
            {
                Fields["progress"].guiActive = false;
                Fields["lastAttempt"].guiActive = false;
            }

            //Update background processing
            updateBackgroundConverter();
        }

        public override void StartResourceConverter()
        {
            base.StartResourceConverter();

            if (!string.IsNullOrEmpty(runningEffect))
                this.part.Effect(startEffect, 1.0f);

            cycleStartTime = Planetarium.GetUniversalTime();
            lastUpdateTime = cycleStartTime;
            elapsedTime = 0.0f;

            PreProcessing();
            updateBackgroundConverter();
        }

        public override void StopResourceConverter()
        {
            base.StopResourceConverter();
            progress = "None";

            if (!string.IsNullOrEmpty(runningEffect))
                this.part.Effect(stopEffect, 1.0f);
            if (!string.IsNullOrEmpty(runningEffect))
                this.part.Effect(runningEffect, 0.0f);

            updateBackgroundConverter();
        }

        protected override ConversionRecipe PrepareRecipe(double deltatime)
        {
            ConversionRecipe recipe = new ConversionRecipe();
            if (!IsActivated)
                return recipe;

            try
            {
                ResourceRatio ratio;
                int count = inputList.Count;

                for (int index = 0; index < count; index++)
                {
                    ratio = new ResourceRatio(inputList[index].ResourceName, inputList[index].Ratio * inputEfficiency * deltatime, inputList[index].DumpExcess);
                    ratio.FlowMode = inputList[index].FlowMode;
                    recipe.Inputs.Add(ratio);
                }

                count = outputList.Count;
                for (int index = 0; index < count; index++)
                {
                    ratio = new ResourceRatio(outputList[index].ResourceName, outputList[index].Ratio * outputEfficiency * deltatime, outputList[index].DumpExcess);
                    ratio.FlowMode = outputList[index].FlowMode;
                    recipe.Outputs.Add(ratio);
                }

                recipe.Requirements.AddRange((IEnumerable<ResourceRatio>)reqList);
            }
            catch (Exception ex)
            {
                Debug.Log("[" + this.ClassName + "]" + "-  error when preparing recipe: " + ex);
            }

            return recipe;
        }

        protected override void PreProcessing()
        {
            base.PreProcessing();

            int specialistBonus = HasSpecialist(this.ExperienceEffect);
            float crewEfficiencyBonus = 1.0f;

            if (specialistBonus > 0)
                crewEfficiencyBonus = 1.0f + (SpecialistBonusBase + (1.0f + (float)specialistBonus) * SpecialistEfficiencyFactor);

            //Update the inputEfficiency and outputEfficiency here.
            inputEfficiency = crewEfficiencyBonus;
            outputEfficiency = crewEfficiencyBonus;
        }

        public override void FixedUpdate()
        {
            double deltaTime = GetDeltaTime();
            lastUpdateTime = Planetarium.GetUniversalTime();

            //Update status
            UpdateConverterStatus();

            //Make sure we're activated if we're always active
            if (this.AlwaysActive)
                this.IsActivated = true;

            //Run the converter
            if (IsActivated)
            {
                ConverterResults results = new ConverterResults();
                ConversionRecipe recipe;
                ResourceRatio resourceRatio;
                int count;
                PartResourceDefinitionList definitions = PartResourceLibrary.Instance.resourceDefinitions;
                PartResourceDefinition resourceDef;
                double amount = 0;
                double maxAmount = 0;
                double amountObtained = 0;
                bool infinitePropellantsEnabled = CheatOptions.InfinitePropellant;
                bool infiniteElectricity = CheatOptions.InfiniteElectricity;

                //Do the pre-processing
                status = "A-OK";
                PreProcessing();
                recipe = PrepareRecipe(deltaTime);
                count = recipe.Inputs.Count;

                //TODO: Handle required resources

                //Make sure we have room for the outputs
                count = recipe.Outputs.Count;
                for (int index = 0; index < count; index++)
                {
                    resourceRatio = recipe.Outputs[index];

                    resourceDef = definitions[resourceRatio.ResourceName];
                    this.part.GetConnectedResourceTotals(resourceDef.id, out amount, out maxAmount, true);
                    if (amount >= maxAmount)
                    {
                        status = resourceDef.displayName + " is full";
                        if (AutoShutdown)
                            StopResourceConverter();
                        return;
                    }
                }

                //Make sure we have enough of the inputs
                count = recipe.Inputs.Count;
                for (int index = 0; index < count; index++)
                {
                    resourceRatio = recipe.Inputs[index];

                    //Skip resource if the appropriate cheat is on
                    if ((resourceRatio.ResourceName == "ElectricCharge" && infiniteElectricity) || (infinitePropellantsEnabled))
                        continue;

                    //Make sure we have enough of the resource
                    resourceDef = definitions[resourceRatio.ResourceName];
                    this.part.GetConnectedResourceTotals(resourceDef.id, out amount, out maxAmount, true);
                    if (amount < resourceRatio.Ratio)
                    {
                        status = "Missing " + resourceDef.displayName;
                        if (AutoShutdown)
                            StopResourceConverter();
                        return;
                    }

                    //Check for mininum EC
                    else if (resourceRatio.ResourceName == "ElectricCharge" && (amount / maxAmount) <= (minimumVesselPercentEC / 100.0f))
                    {
                        status = "Needs more " + resourceDef.displayName;
                        return;
                    }
                }

                //Now process the inputs.
                for (int index = 0; index < count; index++)
                {
                    resourceRatio = recipe.Inputs[index];
                    resourceDef = definitions[resourceRatio.ResourceName];

                    //Skip resource if the appropriate cheat is on
                    if ((resourceRatio.ResourceName == "ElectricCharge" && infiniteElectricity) || (infinitePropellantsEnabled))
                        continue;

                    this.part.RequestResource(resourceDef.id, resourceRatio.Ratio, resourceRatio.FlowMode);
                }

                //Process the outputs
                count = recipe.Outputs.Count;
                for (int index = 0; index < count; index++)
                {
                    resourceRatio = recipe.Outputs[index];
                    resourceDef = definitions[resourceRatio.ResourceName];

                    amountObtained = this.part.RequestResource(resourceDef.id, -resourceRatio.Ratio, resourceRatio.FlowMode);
                    if (amountObtained >= maxAmount)
                    {
                        status = resourceDef.displayName + " is full";
//                        if ((resourceRatio.DumpExcess == false || AutoShutdown))
//                            StopResourceConverter();
                    }
                }

                //Process astronaut inputs

                //Process astronaut outputs

                //Post process the results
                results.Status = status;
                results.TimeFactor = deltaTime;
                PostProcess(results, deltaTime);
            }

            //Cleanup
            CheckForShutdown();
            PostUpdateCleanup();
        }

        protected override void PostProcess(ConverterResults result, double deltaTime)
        {
            base.PostProcess(result, deltaTime);

            if (FlightGlobals.ready == false)
                return;
            if (HighLogic.LoadedSceneIsFlight == false)
                return;
            if (ModuleIsActive() == false)
                return;
            if (hoursPerCycle == 0f)
                return;
            if (yieldResources.Count == 0)
                return;
            if (!IsActivated)
                return;

            //Play the runningEffect
            if (!string.IsNullOrEmpty(runningEffect))
                this.part.Effect(runningEffect, 1.0f);

            //Check cycle start time
            if (cycleStartTime == 0f)
            {
                cycleStartTime = Planetarium.GetUniversalTime();
                lastUpdateTime = cycleStartTime;
                elapsedTime = 0.0f;
                return;
            }

            //If we're missing resources then we're done.
            if (!string.IsNullOrEmpty(result.Status))
            {
                if (result.Status.ToLower().Contains("missing"))
                {
                    status = result.Status;
                    missingResources = true;
                    return;
                }
            }

            //Calculate elapsed time
            elapsedTime = Planetarium.GetUniversalTime() - cycleStartTime;

            //Calculate progress
            CalculateProgress();

            //If we've elapsed time cycle then perform the analyis.
            float completionRatio = (float)(elapsedTime / secondsPerCycle);
            if (completionRatio > 1.0f && !missingResources)
            {
                int cyclesSinceLastUpdate = Mathf.RoundToInt(completionRatio);
                int currentCycle;
                for (currentCycle = 0; currentCycle < cyclesSinceLastUpdate; currentCycle++)
                {
                    PerformAnalysis();

                    //Reset start time
                    cycleStartTime = Planetarium.GetUniversalTime();
                }
            }

            //Update status
            if (yieldResources.Count > 0)
                status = "Progress: " + progress;
            else if (string.IsNullOrEmpty(status))
                status = "Running";
        }
        #endregion

        #region Yield Resources
        protected void loadYieldResources()
        {
            if (this.part.partInfo.partConfig == null)
                return;
            ConfigNode[] nodes = this.part.partInfo.partConfig.GetNodes("MODULE");
            ConfigNode converterNode = null;
            ConfigNode node = null;
            string moduleName;
            List<string> optionNamesList = new List<string>();

            //Get the switcher config node.
            for (int index = 0; index < nodes.Length; index++)
            {
                node = nodes[index];
                if (node.HasValue("name"))
                {
                    moduleName = node.GetValue("name");
                    if (moduleName == this.ClassName)
                    {
                        converterNode = node;
                        break;
                    }
                }
            }
            if (converterNode == null)
                return;

            //Get the nodes we're interested in
            nodes = converterNode.GetNodes("YIELD_RESOURCE");
            if (nodes.Length == 0)
                return;

            //Ok, start processing the yield resources
            yieldResources.Clear();
            ResourceRatio yieldResource;
            string resourceName;
            double amount;
            for (int index = 0; index < nodes.Length; index++)
            {
                node = nodes[index];
                if (!node.HasValue("ResourceName"))
                    continue;
                resourceName = node.GetValue("ResourceName");

                if (!node.HasValue("Ratio"))
                    continue;
                if (!double.TryParse(node.GetValue("Ratio"), out amount))
                    continue;

                yieldResource = new ResourceRatio(resourceName, amount, true);
                yieldResource.FlowMode = ResourceFlowMode.ALL_VESSEL;

                yieldResources.Add(yieldResource);
            }
        }
        public virtual void PerformAnalysis()
        {
            //If we have no minimum success then just produce the yield resources.
            if (minimumSuccess <= 0.0f)
            {
                produceYieldResources(1.0);
                return;
            }

            //Ok, go through the analysis.
            float analysisRoll = performAnalysisRoll();

            if (analysisRoll <= criticalFail)
                onCriticalFailure();

            else if (analysisRoll >= criticalSuccess)
                onCriticalSuccess();

            else if (analysisRoll >= minimumSuccess)
                onSuccess();

            else
                onFailure();

        }

        protected virtual float performAnalysisRoll()
        {
            float roll = 0.0f;

            //Roll 3d6 to approximate a bell curve, then convert it to a value between 1 and 100.
            roll = UnityEngine.Random.Range(1, 6);
            roll += UnityEngine.Random.Range(1, 6);
            roll += UnityEngine.Random.Range(1, 6);
            roll *= 5.5556f;

            //Done
            return roll;
        }

        protected virtual void onCriticalFailure()
        {
            lastAttempt = attemptCriticalFail;

            StopResourceConverter();

            //Show user message
            ScreenMessages.PostScreenMessage(ConverterName + ": " + criticalFailMessage, kMessageDuration);
        }

        protected virtual void onCriticalSuccess()
        {
            lastAttempt = attemptCriticalSuccess;
            produceYieldResources(criticalSuccessMultiplier);

            //Show user message
            ScreenMessages.PostScreenMessage(ConverterName + ": " + criticalSuccessMessage, kMessageDuration);
        }

        protected virtual void onFailure()
        {
            lastAttempt = attemptFail;
            produceYieldResources(failureMultiplier);

            //Show user message
            ScreenMessages.PostScreenMessage(ConverterName + ": " + failMessage, kMessageDuration);
        }

        protected virtual void onSuccess()
        {
            lastAttempt = attemptSuccess;
            produceYieldResources(1.0);

            //Show user message
            ScreenMessages.PostScreenMessage(successMessage, kMessageDuration);
        }

        protected virtual void produceYieldResources(double yieldMultiplier)
        {
            int count = yieldResources.Count;
            ResourceRatio resourceRatio;
            double yieldAmount = 0;
            string resourceName;
            double highestSkill = 0;

            //Find highest skill bonus
            if (UseSpecialistBonus && !string.IsNullOrEmpty(ExperienceEffect))
            {
                List<ProtoCrewMember> crewMembers = this.part.vessel.GetVesselCrew();

                int crewCount = crewMembers.Count;
                for (int index = 0; index < crewCount; index++)
                {
                    if (crewMembers[index].HasEffect(ExperienceEffect))
                    {
                        if (crewMembers[index].experienceLevel > highestSkill)
                            highestSkill = crewMembers[index].experienceTrait.CrewMemberExperienceLevel();
                    }
                }
            }

            //Produce the yield resources
            for (int index = 0; index < count; index++)
            {
                yieldAmount = 0;
                resourceRatio = yieldResources[index];

                resourceName = resourceRatio.ResourceName;
                yieldAmount = resourceRatio.Ratio * (1.0 + (highestSkill * SpecialistEfficiencyFactor)) * yieldMultiplier;

                this.part.RequestResource(resourceName, -yieldAmount, resourceRatio.FlowMode);
            }
        }

        public virtual void CalculateProgress()
        {
            //Get elapsed time (seconds)
            progress = string.Format("{0:f1}%", ((elapsedTime / secondsPerCycle) * 100));
        }
        #endregion

        #region Background Processing
        public virtual void ResetSettings()
        {
            //Create UUID
            ID = Guid.NewGuid().ToString();
        }

        protected void updateBackgroundConverter()
        {
            if (!HighLogic.LoadedSceneIsFlight)
                return;
            if (this.part.vessel == null)
                return;
            SnacksBackgroundConverter backgroundConverter = SnacksScenario.Instance.GetBackgroundConverter(this);
            if (this.part.vessel.isActiveVessel)
                PreProcessing();
            if (backgroundConverter != null)
            {
                //Update vessel ID as that may have changed
                backgroundConverter.vesselID = this.part.vessel.id.ToString();

                //Reset the background processing flags since conditions may have changed.
                backgroundConverter.IsActivated = this.IsActivated;
                backgroundConverter.isMissingResources = false;
                backgroundConverter.isContainerFull = false;
                backgroundConverter.inputEfficiency = inputEfficiency;
                backgroundConverter.outputEfficiency = outputEfficiency;
                backgroundConverter.moduleName = this.ClassName;

                SnacksScenario.Instance.UpdateBackgroundConverter(backgroundConverter);
            }

            //Background converter doesn't exist so create it.
            else
            {
                SnacksScenario.Instance.RegisterBackgroundConverter(this);
            }
        }

        protected void onGameSceneLoadRequested(GameScenes scene)
        {
            if (this.part.vessel != null)
                updateBackgroundConverter();
        }

        protected void onVesselGoOnRails(Vessel vessel)
        {
            if (vessel == this.part.vessel)
                updateBackgroundConverter();
        }

        protected void onVesselDestroy(Vessel vessel)
        {
            if (vessel == this.part.vessel)
            {
                SnacksScenario.Instance.UnregisterBackgroundConverter(this);
            }
        }

        protected void registerForBackgroundProcessing()
        {
            SnacksScenario.Instance.RegisterBackgroundConverter(this);
        }

        protected void unregisterForBackgroundProcessing()
        {
            SnacksScenario.Instance.UnregisterBackgroundConverter(this);
        }
        #endregion
    }
}
