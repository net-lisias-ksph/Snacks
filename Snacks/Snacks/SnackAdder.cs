using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KSP;
using UnityEngine;
namespace Snacks
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
    public class SnackAdder : MonoBehaviour
    {
        public void Awake()
        {
            SnackConfiguration snackConfig = SnackConfiguration.Instance();
            double totalSnacks = 0.0f;
            ConfigNode snacksNode = new ConfigNode("RESOURCE");

            //Configure snacks node
            snacksNode.AddValue("name", "Snacks");
            snacksNode.AddValue("amount", "50");
            snacksNode.AddValue("maxAmount", "50");

            foreach (AvailablePart availablePart in PartLoader.LoadedPartsList)
            {
                //Add snacks to any crewable module that doesn't already have Snacks
                if (availablePart.partPrefab.CrewCapacity > 0)
                {
                    if (availablePart.partPrefab.Resources != null && availablePart.partPrefab.Resources.Contains("Snacks") == false)
                    {
                        totalSnacks = availablePart.partPrefab.CrewCapacity * snackConfig.SnacksPerCrewCapacity;
                        snacksNode.SetValue("amount", totalSnacks.ToString());
                        snacksNode.SetValue("maxAmount", totalSnacks.ToString());

                        availablePart.partPrefab.Resources.Add(snacksNode);
                    }
                }
            }
        }

    }
}
