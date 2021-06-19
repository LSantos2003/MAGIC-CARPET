using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity;
using UnityEngine;
using UnityEngine.UI;

namespace MAGICCARPET
{ 
    public class Carpet : MonoBehaviour
    {


        /*
         * Le plan:
         * Pitch PID to control the pitch (should be 4-5 degrees above horizon)
         * ThrottlePid to control total AOA (controling the pitch and aoa should in turn control the glideslope (aoa - pitch)
         * Flaps pid to inch the plane up and down (hopefully throttle pid gets it to the right speed)
         * (Maybe use addition/subtraction to fool the throttle pid when using the flaps pid)
         */

        private DIY_PID PID_FLAPS;
        private DIY_PID PID_Pitch;
        private DIY_PID PID_THROTTLE;
        private DIY_PID PID_BRAKE;

        public static float FLAPS_P = 0.7f;
        public static float FLAPS_I = 0;
        public static float FLAPS_D = 0;

        public static float BRAKE_P = 2.2f;
        public static float BRAKE_I = 0;
        public static float BRAKE_D = 0;

        public static float PITCH_P = -0.08f;
        public static float PITCH_I = 0;
        public static float PITCH_D = 0;

        public static float THROTTLE_P = -7.2f;
        public static float THROTTLE_I = 0;
        public static float THROTTLE_D = -0.1f;
        public static float THROTTLE_I_MAX = 0.1F;
        public static float THROTTLE_I_MIN = -0.1F;

       
        public void Init()
        {

            foreach (VRInteractable i in this.go.GetComponentsInChildren<VRInteractable>(true))
            {
                if(i.interactableName == "Flaps")
                {
                    this.flapsLever = i.gameObject.GetComponent<VRLever>();
                    break;
                }
            }

           
            this.hudInfo = this.go.GetComponentInChildren<HUDWeaponInfo>(true);
            this.hook = this.go.GetComponentInChildren<Tailhook>();
            this.gearAnim = this.go.GetComponentInChildren<GearAnimator>();

            GameObject velVector = this.go.GetComponentInChildren<HUDVelVector>(true).gameObject;

            GameObject shipVectorGo = Instantiate(velVector, velVector.transform.parent);
                       
            shipVectorGo.SetActive(false);
            Destroy(shipVectorGo.GetComponent<HUDVelVector>());
            this.shipVector = shipVectorGo.AddComponent<HUDShipVector>();
            shipVectorGo.GetComponent<Image>().color = new Color32(255, 0, 0, 255);
            shipVectorGo.SetActive(true);

            
            
            //Creates the glide slope reference
            HUDElevationLadder ladder = this.go.GetComponentInChildren<HUDElevationLadder>(true);

            Vector3 vector = ladder.collimatedHud.GetScaleFactor() * Vector3.one;

            
            this.glideReference = UnityEngine.Object.Instantiate<GameObject>(ladder.descendTemplate, ladder.ladderTransform);
            this.glideReference.GetComponentInChildren<Text>().text = "-----";
            this.glideReference.transform.localRotation = Quaternion.Euler((float)targetGlideSlope, 0f, 0f);
            this.glideReference.transform.localPosition = this.glideReference.transform.localRotation * new Vector3(0f, 0f, ladder.collimatedHud.depth);
            this.glideReference.transform.localScale = vector * this.glideReference.transform.localScale.x;
            this.glideReference.SetActive(false);

            this.hudIndicator = this.go.GetComponentInChildren<HUDJoyIndicator>(true);
            this.aero = this.go.GetComponentInChildren<AeroController>(true);

            this.PID_FLAPS = new DIY_PID(FLAPS_P, FLAPS_I, FLAPS_D);
            this.PID_Pitch = new DIY_PID(PITCH_P, PITCH_I, PITCH_D);
            this.PID_THROTTLE = new DIY_PID(THROTTLE_P, THROTTLE_I, THROTTLE_D, THROTTLE_I_MIN, THROTTLE_I_MAX);
            this.PID_BRAKE = new DIY_PID(BRAKE_P, BRAKE_I, BRAKE_D);
            //Replaces the joystick's sending command in order to make me own auto pilot
            //Might do this for throttle too?
            this.joystick.OnSetStick = new Vector3Event();
            this.joystick.OnSetStick.AddListener(UpdatePYR);

            
            //this.joystick.OnMenuButtonDown.AddListener(toggleAP);
            
            this.joystick.OnMenuButtonDown.AddListener(this.toggleAP);

           /* foreach (AeroController.ControlSurfaceTransform surface in aero.controlSurfaces)
            {
                if (surface.rollFactor == -1f && surface.transform.name.Contains("Right"))
                {
                    surface.flapsFactor = -0.7f;
                    surface.rollFactor = -0.5f;

                    ailerons.Add(surface);
                   
                }else if(surface.rollFactor == -1f && surface.transform.name.Contains("Left"))
                {
                    surface.flapsFactor = 0.7f;
                    surface.rollFactor = -0.5f;
                    ailerons.Add(surface);
                    
                }

            }*/

            this.wm = this.go.GetComponentInChildren<WeaponManager>(true);

        }


