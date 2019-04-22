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
using CommNet;

namespace Snacks
{
    public class SnacksModuleCommand : ModuleCommand
    {
        [KSPField]
        public bool debugMode;

        [KSPField(isPersistant = true)]
        public bool partialControlEnabled;

        protected ModuleCommand.ModuleControlState originalModuleState;
        protected VesselControlState originalLocalVesselControl;


        [KSPEvent(guiActive = false)]
        public void TogglePartialControl()
        {
//            partialControlEnabled = !partialControlEnabled;
            partialControlEnabled = true;

            CheckPartialControl();
        }

        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            originalLocalVesselControl = localVesselControlState;
            originalModuleState = moduleState;

            foreach (BaseEvent baseEvent in Events)
                baseEvent.active = false;
            foreach (BaseField field in Fields)
            {
                field.guiActive = false;
                field.guiActiveEditor = false;
            }

            CheckPartialControl();

            if (debugMode)
            {
                Events["TogglePartialControl"].active = true;
                Events["TogglePartialControl"].guiActive = true;
            }
        }

        public override VesselControlState UpdateControlSourceState()
        {
            if (partialControlEnabled && this.part.protoModuleCrew.Count > 0)
            {
                controlSrcStatusText = "Partial";
                moduleState = ModuleControlState.PartialManned;
                return CommNet.VesselControlState.KerbalPartial;
            }

            return base.UpdateControlSourceState();
        }

        public void CheckPartialControl()
        {
            if (partialControlEnabled && this.part.protoModuleCrew.Count > 0)
            {
                controlSrcStatusText = "Partial";
                moduleState = ModuleControlState.PartialManned;
            }
        }
    }
}
