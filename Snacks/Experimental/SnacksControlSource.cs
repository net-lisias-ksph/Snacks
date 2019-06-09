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
using KSP;
using KSP.UI;
using CommNet;

namespace Snacks
{
    public class SnacksControlSource : PartModule, ICommNetControlSource
    {
        [KSPField(isPersistant = true)]
        public bool partialControlEnabled;

        [KSPField]
        public bool debugMode;

        [KSPField]
        public VesselControlState controlState = VesselControlState.Full;

        [KSPEvent(guiName = "Toggle partial control")]
        public void TogglePartial()
        {
            partialControlEnabled = !partialControlEnabled;
        }

        public override void OnStart(StartState state)
        {
            base.OnStart(state);
            if (this.part.vessel == null)
                return;

            if (this.part.vessel.connection.CommandSources.Contains(this) == false)
                this.part.vessel.connection.RegisterCommandSource(this);
            UpdateNetwork();

            Events["TogglePartial"].guiActive = debugMode;
            Fields["controlState"].guiActive = debugMode;
        }

        public VesselControlState GetControlSourceState()
        {
            return controlState;
        }

        public bool IsCommCapable()
        {
            return true;
        }

        public void UpdateNetwork()
        {
            if (partialControlEnabled)
            {
                controlState = VesselControlState.Partial;
                this.part.vessel.maxControlLevel = Vessel.ControlLevel.PARTIAL_MANNED;
            }
            else
            {
                controlState = VesselControlState.Full;
                this.part.vessel.maxControlLevel = Vessel.ControlLevel.FULL;
            }
        }

        string ICommNetControlSource.name
        {
            get 
            {
                return "SnacksControlSource";
            }
        }
    }
}
