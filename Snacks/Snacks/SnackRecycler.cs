using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.IO;

namespace Snacks
{
    public class SnackRecycler : ModuleResourceConverter
    {
        [KSPField()]
        public float RecyclePercentage;

        [KSPField()]
        public int CrewCapacity;

        public override string GetInfo()
        {
            StringBuilder info = new StringBuilder();
            string baseInfo = base.GetInfo();

            baseInfo.Replace("Outputs:", "");
            info.AppendLine(baseInfo);
            info.AppendFormat("Recycles {0:f2}%/r/n", RecyclePercentage);
            info.AppendFormat("Supports {0:f2} crew", CrewCapacity);

            return info.ToString();
        }
    }
}
