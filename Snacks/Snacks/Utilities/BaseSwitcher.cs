using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.IO;

namespace Snacks
{
    public class BaseSwitcher : PartModule
    {
        public const string kOptionNode = "OPTION";

        [KSPField()]
        public string defaultOption = string.Empty;

        [KSPField(isPersistant = true)]
        public int currentOptionIndex = -1;

        protected string[] optionNames = null;

        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            //Get option nodes
            GetOptionNodes();

            //Set default option if needed
            checkDefaultOption();
        }

        [KSPEvent(guiActiveEditor = true, guiName = "Toggle Option")]
        public virtual void ToggleOption()
        {
            currentOptionIndex = (currentOptionIndex + 1) % optionNames.Length;
        }
        
        protected virtual void checkDefaultOption()
        {
            if (optionNames == null || optionNames.Length == 0)
                return;
            if (currentOptionIndex == -1 && string.IsNullOrEmpty(defaultOption) == false)
            {
                for (int index = 0; index < optionNames.Length; index++)
                {
                    if (optionNames[index] == defaultOption)
                    {
                        currentOptionIndex = index;
                        break;
                    }
                }
            }

            if (currentOptionIndex == -1)
                currentOptionIndex = 0;
        }

        public virtual ConfigNode[] GetOptionNodes(string nodeName = kOptionNode)
        {
            if (this.part.partInfo.partConfig == null)
                return null;
            ConfigNode[] nodes = this.part.partInfo.partConfig.GetNodes("MODULE");
            ConfigNode switcherNode = null;
            ConfigNode node = null;
            string moduleName;
            List<string> optionNamesList = new List<string>();

            //Get the switcher config node.
            for (int index = 0; index < nodes.Length; index++)
            {
                node = nodes[index];
                if (node.HasValue("name"))
                {
                    moduleName = node.GetValue("name");
                    if (moduleName == this.ClassName)
                    {
                        switcherNode = node;
                        break;
                    }
                }
            }
            if (switcherNode == null)
                return null;

            //Get the nodes we're interested in
            nodes = switcherNode.GetNodes(nodeName);

            //Get the option names
            for (int index = 0; index < nodes.Length; index++)
            {
                if (nodes[index].HasValue("name"))
                    optionNamesList.Add(nodes[index].GetValue("name"));
            }
            if (optionNamesList.Count > 0)
                optionNames = optionNamesList.ToArray();

            //Done building ship!
            return nodes;
        }
    }
}