        private void Update()
        {
            if (this.hook.isDeployed && !this.glideReference.activeSelf)
            {
                this.glideReference.SetActive(true);
                
            }else if (!this.hook.isDeployed && this.glideReference.activeSelf)
            {
                this.glideReference.SetActive(false);
            }
            

        }

        private void UpdatePYR(Vector3 vector)
        {
            
            this.Evaluate(vector);
        }

        
        private void Evaluate(Vector3 joyPYR)
        {

            //breaks out of magic carpet when you go afterburner
            if(throttle.currentThrottle >= 0.75 && this.apEnabled)
            {
                this.toggleAP();
                return;
            }

            if (this.apEnabled)
            {


                

                //Shuts off AP if throttle is above 0.8
                /*if(throttle.currentThrottle > 0.8f)
                {
                    this.toggleAP();
                    return;
                }*/

                float currentAlpha = flightInfo.aoa;
                float currentPitch = flightInfo.pitch;

                //Desired slope is the desired slope displacement
                float desiredSlope = joyPYR.x * 4f;
                desiredSlope = Mathf.Clamp(desiredSlope, -2.2f, 2.2f);

                //Debug the joystick to find the bounds for pitch
                float pitchOutput = this.PID_Pitch.getOutput(targetPitch, currentPitch, 0, -1.0f, 1.0f);

                Vector3 newJoystickInput = new Vector3(pitchOutput, joyPYR.y * 0.2f, joyPYR.z);


                //Using desired slope to fool the throttle/aoa pid so it doesn't mess with flaps pid
                float currentSlope = (currentAlpha - currentPitch) - desiredSlope;
                

               
                //FlightLogger.Log((flapDeflection).ToString());
             

                //Throttle PID 
                //TODO: ADD SPEED BRAKE TO SLOW DOWN (Separate pid?)
                float throttleOutput = this.PID_THROTTLE.getOutput(targetAlpha + desiredSlope, currentAlpha, 0, 0, 0.7f);

                //Brakes PID
                float brakeOutput = this.PID_BRAKE.getOutput(targetAlpha + desiredSlope, currentAlpha, 0, 0, 1f);


                float flapDeflection = -this.PID_FLAPS.getOutput(targetAlpha + desiredSlope, currentAlpha, 0, -0.3f, 0.3f);
                this.aero.SetFlaps(0.4f + flapDeflection);
                
                
              
                this.vtolAP.SetThrottle(throttleOutput);
                

                

                this.inputManager.SetJoystickPYR(newJoystickInput);

                this.brakes.SetBrake(brakeOutput);
                this.inputManager.SetVirtualBrakes(brakeOutput);
                this.aero.SetBrakes(brakeOutput);
                this.hudIndicator.SetBrakes(brakeOutput);

                
                
            }
            else
            {
                
                inputManager.SetJoystickPYR(joyPYR);
            }
        }

