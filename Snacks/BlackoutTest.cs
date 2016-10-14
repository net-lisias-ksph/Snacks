using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Snacks
{
    public class BlackoutTest : PartModule
    {
        bool blackoutEnabled;

        [KSPEvent(guiActive = true)]
        public void TakeNap()
        {
            blackoutEnabled = !blackoutEnabled;

            foreach (ProtoCrewMember crew in this.part.protoModuleCrew)
            {
                //This is in seconds and is affected by timewarp.
                //When you load a vessel, check the crew and see who is out cold.
                //You'll need to calculate the time they started going out cold, the current time
                //and the nap time.
                //This does appear to be persistent. Be sure to wake up the kerbal when unloading the vessel.
                crew.SetInactive(3600 * 6);
            }
        }

        [KSPEvent(guiActive = true)]
        public void WakeUp()
        {
            blackoutEnabled = !blackoutEnabled;

            //Set inactive = false to wake the crew immediately.
            foreach (ProtoCrewMember crew in this.part.protoModuleCrew)
            {
                crew.inactive = false;
            }
        }
    }
}
