using Harmony;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace MAGICCARPET
{
    public class HUDCarrierSlope : MonoBehaviour
    {

        private void Awake()
        {
            this.myTransform = base.transform;
            
        }

        private void Start()
        {
            this.depth = base.GetComponentInParent<CollimatedHUDUI>().depth;

            this.img = base.GetComponent<Image>();
            this.img.enabled = false;
            //this.img.sprite = this.imgSpriteReplace.sprite;
        }


        private void LateUpdate()
        {
            if (this.hudEnabled && this.shipSlopeTf != null)
            {
                SetPos(this.myTransform, this.shipSlopeTf.position);

            }
            
        }

        private void SetPos(Transform tf, Vector3 shipPos)
        {
            Ray ray = new Ray(VRHead.position, shipPos - VRHead.position);
            tf.position = ray.GetPoint(this.depth);
            tf.rotation = Quaternion.LookRotation(ray.direction, this.myTransform.parent.up);
        }


        public void enableHud(bool enable)
        {
            this.hudEnabled = true;
            this.img.enabled = true;
        }
        public void setShipTf()
        {
            if (RequestLandingPatch.requestedCarrier != null && RequestLandingPatch.requestedCarrier.playerRequestStatus == AirportManager.PlayerRequestStatus.ClearedToLand)
            {
                Debug.Log("Setting OLS transform in hud carrier slope");
                this.currentCarrierAirport = RequestLandingPatch.requestedCarrier;

                this.shipSlopeTf = RequestLandingPatch.shipOLS.transform;
                return;
              
            }
            
            Debug.Log("OLS transform not set");
               
            
        }


        private bool hudEnabled = false;

        private Transform shipSlopeTf;

        private Transform myTransform;
        private float depth;
        private AirportManager currentCarrierAirport;

        private Image img;
        public Image imgSpriteReplace;

        
    }
}
