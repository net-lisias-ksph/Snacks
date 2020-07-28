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
using System.Text;
using UnityEngine;
using KSP.IO;

namespace Snacks
{
    /// <summary>
    /// The BaseOutcome class is the basis for all outcome processing. An outcome is used with the resource processors as well as 
    /// by the event system. It represents the consequences (or benefits) of a process result as well as the actions
    /// to take when an event's preconditions are met.
    /// </summary>
    public class BaseOutcome
    {
        #region Constants
        public const string ValueCanBeRandom = "canBeRandom";
        public const string ValuePlayerMessage = "playerMessage";
        public const string ValueSelectRandomCrew = "selectRandomCrew";
        public const string OUTCOME = "OUTCOME";
        public const string OutcomeName = "name";
        public const string ResourceName = "resourceName";
        public const string ResourceAmount = "amount";
        public const string ResourceSelectRandomCrew = "selectRandomCrew";
        public const string ResourceRandomMin = "randomMin";
        public const string ResourceRandomMax = "randomMax";
        public const string ConditionName = "conditionName";
        #endregion

        #region Housekeeping
        /// <summary>
        /// Flag to indicate whether or not the outcome can be randomly selected.
        /// Requires random outcomes to be turned on. If it isn't then the 
        /// outcome is always applied.
        /// </summary>
        public bool canBeRandom;

        /// <summary>
        /// Flag to indicate whether or not to select a random crew member for the outcome
        /// instead of applying the outcome to the entire crew.
        /// </summary>
        public bool selectRandomCrew;

        /// <summary>
        /// Optional message to display to the player.
        /// </summary>
        public string playerMessage = string.Empty;

        /// <summary>
        /// Optional list of child outcomes to apply when the parent outcome is applied.
        /// Child outcomes use same vessel/kerbal as the parent.
        /// </summary>
        public List<BaseOutcome> childOutcomes;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Snacks.BaseOutcome"/> class.
        /// </summary>
        public BaseOutcome()
        {
            childOutcomes = new List<BaseOutcome>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Snacks.BaseOutcome"/> class.
        /// </summary>
        /// <param name="node">A ConfigNode containing initialization parameters.</param>
        public BaseOutcome(ConfigNode node)
        {
            if (node.HasValue(ValueCanBeRandom))
                bool.TryParse(node.GetValue(ValueCanBeRandom), out canBeRandom);

            if (node.HasValue(ValuePlayerMessage))
                playerMessage = node.GetValue(ValuePlayerMessage);

            if (node.HasValue(ValueSelectRandomCrew))
                bool.TryParse(node.GetValue(ValueSelectRandomCrew), out selectRandomCrew);

            childOutcomes = new List<BaseOutcome>();
            if (node.HasNode(OUTCOME))
            {
                ConfigNode[] outcomeNodes = node.GetNodes(OUTCOME);
                BaseOutcome baseOutcome;
                for (int index = 0; index < outcomeNodes.Length; index++)
                {
                    baseOutcome = SnacksScenario.Instance.CreateOutcome(outcomeNodes[index]);
                    if (baseOutcome != null)
                        childOutcomes.Add(baseOutcome);
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Snacks.BaseOutcome"/> class.
        /// </summary>
        /// <param name="canBeRandom">If set to <c>true</c>, the outcome can be randomly selected.</param>
        public BaseOutcome(bool canBeRandom)
        {
            this.canBeRandom = canBeRandom;
            childOutcomes = new List<BaseOutcome>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Snacks.BaseOutcome"/> class.
        /// </summary>
        /// <param name="canBeRandom">If set to <c>true</c>, the outcome can be randomly selected.</param>
        /// <param name="playerMessage">A string containing a message to the player that is shown when the outcome
        /// is applied..</param>
        public BaseOutcome(bool canBeRandom, string playerMessage)
        {
            this.canBeRandom = canBeRandom;
            this.playerMessage = playerMessage;
            childOutcomes = new List<BaseOutcome>();
        }
        #endregion

        #region API
        /// <summary>
        /// Loads the configuration
        /// </summary>
        /// <param name="node">A ConfigNode containing data to load.</param>
        public virtual void Load(ConfigNode node)
        {

        }

        /// <summary>
        /// Indicates whether or not the outcome is enabled.
        /// </summary>
        /// <returns>true if inabled, false if not.</returns>
        public virtual bool IsEnabled()
        {
            return false;
        }

        /// <summary>
        /// Applies the outcome to the vessel's crew
        /// </summary>
        /// <param name="vessel">The Vessel being processed.</param>
        /// <param name="result">The Result of the processing attempt.</param>
        public virtual void ApplyOutcome(Vessel vessel, SnacksProcessorResult result)
        {
            int count = childOutcomes.Count;
            for (int index = 0; index < count; index++)
                childOutcomes[index].ApplyOutcome(vessel, result);
        }

        /// <summary>
        /// Removes the outcome from the vessel's crew.
        /// </summary>
        /// <param name="vessel">The Vessel to process.</param>
        /// <param name="informPlayer">A Bool indicating whether or not to inform the player.</param>
        public virtual void RemoveOutcome(Vessel vessel, bool informPlayer = true)
        {
            int count = childOutcomes.Count;
            for (int index = 0; index < count; index++)
                childOutcomes[index].RemoveOutcome(vessel, informPlayer);
        }
        #endregion
    }
}
