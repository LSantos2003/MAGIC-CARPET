using Harmony;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace MAGICCARPET
{
    public class Main : VTOLMOD
    {

        public GameObject go;

        private bool mpActive = false;
        // This method is run once, when the Mod Loader is done initialising this game object
        public override void ModLoaded()
        {
            //This is an event the VTOLAPI calls when the game is done loading a scene

            Debug.Log("Attempting to make harmony instance");
            HarmonyInstance harmonyInstance = HarmonyInstance.Create("C-137.MAGIC_CARPET");
            harmonyInstance.PatchAll();

            Debug.Log("Made the harmony instance");
            VTOLAPI.SceneLoaded += SceneLoaded;
            base.ModLoaded();
        }

        private void Start()
        {
           
        }
        //This method is called every frame by Unity. Here you'll probably put most of your code
        void Update()
        {
            if (!mpActive)
            {
                this.go = VTOLAPI.GetPlayersVehicleGameObject();
                if (this.go && this.go.GetComponentInChildren<Carpet>(true) == null)
                {
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

        /*private void addCarpet(PlayerManager.CustomPlaneDef def)
        {
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

        private void checkMPloaded()
        {
            Debug.Log("checking Multiplayer is installed");
            List<Mod> list = new List<Mod>();
            list = VTOLAPI.GetUsersMods();
            foreach (Mod mod in list)
            {
                bool flag = mod.name.Contains("ultiplayer");
                if (flag)
                {
                    Debug.Log("found Multiplayer set f16 mp");
                    mpActive = true;

                   this.addHook();
                    
                }
            }
        }

        private void addHook()
        {
            PlayerManager.onSpawnLocalPlayer += this.addCarpet;
        }

        */

        //This method is like update but it's framerate independent. This means it gets called at a set time interval instead of every frame. This is useful for physics calculations
        void FixedUpdate()
        {

        }


        //This function is called every time a scene is loaded. this behaviour is defined in Awake().
        private void SceneLoaded(VTOLScenes scene)
        {
            //If you want something to happen in only one (or more) scenes, this is where you define it.

            //For example, lets say you're making a mod which only does something in the ready room and the loading scene. This is how your code could look:
            switch (scene)
            {
                case VTOLScenes.ReadyRoom:
                    //Add your ready room code here
                    
                    break;
                case VTOLScenes.LoadingScene:
                    //Add your loading scene code here
                   
                    break;
                case VTOLScenes.Akutan:
                case VTOLScenes.CustomMapBase:
                    Debug.Log("Loading into scene, gonna check mp");
                    //checkMPloaded();
                    break;
            }
        }
    }
}