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
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.IO;

namespace Snacks
{
    public class RepPenalty : ISnacksPenalty
    {
        public void GameSettingsApplied()
        {
        }

        public bool IsEnabled()
        {
            return SnacksProperties.LoseRepWhenHungry;
        }

        public bool AlwaysApply()
        {
            return !SnacksProperties.RandomPenaltiesEnabled;
        }

        public void RemovePenalty(Vessel vessel)
        {
        }

        public void ApplyPenalty(int hungryKerbals, Vessel vessel)
        {
            //Only applies to Career mode
            if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER)
            {
                //Apply reputation loss
                if (SnacksProperties.LoseRepWhenHungry)
                {
                    double repLoss;

                    if (Reputation.CurrentRep > 0)
                        repLoss = hungryKerbals * SnacksProperties.RepLostWhenHungry * Reputation.Instance.reputation;
                    else
                        repLoss = hungryKerbals;

                    Reputation.Instance.AddReputation(Convert.ToSingle(-1 * repLoss), TransactionReasons.Any);
                    ScreenMessages.PostScreenMessage("Your reputation decreased by " + Convert.ToInt32(repLoss), 5, ScreenMessageStyle.UPPER_LEFT);
                }
            }
        }
    }
}
