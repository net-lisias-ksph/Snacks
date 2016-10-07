using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.IO;
using KSP.UI.Screens;

namespace Snacks
{
    public class SnackAppView : Window<SnackAppView>
    {
        private Vector2 scrollPos = new Vector2();

        public SnackAppView() :
        base("Snack Supply", 300, 300)
        {
            Resizable = false;
        }

        protected override void DrawWindowContents(int windowId)
        {
            Dictionary<int, List<ShipSupply>> snapshot = SnackSnapshot.Instance().Vessels();
            var keys = snapshot.Keys.ToList();

            scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.Height(300), GUILayout.Width(300));
            keys.Sort();
            foreach (int planet in keys)
            {
                List<ShipSupply> supplies;
                snapshot.TryGetValue(planet, out supplies);
                //supplies.Sort();
                GUILayout.Label(supplies.First().BodyName + ":");
                foreach (ShipSupply supply in supplies)
                {
                    if (supply.Percent > 50)
                    {
                        GUILayout.Label(supply.VesselName + ": " + supply.SnackAmount + "/" + supply.SnackMaxAmount);
                        GUILayout.Label("Crew: " + supply.CrewCount + "  Duration: " + supply.DayEstimate + " days");
                    }
                    else if (supply.Percent > 25)
                    {
                        GUILayout.Label("<color=yellow>" + supply.VesselName + ": " + supply.SnackAmount + "/" + supply.SnackMaxAmount + "</color>");
                        GUILayout.Label("<color=yellow>Crew: " + supply.CrewCount + "  Duration: " + supply.DayEstimate + " days</color>");
                    }
                    else
                    {
                        GUILayout.Label("<color=red>" + supply.VesselName + ": " + supply.SnackAmount + "/" + supply.SnackMaxAmount + "</color>");
                        GUILayout.Label("<color=red>Crew: " + supply.CrewCount + "  Duration: " + supply.DayEstimate + " days</red>");
                    }
                }
            }
            
            GUILayout.EndScrollView();
        }
    }
}
