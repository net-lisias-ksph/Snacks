using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.IO;

namespace Snacks
{
    public class EVARefiller : PartModule
    {
        [KSPEvent(guiActiveUnfocused = true, unfocusedRange = 3.0f, guiName = "Refill Snack Pack")]
        public void RefillSnackPack()
        {
            if (FlightGlobals.ActiveVessel.isEVA && SnackController.IsExperienceEnabled())
            {
                Vessel vessel = FlightGlobals.ActiveVessel;
                ProtoCrewMember astronaut = vessel.GetVesselCrew()[0];
            }
        }
    }
}
