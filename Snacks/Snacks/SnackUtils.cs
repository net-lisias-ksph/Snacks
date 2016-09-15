using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Snacks
{
    class SnackUtils
    {
        public const string kSnacksResource = "Snacks";

        public static double ConsumeSnacks(Part part, double demand)
        {
            if (part.Resources.Contains(kSnacksResource) == false)
                return 0;

            PartResource resource = part.Resources[kSnacksResource];
            double supplied = 0;

            if (resource.amount >= demand)
            {
                resource.amount -= demand;
                supplied += demand;
                return supplied;
            }
            else
            {
                supplied += resource.amount;
                demand -= resource.amount;
                resource.amount = 0;
            }

            return supplied;
        }
    }
}
