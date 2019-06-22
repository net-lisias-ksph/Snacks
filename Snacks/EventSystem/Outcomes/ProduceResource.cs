using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Snacks
{
    public class ProduceResource : BaseOutcome
    {
        public string resourceName = string.Empty;
        public double amount = 0;
        protected bool isRosterResource = false;

        #region Overrides
        public ProduceResource(ConfigNode node) : base(node)
        {

        }

        public ProduceResource(string resourceName, double amount, bool canBeRandom, string playerMessage) : base(canBeRandom, playerMessage)
        {
            this.resourceName = resourceName;
            this.amount = amount;

            PartResourceDefinitionList definitions = PartResourceLibrary.Instance.resourceDefinitions;
            if (definitions.Contains(resourceName))
                isRosterResource = false;
            else
                isRosterResource = true;
        }

        public override void ApplyOutcome(Vessel vessel, SnacksProcessorResult result)
        {
            base.ApplyOutcome(vessel, result);
        }
        #endregion
    }
}
