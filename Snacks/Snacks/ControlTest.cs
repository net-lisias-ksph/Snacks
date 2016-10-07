using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.IO;
using CommNet;

namespace Snacks
{
    public class ControlTest : PartModule, ICommNetControlSource
    {
        [KSPField(guiActive = true)]
        public Vessel.ControlLevel Maxlevel;

        [KSPField(guiActive = true)]
        public Vessel.ControlLevel CurLevel;

        [KSPEvent(guiActive = true)]
        public void SetLevel()
        {
            this.part.vessel.maxControlLevel = Vessel.ControlLevel.PARTIAL_MANNED;
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            Maxlevel = this.part.vessel.maxControlLevel;

            CurLevel = this.part.vessel.CurrentControlLevel;
        }

        public VesselControlState GetControlSourceState()
        {
            if (this.part.vessel.maxControlLevel == Vessel.ControlLevel.PARTIAL_MANNED)
                return VesselControlState.KerbalPartial;
            else
                return VesselControlState.Full;
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
