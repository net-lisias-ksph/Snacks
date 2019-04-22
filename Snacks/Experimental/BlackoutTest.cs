/**
The MIT License (MIT)
Copyright (c) 2014-2019 by Michael Billard
Original concept by Troy Gruetzmacher

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
