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
    class HUDShipVector : MonoBehaviour
    {
		private void Awake()
		{
			this.myTransform = base.transform;
		}

		
		private void Start()
		{
			this.img = base.GetComponent<Image>();
			this.flightInfo = base.GetComponentInParent<FlightInfo>();
			this.rb = this.flightInfo.rb;
			this.depth = base.GetComponentInParent<CollimatedHUDUI>().depth;
		}

		public void setShipRB()
        {
			Debug.Log("Attempting to set ship rb");
			if (RequestLandingPatch.requestedCarrier != null && (RequestLandingPatch.requestedCarrier.playerRequestStatus == AirportManager.PlayerRequestStatus.ClearedToLand ||
				RequestLandingPatch.requestedCarrier.playerRequestStatus == AirportManager.PlayerRequestStatus.WaitingForLandingClearance 
				|| RequestLandingPatch.requestedCarrier.playerRequestStatus == AirportManager.PlayerRequestStatus.DirectingToAirbase))
            {
				Debug.Log("Setting rigidbody in hudshipvector");
				currentCarrierAirport = RequestLandingPatch.requestedCarrier;
				shipRB = currentCarrierAirport.gameObject.GetComponentInChildren<Rigidbody>(true);
				vectorEnabled = true;
            }
            else
            {
				Debug.Log("Ship rb not set");
				vectorEnabled = false;
            }
			
			
        }

		public void setVector(bool enable)
        {
			this.vectorEnabled = enable;
        }

		private void FixedUpdate()
		{
			if(currentCarrierAirport != null && (RequestLandingPatch.requestedCarrier.playerRequestStatus == AirportManager.PlayerRequestStatus.ClearedToLand ||
				RequestLandingPatch.requestedCarrier.playerRequestStatus == AirportManager.PlayerRequestStatus.WaitingForLandingClearance
				|| RequestLandingPatch.requestedCarrier.playerRequestStatus == AirportManager.PlayerRequestStatus.DirectingToAirbase) && vectorEnabled)
            {
				if (!this.rb.isKinematic && this.flightInfo.airspeed > 0.5f)
				{
					Ray ray = new Ray(VRHead.instance.transform.position, this.rb.velocity - this.shipRB.velocity);
					this.myTransform.position = ray.GetPoint(this.depth);
					this.myTransform.rotation = Quaternion.LookRotation(ray.direction, this.myTransform.parent.up);
					if (!this.img.enabled)
					{
						this.img.enabled = true;
						return;
					}
				}
				else if (this.img.enabled)
				{
					this.img.enabled = false;
					this.myTransform.localPosition = Vector3.zero;
				}
            }
            else
            {
				this.img.enabled = false;
            }
			
		}

		
		private FlightInfo flightInfo;

		private Rigidbody rb;

		private Image img;

		private float depth;

		private Transform myTransform;

		private Rigidbody shipRB;

		private AirportManager currentCarrierAirport;


		public bool vectorEnabled { get; private set; } = false;

	}
}
