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

using UnityEngine;
namespace Snacks
{
    /// <summary>
    /// This precondition checks to see if a kerbal or vessel is in an environemnt with breathable air, and matches it with the expected parameter.
    /// The vessel's celestial body must have an atmosphere with oxygen, and the vessel altitude must be between sea level and half the atmosphere height.
    /// Example definition:
    /// PRECONDITION 
    /// {
    ///     name  = CheckBreathableAir
    ///     mustExist = false
    /// }
    /// </summary>
    public class CheckBreathableAir: BasePrecondition
    {
        #region Constants
        public const double MinSafeAtmospherePressure = 0.177;
        #endregion

        #region Housekeeping
        /// <summary>
        /// Flag to indicate pressence (true) or absence (false) of the value to check.
        /// </summary>
        public bool mustExist = true;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Snacks.CheckBreathableAir"/> class.
        /// </summary>
        /// <param name="node">A ConfigNode containing initialization parameters. parameters from the 
        /// <see cref="T:Snacks.BasePrecondition"/> class also apply.</param>
        public CheckBreathableAir(ConfigNode node): base(node)
        {
            if (node.HasValue(ValueExists))
                bool.TryParse(node.GetValue(ValueExists), out mustExist);
        }
        #endregion

        #region Overrides
        public override bool IsValid(ProtoCrewMember astronaut, Vessel vessel)
        {
            if (!base.IsValid(astronaut, vessel))
                return false;

            //Get static pressure
            double staticPressure = 0;
            if (vessel.loaded)
            {
                staticPressure = vessel.staticPressurekPa;
            }
            else
            {
                if (vessel.protoVessel.vesselModules.HasNode("SnacksVesselModule"))
                {
                    ConfigNode node = vessel.protoVessel.vesselModules.GetNode("SnacksVesselModule");
                    if (node.HasValue(SnacksVesselModule.ValueStaticPressure))
                        double.TryParse(node.GetValue(SnacksVesselModule.ValueStaticPressure), out staticPressure);
                    else
                        return false;
                }
                else
                {
                    return false;
                }

                if (vessel.protoVessel.situation == Vessel.Situations.ORBITING || vessel.protoVessel.situation == Vessel.Situations.SUB_ORBITAL || vessel.situation == Vessel.Situations.ESCAPING)
                    staticPressure = 0;
            }

            if (mustExist)
            {
                //Body must have an oxygenated atmosphere
                if (!vessel.mainBody.atmosphere || !vessel.mainBody.atmosphereContainsOxygen)
                    return false;

                //Vessel must be above sea level and in the minimum safe atmospheric pressure.
                return vessel.altitude >= 0 && staticPressure >= MinSafeAtmospherePressure;
            }
            else
            {
                //Body must not have an oxygenated atmosphere
                if (!vessel.mainBody.atmosphere || !vessel.mainBody.atmosphereContainsOxygen)
                    return true;

                //Vessel must be below sea level or below the minimum safe atmospheric pressure.
                return vessel.altitude <= 0 || staticPressure < MinSafeAtmospherePressure;
            }
        }

        public override bool IsValid(ProtoCrewMember astronaut)
        {
            Vessel vessel = SnacksScenario.Instance.FindVessel(astronaut);
            if (vessel == null)
                return false;

            return IsValid(astronaut, vessel);
        }
        #endregion

    }
}
