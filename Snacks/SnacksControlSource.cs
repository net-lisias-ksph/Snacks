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
        bool partial;
        ModuleCommand commandModule;

        [KSPEvent(guiActive = true)]
        public void SetPartial()
        {
            partial = true;
        }

        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            commandModule = this.part.FindModuleImplementing<ModuleCommand>();
        }

        public VesselControlState GetControlSourceState()
        {
            if (partial)
            {
                this.part.vessel.maxControlLevel = Vessel.ControlLevel.PARTIAL_MANNED;
                return VesselControlState.KerbalPartial;
            }
            else
            {
                return commandModule.VesselControlState;
            }

        }

        public bool IsCommCapable()
        {
            return true;
        }

        public void UpdateNetwork()
        {
        }
    }
}
