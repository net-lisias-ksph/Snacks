using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.IO;

namespace Snacks
{
    public class SnacksVesselModule : VesselModule
    {
        public override Activation GetActivation()
        {
            return Activation.LoadedVessels;
        }

        public override bool ShouldBeActive()
        {
            return vessel.loaded;
        }

        protected override void OnStart()
        {
            base.OnStart();

            //Apply science penalties
            SciencePenalty.CheckSciencePenalties(vessel);

            //Apply fainting penalties
            FaintPenalty.CheckFaintKerbals(this.vessel);

            //Apply death penalties
            DeathPenalty.CheckDeadKerbals(this.vessel);
        }
    }
}
