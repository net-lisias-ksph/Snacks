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
    public class BaseOutcome
    {
        #region Constants
        public const string ValueCanBeRandom = "canBeRandom";
        public const string ValuePlayerMessage = "playerMessage";
        #endregion

        #region Fields
        /// <summary>
        /// Flag to indicate whether or not the outcome can be randomly selected.
        /// Requires random outcomes to be turned on. If it isn't then the 
        /// outcome is always applied.
        /// </summary>
        public bool canBeRandom;
        #endregion

        #region Housekeeping
        protected string playerMessage = string.Empty;
        #endregion

        #region Constructors
        public BaseOutcome()
        {
        }

        public BaseOutcome(ConfigNode node)
        {
            if (node.HasValue(ValueCanBeRandom))
                bool.TryParse(node.GetValue(ValueCanBeRandom), out canBeRandom);

            if (node.HasValue(ValuePlayerMessage))
                playerMessage = node.GetValue(ValuePlayerMessage);
        }

        public BaseOutcome(bool canBeRandom)
        {
            this.canBeRandom = canBeRandom;
        }

        public BaseOutcome(bool canBeRandom, string playerMessage)
        {
            this.canBeRandom = canBeRandom;
            this.playerMessage = playerMessage;
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

        }

        /// <summary>
        /// Removes the outcome from the vessel's crew.
        /// </summary>
        /// <param name="vessel">The Vessel to process.</param>
        public virtual void RemoveOutcome(Vessel vessel)
        {

        }
        #endregion
    }
}
