using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.IO;

namespace Snacks
{
    [KSPScenario(ScenarioCreationOptions.AddToAllGames, GameScenes.SPACECENTER, GameScenes.EDITOR, GameScenes.FLIGHT, GameScenes.TRACKSTATION)]
    public class SnacksScenario : ScenarioModule
    {
        public static SnacksScenario Instance;

        private static bool showWelcomeMessage = true;

        public override void OnAwake()
        {
            base.OnAwake();
            Instance = this;
        }

        public bool ShowWelcomeMessage
        {
            get
            {
                return showWelcomeMessage;
            }

            set
            {
                showWelcomeMessage = value;
            }
        }

        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);

            if (node.HasValue("showWelcomeMessage"))
                showWelcomeMessage = bool.Parse(node.GetValue("showWelcomeMessage"));
        }

        public override void OnSave(ConfigNode node)
        {
            base.OnSave(node);

            if (node.HasValue("showWelcomeMessage") == false)
                node.AddValue("showWelcomeMessage", showWelcomeMessage.ToString());
            else
                node.SetValue("showWelcomeMessage", showWelcomeMessage.ToString());
        }
    }
}
