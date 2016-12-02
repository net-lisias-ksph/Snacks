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
