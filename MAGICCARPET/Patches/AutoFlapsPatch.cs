using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAGICCARPET
{

    [HarmonyPatch(typeof(AutoFlaps), "Update")]
    class AutoFlapsPatch
    {
        public static bool apEnabled = false;
        public static bool Prefix()
        {
            return !apEnabled;
        }
    }
}
