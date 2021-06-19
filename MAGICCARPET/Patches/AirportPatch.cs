using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MAGICCARPET
{
	[HarmonyPatch(typeof(AirportManager), "Awake")]
	class AirportPatch
	{
		// Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
		[HarmonyPostfix]
		public static void Postfix(AirportManager __instance)
		{
			/*Debug.Log("Airport Awake!");
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
							FlightLogger.Log("We got a carrier!");
							isCarrier = true;
						}
					}
				}
			}

			if (isCarrier)
			{
				FlightLogger.Log("Found carrier! Setting rigidbody");
				shipRbs.Add(__instance.gameObject.GetComponentInChildren<Rigidbody>(true));
				shipRB = __instance.gameObject.GetComponentInChildren<Rigidbody>(true);
			}*/
		}

		public static List<Rigidbody> shipRbs = new List<Rigidbody>();
		public static Rigidbody shipRB;
	}
}
