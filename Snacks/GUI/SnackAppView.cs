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
            if (HighLogic.LoadedSceneIsEditor == false)
                drawFlightWindow();
            else
                drawEditorWindow();
        }

        public void drawEditorWindow()
        {
            ShipSupply supply = SnackSnapshot.Instance().TakeEditorSnapshot();

            scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.Height(300), GUILayout.Width(300));

            if (SnacksProperties.EnableRandomSnacking || SnacksProperties.RecyclersEnabled)
                GUILayout.Label("<color=yellow>The following are estimates</color>");

            if (supply.DayEstimate > 0)
                GUILayout.Label("<color=white>Current Crew: " + supply.CrewCount + "  Duration: " + supply.DayEstimate + " days</color>");
            else
                GUILayout.Label("<color=white>Current Crew: " + supply.CrewCount + "  Duration: Indefinite</color>");

            if (supply.MaxDayEstimate > 0)
                GUILayout.Label("<color=white>Max Crew: " + supply.MaxCrewCount + "  Duration: " + supply.MaxDayEstimate + " days</color>");
            else
                GUILayout.Label("<color=white>Max Crew: " + supply.MaxCrewCount + "  Duration: Indefinite</color>");


            GUILayout.EndScrollView();
        }

        public void drawFlightWindow()
        {
            Dictionary<int, List<ShipSupply>> snapshot = SnackSnapshot.Instance().TakeSnapshot();
            var keys = snapshot.Keys.ToList();
            List<ShipSupply> supplies;

            scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.Height(300), GUILayout.Width(300));
            keys.Sort();
            foreach (int planet in keys)
            {
                if (!snapshot.TryGetValue(planet, out supplies))
                {
                    GUILayout.Label("Can't seem to get supplies");
                    GUILayout.EndScrollView();
                }

                if (SnacksProperties.EnableRandomSnacking)
                {
                    GUILayout.Label("<color=yellow>The following are estimates</color>");
                }

                GUILayout.Label("<color=white><b>" + supplies.First().BodyName + ":</b></color>");
                foreach (ShipSupply supply in supplies)
                {
                    if (supply.DayEstimate < 0)
                    {
                        GUILayout.Label("<color=white>" + supply.VesselName + ": " + supply.SnackAmount + "/" + supply.SnackMaxAmount + "</color>");
                        GUILayout.Label("<color=white>Crew: " + supply.CrewCount + "  Duration: Indefinite</color>");
                    }
                    else if (supply.Percent > 50)
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
                        GUILayout.Label("<color=red>Crew: " + supply.CrewCount + "  Duration: " + supply.DayEstimate + " days</color>");
                    }
                }
            }

            GUILayout.EndScrollView();
        }
    }
}
