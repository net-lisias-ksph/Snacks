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

namespace Snacks
{
    /// <summary>
    /// This precondition checks to see if a vessel's connection status to the homeworld matches the desired value.
    /// Example definition:
    /// PRECONDITION 
    /// {
    ///     name  = CheckHomeworldConnection
    ///     hasConnection = false
    /// }
    public class CheckHomeworldConnection: BasePrecondition
    {
        #region Housekeeping
        /// <summary>
        /// Flag to indicate pressence (true) or absence (false) of the value to check.
        /// </summary>
        public bool mustExist = true;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Snacks.CheckHomeworldConnection"/> class.
        /// </summary>
        /// <param name="node">A ConfigNode containing initialization parameters. parameters from the 
        /// <see cref="T:Snacks.BasePrecondition"/> class also apply.</param>
        public CheckHomeworldConnection(ConfigNode node): base(node)
        {
            if (node.HasValue(ValueExists))
                bool.TryParse(node.GetValue(ValueExists), out mustExist);
        }
        #endregion

        #region Overrides
        public override bool IsValid(ProtoCrewMember astronaut, Vessel vessel)
        {
            if (!CommNet.CommNetScenario.CommNetEnabled)
                return true;
            if (!base.IsValid(astronaut, vessel))
                return false;

            return vessel.connection.IsConnectedHome == mustExist;
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
