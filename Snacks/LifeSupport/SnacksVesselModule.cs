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

            //Add our vessel ID to the list of known vessels.
            if (SnacksScenario.Instance.knownVessels.Contains(vessel.id.ToString()) == false)
                SnacksScenario.Instance.knownVessels.Add(vessel.id.ToString());

            //Apply science penalties
            SciencePenalty.CheckSciencePenalties(vessel);

            //Apply fainting penalties
            FaintPenalty.CheckFaintKerbals(this.vessel);
        }
    }
}
