using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KSP.IO;
using CommNet;

namespace Snacks
{
    public class SnackVesselController : PartModule, ICommNetControlSource
    {
        [KSPField()]
        public bool debugMode;

        [KSPField(guiName = "Control State", guiActive = false)]
        public string controlState = string.Empty;

        [KSPField(isPersistant = true)]
        public bool partialControlEnabled;

        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            if (debugMode)
                Fields["controlState"].guiActive = true;
        }

        public VesselControlState GetControlSourceState()
        {
            if (partialControlEnabled)
            {
                if (debugMode)
                    controlState = VesselControlState.Partial.ToString();

                return VesselControlState.Partial;
            }
            else
            {
                if (debugMode)
                    controlState = this.part.vessel.connection.ControlState.ToString();
                return this.part.vessel.connection.ControlState;
            }
        }

        public bool  IsCommCapable()
        {
            return true;
        }

        public void  UpdateNetwork()
        {
        }

    }
}
