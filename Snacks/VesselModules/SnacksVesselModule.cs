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
using KSP.IO;

namespace Snacks
{
    public class SnacksVesselModule : VesselModule
    {
        #region Constants
        public const string SnacksVesselModuleNode = "SnacksVesselModule";
        public const string ValueSolarFlux = "solarFlux";
        public const string ValueGeeForce = "geeForce";
        public const string ValueSciencePenalties = "sciencePenalties";
        public const string ValueStaticPressure = "staticPressurekPa";
        #endregion

        /// <summary>
        /// Number of science penalties to apply when the vessel becomes active.
        /// </summary>
        public int sciencePenalties = 0;

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

        protected override void OnSave(ConfigNode node)
        {
            base.OnSave(node);

            //Record solar flux for simulations and background processing.
            node.AddValue(ValueSolarFlux, SnacksScenario.GetSolarFlux(vessel));

            node.AddValue(ValueSciencePenalties, sciencePenalties);

            node.AddValue(ValueGeeForce, vessel.graviticAcceleration.magnitude);

            node.AddValue(ValueStaticPressure, vessel.staticPressurekPa);
        }

        protected override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);

            if (node.HasValue(ValueSciencePenalties))
                int.TryParse(ValueSciencePenalties, out sciencePenalties);
        }
    }
}
