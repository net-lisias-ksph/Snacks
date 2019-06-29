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
    /// This outcome reduces the space agency's reputation based on the supplied parameters.
    /// Example definition:
    /// OUTCOME 
    /// {
    ///     name  = RepPenalty
    ///     repLossPerKerbal = 5
    /// }
    /// </summary>   

    public class RepPenalty : BaseOutcome
    {
        #region Constants
        const string ValueRepLossPerKerbal = "repLossPerKerbal";
        #endregion

        #region Housekeeping
        /// <summary>
        /// The rep loss per kerbal.
        /// </summary>
        public float repLossPerKerbal = 0;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Snacks.RepPenalty"/> class.
        /// </summary>
        /// <param name="node">A ConfigNode containing initialization parameters. Parameters in the
        /// <see cref="T:Snacks.BaseOutcome"/> class also apply.</param>
        public RepPenalty(ConfigNode node) : base (node)
        {
            if (node.HasValue(ValueRepLossPerKerbal))
                float.TryParse(node.GetValue(ValueRepLossPerKerbal), out repLossPerKerbal);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Snacks.RepPenalty"/> class.
        /// </summary>
        /// <param name="canBeRandom">If set to <c>true</c> it can be randomly selected from the outcomes list.</param>
        /// <param name="repLossPerKerbal">Rep loss per kerbal.</param>
        /// <param name="playerMessage">A string containing the bad news.</param>
        public RepPenalty(bool canBeRandom, float repLossPerKerbal, string playerMessage) : base(canBeRandom)
        {
            this.repLossPerKerbal = repLossPerKerbal;
            this.playerMessage = playerMessage;
        }
        #endregion

        #region Overrides
        public override bool IsEnabled()
        {
            return SnacksProperties.LoseRepWhenHungry;
        }

        public override void ApplyOutcome(Vessel vessel, SnacksProcessorResult result)
        {
            //Only applies to Career mode
            if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER)
            {
                float repLoss = repLossPerKerbal * result.affectedKerbalCount;

                Reputation.Instance.AddReputation(-repLoss, TransactionReasons.Any);

                if (!string.IsNullOrEmpty(playerMessage))
                {
                    if (playerMessage.Contains("{0:N3}"))
                        ScreenMessages.PostScreenMessage(string.Format(playerMessage, repLoss), 5, ScreenMessageStyle.UPPER_LEFT);
                    else
                        ScreenMessages.PostScreenMessage(playerMessage, 5, ScreenMessageStyle.UPPER_LEFT);
                }
            }

            //Call the base class
            base.ApplyOutcome(vessel, result);
        }
        #endregion
    }
}
