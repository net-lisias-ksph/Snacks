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
    public class FundingPenalty : BaseOutcome
    {
        #region Constants
        const string ValueFinePerKerbal = "finePerKerbal";
        #endregion

        #region Housekeeping
        double finePerKerbal;
        #endregion

        #region Constructors
        public FundingPenalty(ConfigNode node) : base (node)
        {
            if (node.HasValue(ValueFinePerKerbal))
                double.TryParse(node.GetValue(ValueFinePerKerbal), out finePerKerbal);
        }

        public FundingPenalty(bool canBeRandom, string playerMessage, double finePerKerbal) : base(canBeRandom)
        {
            this.playerMessage = playerMessage;
            this.finePerKerbal = finePerKerbal;
        }
        #endregion

        #region Overrides
        public override bool IsEnabled()
        {
            return SnacksProperties.LoseFundsWhenHungry;
        }

        public override void ApplyOutcome(Vessel vessel, ProcessedResource resource, SnacksProcessorResult result)
        {
            //Only applies to Career mode
            if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER)
            {
                //Apply funding loss
                if (SnacksProperties.LoseFundsWhenHungry)
                {
                    double fine = finePerKerbal * result.affectedKerbalCount;

                    Funding.Instance.AddFunds(-fine, TransactionReasons.Any);

                    if (!string.IsNullOrEmpty(playerMessage))
                    {
                        if (playerMessage.Contains("{0:N2}"))
                            ScreenMessages.PostScreenMessage(string.Format(playerMessage, fine), 5, ScreenMessageStyle.UPPER_LEFT);
                        else
                            ScreenMessages.PostScreenMessage(playerMessage, 5, ScreenMessageStyle.UPPER_LEFT);
                    }
                }
            }
        }
        #endregion
    }
}
