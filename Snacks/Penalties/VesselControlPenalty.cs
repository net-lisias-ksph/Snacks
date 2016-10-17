/**
The MIT License (MIT)
Copyright (c) 2014 Troy Gruetzmacher, Michael Billard

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
 * 
 * 
 * */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.IO;

namespace Snacks
{
    public class VesselControlPenalty : ISnacksPenalty
    {
        public void GameSettingsApplied()
        {
            //Disable partial control if the feature is turned off.
            if (!SnacksProperties.PartialControlWhenHungry)
            {
                Vessel[] vessels = FlightGlobals.Vessels.ToArray();

                for (int index = 0; index < vessels.Length; index++)
                    setPartialControl(vessels[index], false);
            }
        }

        public bool IsEnabled()
        {
            return SnacksProperties.LoseScienceWhenHungry;
        }

        public bool AlwaysApply()
        {
            return !SnacksProperties.RandomPenaltiesEnabled;
        }

        public void ApplyPenalty(int hungryKerbals, Vessel vessel)
        {
            applyPartialVesselControl(vessel, true);
        }

        public void RemovePenalty(Vessel vessel)
        {
            applyPartialVesselControl(vessel, false);
        }

        protected void applyPartialVesselControl(Vessel vessel, bool enablePartialControl)
        {
            //Set partial control for the vessel
            if (SnacksProperties.PartialControlWhenHungry)
                setPartialControl(vessel, enablePartialControl);
        }

        protected void setPartialControl(Vessel vessel, bool enablePartialControl)
        {
            if (vessel.loaded)
            {
                List<SnacksModuleCommand> snackVesselControllers = vessel.FindPartModulesImplementing<SnacksModuleCommand>();

                if (snackVesselControllers.Count > 0)
                {
                    SnacksModuleCommand[] controllers = snackVesselControllers.ToArray();

                    for (int index = 0; index < controllers.Length; index++)
                    {
                        controllers[index].partialControlEnabled = enablePartialControl;
                        controllers[index].UpdateControlSourceState();
                    }
                }
            }

            else
            {
                ProtoPartSnapshot[] partSnapshots = vessel.protoVessel.protoPartSnapshots.ToArray();
                ProtoPartSnapshot partSnapshot;
                ProtoPartModuleSnapshot[] moduleSnapshots;
                ProtoPartModuleSnapshot moduleSnapshot;

                for (int partIndex = 0; partIndex < partSnapshots.Length; partIndex++)
                {
                    partSnapshot = partSnapshots[partIndex];
                    moduleSnapshots = partSnapshot.modules.ToArray();

                    for (int index = 0; index < moduleSnapshots.Length; index++)
                    {
                        moduleSnapshot = moduleSnapshots[index];

                        if (moduleSnapshot.moduleName == "SnacksModuleCommand")
                            moduleSnapshot.moduleValues.SetValue("partialControlEnabled", enablePartialControl.ToString());
                    }
                }
            }

            if (enablePartialControl)
                ScreenMessages.PostScreenMessage(vessel.vesselName + ": The crew is starving and can't fly straight!", 5.0f, ScreenMessageStyle.UPPER_LEFT);
        }
    }
}
