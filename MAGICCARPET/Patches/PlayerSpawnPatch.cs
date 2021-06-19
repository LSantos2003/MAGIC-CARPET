using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MAGICCARPET.Patches
{

    [HarmonyPatch(typeof(Actor), "Start")]
    class PlayerSpawnPatch
    {
        public static void Postfix(Actor __instance)
        {
            if (__instance.isPlayer && VTOLAPI.GetPlayersVehicleEnum() == VTOLVehicles.FA26B)
            {
                GameObject go = __instance.gameObject;

                FlightLogger.Log("Player spawned, adding magic carpet");
                Carpet magicCarpet = go.AddComponent<Carpet>();
                magicCarpet.flightInfo = go.GetComponentInChildren<FlightInfo>(true);
                magicCarpet.joystick = go.GetComponentInChildren<VRJoystick>();
                magicCarpet.throttle = go.GetComponentInChildren<VRThrottle>(true);
                magicCarpet.inputManager = go.GetComponentInChildren<VehicleInputManager>(true);
                magicCarpet.vtolAP = go.GetComponentInChildren<VTOLAutoPilot>(true);
                magicCarpet.brakes = go.GetComponentInChildren<AirBrakeController>(true);
                magicCarpet.go = go;
                magicCarpet.Init();
            }
        }
    }
}
