using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.IO;


namespace Snacks
{
    public struct ResourceOption
    {
        public string name;
        public ConfigNode[] resourceConfigs;
    }

    public class SnacksResourceSwitcher : BaseSwitcher
    {
        protected ResourceOption[] resourceOptions = null;

        public override void ToggleOption()
        {
            base.ToggleOption();

            LoadOptionResources(true);
        }

        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            //If we don't have any resources then make sure to load the defaults.
            if (this.part.Resources.Count == 0)
                LoadOptionResources();
        }

        public override ConfigNode[] GetOptionNodes(string nodeName = kOptionNode)
        {
            ConfigNode[] nodes =  base.GetOptionNodes(nodeName);
            ConfigNode node;
            ResourceOption resourceOption;
            List<ResourceOption> resourceOptionList = new List<ResourceOption>();

            for (int index = 0; index < nodes.Length; index++)
            {
                node = nodes[index];

                if (node.HasNode("RESOURCE") && node.HasValue("name"))
                {
                    resourceOption = new ResourceOption();
                    resourceOption.name = node.GetValue("name");
                    resourceOption.resourceConfigs = node.GetNodes("RESOURCE");
                    resourceOptionList.Add(resourceOption);
                }
            }
            if (resourceOptionList.Count > 0)
                resourceOptions = resourceOptionList.ToArray();

            return nodes;
        }

        public virtual void RemoveAllResources()
        {
            List<PartResource> doomedResources = new List<PartResource>();
            PartResource[] resources = this.part.Resources.ToArray<PartResource>();

            for (int index = 0; index < resources.Length; index++)
                doomedResources.Add(resources[index]);

            resources = doomedResources.ToArray();
            for (int index = 0; index < resources.Length; index++)
                RemoveResource(resources[index].resourceName);

            //Dirty the GUI
            MonoUtilities.RefreshContextWindows(this.part);
        }

        public virtual void RemoveResource(string resourceName)
        {
            PartResourceDefinitionList definitions = PartResourceLibrary.Instance.resourceDefinitions;
            int resourceID = definitions[resourceName].id;

            this.part.Resources.dict.Remove(resourceID);
        }

        protected void LoadOptionResources(bool updateSymmetryParts = false)
        {
            ConfigNode[] resourceConfigs;
            Part[] symmetryParts;
            SnacksResourceSwitcher switcher;

            //Clear our resources
            RemoveAllResources();

            //Set the option name
            Events["ToggleOption"].guiName = resourceOptions[currentOptionIndex].name;

            //Get the resource configs
            resourceConfigs = resourceOptions[currentOptionIndex].resourceConfigs;

            //Now add the resources to the part.
            for (int index = 0; index < resourceConfigs.Length; index++)
                this.part.AddResource(resourceConfigs[index]);

            //Dirty the GUI
            MonoUtilities.RefreshContextWindows(this.part);

            //Update symmetry parts
            if (updateSymmetryParts && this.part.symmetryCounterparts.Count > 0)
            {
                symmetryParts = this.part.symmetryCounterparts.ToArray();
                for (int index = 0; index < symmetryParts.Length; index++)
                {
                    switcher = symmetryParts[index].FindModuleImplementing<SnacksResourceSwitcher>();
                    switcher.currentOptionIndex = this.currentOptionIndex;
                    switcher.LoadOptionResources();
                }
            }
        }
    }
}
