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
