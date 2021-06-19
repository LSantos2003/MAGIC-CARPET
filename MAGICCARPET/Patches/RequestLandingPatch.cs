using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace MAGICCARPET
{
    [HarmonyPatch(typeof(AirportManager), "PlayerRequestLanding")]
    class RequestLandingPatch
    {
        public static void Postfix(AirportManager __instance)
        {

			Debug.Log("Player requested landing! Going through airport patch");
			bool isCarrier = false;

			if (__instance.isCarrier)
			{
				bool noRunway = __instance.runways.Length == 0;

				if (!noRunway)
				{
					if (__instance.team != Teams.Enemy)
					{
						bool hasArrestor = __instance.hasArrestor;
						if (hasArrestor)
						{
							Debug.Log("We got a carrier!");
							//FlightLogger.Log("We got a carrier!");
							isCarrier = true;
						}
					}
				}
			}

			if (isCarrier)
			{
				Debug.Log("Setting requested carrier");
				requestedCarrier = __instance;
				shipOLS = __instance.gameObject.GetComponentInChildren<OpticalLandingSystem>(true);
            }
            
		}

        public static AirportManager requestedCarrier;
		public static OpticalLandingSystem shipOLS;
    }
}
