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
using UnityEngine;
namespace Snacks
{
    /// <summary>
    /// This is the base class for a resource consumer. Similar to ModuleResourceConverter, the consumer will consume resources and produce resources, but it happens at the vessel level, not the part level.
    /// It's also designed to work with both loaded and unloaded vessels. Another important difference is that consumed/produced resources can occur on a per crewmember basis; a vessel with 5 crew will
    /// consume and/or produce 5 times the resources as a vessel with 1 crewmember. The configuration of a BaseResourceConsumer is done through config files.
    /// </summary>
    public class BaseResourceConsumer
    {
        #region Fields
        /// <summary>
        /// User friendly name of the resource consumer
        /// </summary>
        public string consumerName = string.Empty;

        /// <summary>
        /// Number of seconds that must pass before running the consumer.
        /// </summary>
        public double secondsPerCycle = 3600.0f;
        #endregion

        #region Housekeeping
        #endregion

        #region Event Handlers
        /// <summary>
        /// Handles vessel loaded event, for instance, adding resources that should be on the vessel.
        /// </summary>
        /// <param name="vessel"></param>
        public virtual void onVesselLoaded(Vessel vessel)
        {

        }
        #endregion

        #region API
        /// <summary>
        /// Initializes the consumer
        /// </summary>
        public virtual void Initialize()
        {

        }

        /// <summary>
        /// Determines whether or not the consumer can start its cycle. The consumer should check and record the elapsed time, accumulating the seconds until the cycle can start.
        /// </summary>
        /// <param name="elapsedTime">Number of hours that have passed.</param>
        /// <returns>true if the cycle can stop; false if not.</returns>
        public virtual bool CanStartCycle(double elapsedTime)
        {
            return true;
        }

        /// <summary>
        /// Runs the converter, consuming input resources, producing output resources, applying penalties, and updating snapshots.
        /// </summary>
        public virtual void RunConverterCycle()
        {

        }
        #endregion
    }
}
