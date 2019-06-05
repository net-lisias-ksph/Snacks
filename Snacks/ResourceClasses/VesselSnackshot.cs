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
    public class VesselSnackshot
    {
        /// <summary>
        /// ID of the celestial body where the vessel is located.
        /// </summary>
        public int bodyID;

        /// <summary>
        /// Name of the vessel
        /// </summary>
        public string vesselName;

        /// <summary>
        /// Current number of crew in the vessel
        /// </summary>
        public int crewCount;

        /// <summary>
        /// Max number of crew in the vessel
        /// </summary>
        public int maxCrewCount;

        /// <summary>
        /// Reference to the vessel itself
        /// </summary>
        public Vessel vessel;

        /// <summary>
        /// List of resource snapshots
        /// </summary>
        public List<Snackshot> snackshots;

        /// <summary>
        /// Flag to indicate that the simulator couldn't determine if the converters were active, so it assumed that they were.
        /// </summary>
        public bool convertersAssumedActive;

        /// <summary>
        /// Returns the status of the vessel and its resources.
        /// </summary>
        /// <returns>A string containing the vessel's status.</returns>
        public virtual string GetStatusDisplay()
        {
            StringBuilder status = new StringBuilder();
            int snapshotCount = snackshots.Count;

            //Vessel name
            status.AppendLine("<color=white><b>" + vesselName + "</b></color>");

            //Crew capacity
            status.AppendLine("<color=white>Crew: " + crewCount + "/" + maxCrewCount + "</color>");

            //Resource snapshots
            for (int snapshotIndex = 0; snapshotIndex < snapshotCount; snapshotIndex++)
            {
                status.AppendLine(snackshots[snapshotIndex].GetStatusDisplay());
            }

            return status.ToString();
        }

        public VesselSnackshot()
        {
            snackshots = new List<Snackshot>();
        }
    }
}
