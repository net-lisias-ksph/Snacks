using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KSP;
using KSP.IO;

namespace Snacks
{
    [KSPScenario(ScenarioCreationOptions.AddToAllGames, GameScenes.SPACECENTER, GameScenes.EDITOR, GameScenes.FLIGHT, GameScenes.TRACKSTATION)]
    public class SnacksScenario : ScenarioModule
    {
        public static SnacksScenario Instance;
        public Dictionary<string, int> sciencePenalties = new Dictionary<string, int>();
        public List<string> knownVessels = new List<string>();

        public override void OnAwake()
        {
            base.OnAwake();
            Instance = this;
        }

        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);

            ConfigNode[] penalties = node.GetNodes("SCIENCE_PENALTY");
            foreach (ConfigNode penaltyNode in penalties)
            {
                sciencePenalties.Add(penaltyNode.GetValue("vesselID"), int.Parse(penaltyNode.GetValue("amount")));
            }

            ConfigNode[] KnownVessels = node.GetNodes("KNOWN_VESSEL");
            foreach (ConfigNode vesselNode in KnownVessels)
            {
                if (knownVessels.Contains(vesselNode.GetValue("vesselID")) == false)
                    knownVessels.Add(vesselNode.GetValue("vesselID"));
            }
        }

        public override void OnSave(ConfigNode node)
        {
            base.OnSave(node);
            ConfigNode configNode;
            foreach (string key in sciencePenalties.Keys)
            {
                configNode = new ConfigNode("SCIENCE_PENALTY");
                configNode.AddValue("vesselID", key);
                configNode.AddValue("amount", sciencePenalties[key].ToString());
                node.AddNode(configNode);
            }

            foreach (string vesselID in knownVessels)
            {
                configNode = new ConfigNode("KNOWN_VESSEL");
                configNode.AddValue("vesselID", vesselID);
                node.AddNode(configNode);
            }
        }
    }
}
