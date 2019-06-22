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
    /// <summary>
    /// Enumerator specifying the different types of events
    /// </summary>
    public enum SnacksEventCategories
    {
        /// <summary>
        /// Event is processed after the resource process cycle completes.
        /// </summary>
        categoryPostProcessCycle,

        /// <summary>
        /// The event is chosen at random once per process cycle.
        /// </summary>
        categoryEventCard,

        /// <summary>
        /// The event is processed when a kerbal levels up.
        /// </summary>
        categoryKerbalLevelUp
    }

    /// <summary>
    /// Enumerator specifying which kerbals are affected by the preconditions.
    /// </summary>
    public enum KerbalsAffectedTypes
    {
        /// <summary>
        /// A single available kerbal is chosen at random.
        /// </summary>
        affectsRandomAvailable,

        /// <summary>
        /// A single assigned kerbal is chosen at random.
        /// </summary>
        affectsRandomAssigned,

        /// <summary>
        /// All available kerbals are affected.
        /// </summary>
        affectsAllAvailable,

        /// <summary>
        /// All assigned kerbals are affected.
        /// </summary>
        affectsAllAssigned,

        /// <summary>
        /// A single random kerbal is chosesn amongst each crewed vessel.
        /// </summary>
        affectsRandomCrewPerVessel
    }

    public class SnacksEvent
    {
        #region Constants
        #endregion

        #region Housekeeping
        public SnacksEventCategories eventCategory;
        public KerbalsAffectedTypes affectedKerbals;
        #endregion

        #region Constructors
        #endregion
    }
}