        /*void OnGUI()
        {
            if (this.flightInfo != null)
            {
                THROTTLE_P = GUI.HorizontalSlider(new Rect(525, 25, 200, 30), THROTTLE_P, -10.0F, 10.0F);
                GUI.TextField(new Rect(800, 25, 200, 30), "p: " + (Mathf.Round(THROTTLE_P * 10) / 10f).ToString());
                THROTTLE_I = GUI.HorizontalSlider(new Rect(525, 50, 200, 30), THROTTLE_I, -10.0F, 10.0F);
                GUI.TextField(new Rect(800, 50, 200, 30), "i: " + (Mathf.Round(THROTTLE_I * 10) / 10f).ToString());
                THROTTLE_D = GUI.HorizontalSlider(new Rect(525, 75, 200, 30), THROTTLE_D, -10.0F, 10.0F);
                GUI.TextField(new Rect(800, 75, 200, 30), "d: " + (Mathf.Round(THROTTLE_D * 10) / 10f).ToString());

                PITCH_P = GUI.HorizontalSlider(new Rect(525, 125, 200, 30), PITCH_P, -10.0F, 10.0F);
                GUI.TextField(new Rect(800, 125, 200, 30), "p: " + PITCH_P.ToString());
                PITCH_I = GUI.HorizontalSlider(new Rect(525, 150, 200, 30), PITCH_I, -10.0F, 10.0F);
                GUI.TextField(new Rect(800, 150, 200, 30), "i: " + PITCH_I.ToString());
                PITCH_D = GUI.HorizontalSlider(new Rect(525, 175, 200, 30), PITCH_D, -10.0F, 10.0F);
                GUI.TextField(new Rect(800, 175, 200, 30), "d: " + PITCH_D.ToString());

                BRAKE_P = GUI.HorizontalSlider(new Rect(525, 225, 200, 30), BRAKE_P, -10.0F, 10.0F);
                GUI.TextField(new Rect(800, 225, 200, 30), "p: " + (Mathf.Round(BRAKE_P * 10) / 10f).ToString());
                BRAKE_I = GUI.HorizontalSlider(new Rect(525, 250, 200, 30), BRAKE_I, -10.0F, 10.0F);
                GUI.TextField(new Rect(800, 250, 200, 30), "i: " + (Mathf.Round(BRAKE_I * 10) / 10f).ToString());
                BRAKE_D = GUI.HorizontalSlider(new Rect(525, 275, 200, 30), BRAKE_D, -10.0F, 10.0F);
                GUI.TextField(new Rect(800, 275, 200, 30), "d: " + (Mathf.Round(BRAKE_D * 10) / 10f).ToString());

                FLAPS_P = GUI.HorizontalSlider(new Rect(525, 325, 200, 30), FLAPS_P, -5.0F, 5.0F);
                GUI.TextField(new Rect(800, 325, 200, 30), "p: " + (Mathf.Round(FLAPS_P * 10) / 10f).ToString());

                //Need to divide by 100 in order to get decimal
                //I know there's a better way of doing this lmao
                this.PID_THROTTLE.changeValues(THROTTLE_P, THROTTLE_I, THROTTLE_D);
                this.PID_Pitch.changeValues(PITCH_P, PITCH_I, PITCH_D);
                this.PID_BRAKE.changeValues(BRAKE_P, BRAKE_I, BRAKE_D);
                this.PID_FLAPS.changeValues(FLAPS_P, FLAPS_I, FLAPS_D);
            }
        }*/

        public void toggleAP()
        {
            
            if ((!this.wm.isMasterArmed && this.gearAnim.state != GearAnimator.GearStates.Retracted) || this.apEnabled)
            {
                this.apEnabled = !this.apEnabled;

                this.throttle.sendEvents = !this.apEnabled;

                this.PID_THROTTLE.resetValues();

                this.shipVector.setShipRB();
                this.shipVector.setVector(this.apEnabled);


                if (apEnabled)
                {
                    this.hudInfo.weaponNameText.text = "M-CARP";
                }
                else
                {
                    this.flapsLever.RemoteSetState(this.flapsLever.currentState);
                    this.hudInfo.RefreshWeaponInfo();
                }
                //this.glideReference.SetActive(this.apEnabled);

                /* foreach(AeroController.ControlSurfaceTransform aileron in this.ailerons)
                 {
                     if (aileron.transform.name.Contains("Right") && apEnabled)
                     {
                         aileron.flapsFactor = -1f;
                     }else if(aileron.transform.name.Contains("Left") && apEnabled)
                     {
                         aileron.flapsFactor = 1f;
                     }else if(!apEnabled)
                     {
                         aileron.flapsFactor = 0f;
                     }
                     aileron.Init();
                 }*/

                FlightLogger.Log(apEnabled ? "MAGIC CARPET ACTIVATED" : "MAGIC CARPET DEACTIVATED");
            }
            

        }

        float map(float x, float in_min, float in_max, float out_min, float out_max)
        {
            return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
        }

        public static GameObject GetChildWithName(GameObject obj, string name)
        {


            Transform[] children = obj.GetComponentsInChildren<Transform>(true);
            foreach (Transform child in children)
            {
                if (child.name == name || child.name.Contains(name + "(clone"))
                {
                    return child.gameObject;
                }
            }


            return null;

        }


        private void togglePause()
        {
            paused = !paused;

            Time.timeScale = paused ? 0.01f : 1;
        }

        private HUDWeaponInfo hudInfo;
        private GameObject glideReference;
        private List<AeroController.ControlSurfaceTransform> ailerons = new List<AeroController.ControlSurfaceTransform>();
        private bool apEnabled = false;
        private WeaponManager wm;
        private Tailhook hook;
        private GearAnimator gearAnim;
        private VRLever flapsLever;

        public FlightInfo flightInfo;
        public VRJoystick joystick;
        public VRThrottle throttle;
        public VehicleInputManager inputManager;
        public VTOLAutoPilot vtolAP;
        public AirBrakeController brakes;
        private AeroController aero;
        private HUDJoyIndicator hudIndicator;
        public GameObject go;


        public static float targetAlpha = 8.0f;
        public static float targetGlideSlope = 3.5f;
        public static float targetPitch = targetAlpha - targetGlideSlope;


        private HUDShipVector shipVector;

        public bool paused = false;

        

    }
}
