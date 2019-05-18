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
    /// When a kerbal goes on EVA, take this resource along and remove a corresponding amount from the vessel. Use the SNACKS_EVA_RESOURCE to define the resource to add.
    /// </summary>
    public class SnacksEVAResource
    {
        #region Fields
        /// <summary>
        /// Name of the resource
        /// </summary>
        public string resourceName = string.Empty;

        /// <summary>
        /// Amount to add
        /// </summary>
        public double amount;

        /// <summary>
        /// Max amount possible
        /// </summary>
        public double maxAmount;
        #endregion

        #region Housekeeping
        /// <summary>
        /// The EVA resource that defines how many Snacks the kerbal gets. We track this so that we can update its amount and max amount based on game settings.
        /// </summary>
        static public SnacksEVAResource snacksEVAResource;
        #endregion

        #region API
        /// <summary>
        /// Loads the SNACKS_EVA_RESOURCE config nodes, if any, and returns SnacksEVAResource objects.
        /// </summary>
        /// <returns>An array of SnacksEVAResource objects.</returns>
        public static SnacksEVAResource[] LoadEVAResources()
        {
            ConfigNode[] evaResourceNodes = GameDatabase.Instance.GetConfigNodes("SNACKS_EVA_RESOURCE");
            ConfigNode node;
            List<SnacksEVAResource> resourceList = new List<SnacksEVAResource>();
            SnacksEVAResource resource;

            //Add snacks resource. We do this internally because we need to keep track of changing game settings.
            resource = new SnacksEVAResource();
            resource.resourceName = SnacksProperties.SnacksResourceName;
            resource.amount = SnacksProperties.SnacksPerMeal;
            resource.maxAmount = SnacksProperties.SnacksPerMeal;
            SnacksEVAResource.snacksEVAResource = resource;
            resourceList.Add(resource);

            for (int index = 0; index < evaResourceNodes.Length; index++)
            {
                node = evaResourceNodes[index];

                resource = new SnacksEVAResource();
                if (node.HasValue("resourceName"))
                    resource.resourceName = node.GetValue("resourceName");
                //Skip the Snacks resource definition as we build that internally.
                if (resource.resourceName == SnacksProperties.SnacksResourceName)
                    continue;

                if (node.HasValue("amount"))
                    double.TryParse(node.GetValue("amount"), out resource.amount);
                if (node.HasValue("maxAmount"))
                    double.TryParse(node.GetValue("maxAmount"), out resource.maxAmount);

                resourceList.Add(resource);
            }

            return resourceList.ToArray();
        }

        /// <summary>
        /// Handles the crew boarded event. The resource is removed from the kerbal and added to the vessel.
        /// </summary>
        /// <param name="evaKerbal">The kerbal that is returning from EVA</param>
        /// <param name="boardedPart">The part that the kerbal boarded</param>
        public void onCrewBoardedVessel(Part evaKerbal, Part boardedPart)
        {
            PartResourceDefinitionList definitions = PartResourceLibrary.Instance.resourceDefinitions;
            PartResourceDefinition def;
            double partCurrentAmount = 0;
            double partMaxAmount = 0;

            if (string.IsNullOrEmpty(resourceName))
                return;
            if (!definitions.Contains(resourceName))
                return;

            //Get the resource amounts
            def = definitions[resourceName];
            evaKerbal.vessel.rootPart.GetConnectedResourceTotals(def.id, out partCurrentAmount, out partMaxAmount, true);

            //Add resource to the vessel
            boardedPart.vessel.rootPart.RequestResource(def.id, -partCurrentAmount);
        }

        /// <summary>
        /// Handles the crew eva event. The kerbal gains the EVA resource and the vessel loses a corresponding amount.
        /// </summary>
        /// <param name="evaKerbal">The kerbal that went on EVA</param>
        /// <param name="partExited">The part that the kerbal exited</param>
        public void onCrewEVA(Part evaKerbal, Part partExited)
        {
            PartResourceDefinitionList definitions = PartResourceLibrary.Instance.resourceDefinitions;
            PartResourceDefinition def;
            double partCurrentAmount = 0;
            double partMaxAmount = 0;

            if (string.IsNullOrEmpty(resourceName))
                return;
            if (!definitions.Contains(resourceName))
                return;

            //Make sure that we have enough resources.
            def = definitions[resourceName];
            partExited.vessel.rootPart.GetConnectedResourceTotals(def.id, out partCurrentAmount, out partMaxAmount, true);
            if (partCurrentAmount <= amount)
                return;

            //Add the resource
            evaKerbal.Resources.Add(def.name, amount, maxAmount, true, true, false, true, PartResource.FlowMode.Both);

            //Remove resource from the vessel
            partExited.vessel.rootPart.RequestResource(def.id, amount);
        }

        /// <summary>
        /// If the loaded vessel's parts with crew capacity don't have the resource
        /// </summary>
        /// <param name="vessel"></param>
        public void addResourcesIfNeeded(Vessel vessel)
        {
            if (!vessel.isEVA)
                return;
            PartResourceDefinitionList definitions = PartResourceLibrary.Instance.resourceDefinitions;
            PartResourceDefinition def;
            double partCurrentAmount = 0;
            double partMaxAmount = 0;

            if (string.IsNullOrEmpty(resourceName))
                return;
            if (!definitions.Contains(resourceName))
                return;

            //Check the vessel to see if it has the resources already.
            def = definitions[resourceName];
            vessel.rootPart.GetConnectedResourceTotals(def.id, out partCurrentAmount, out partMaxAmount, true);
            if (partMaxAmount > 0)
                return;

            //Add the resource
            vessel.rootPart.Resources.Add(def.name, amount, maxAmount, true, true, false, true, PartResource.FlowMode.Both);
        }
        #endregion
    }
}
