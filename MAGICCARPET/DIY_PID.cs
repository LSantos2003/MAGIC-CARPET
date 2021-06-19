using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
namespace MAGICCARPET
{
    public class DIY_PID 
    {


      
        public DIY_PID(float p, float i, float d)
        {
            this.kP = p;
            this.kI = i;
            this.kD = d;

        }

        public DIY_PID(float p, float i, float d, float iMin, float iMax)
        {
            this.kP = p;
            this.kI = i;
            this.kD = d;

            this.iMin = iMin;
            this.iMax = iMax;

        }


        public void changeValues(float p, float i, float d) 
        {
            this.kP = p;
            this.kI = i;
            this.kD = d;
        }

        public float getOutput(float desiredVal, float currentVal, float dt, float minVal, float maxVal)
        {
            
            float error = desiredVal - currentVal;

            if(dt == 0)
            {
                dt = Time.deltaTime;
            }

            this._p = this.kP * error;

            this._i = this.kI + error * dt;
            
            float iClamped = Mathf.Clamp(this._i, this.iMin, this.iMax);

            this._d = (this.kD - prev_error) / dt;


            float output = Mathf.Clamp(this._p + iClamped + this._d, minVal, maxVal);

            return output;
        }


        public float getOutput(float desiredVal, float currentVal, float dt)
        {

            float error = desiredVal - currentVal;

            if (dt == 0)
            {
                dt = Time.deltaTime;
            }

            this._p = this.kP * error;

            this._i = this.kI + error * dt;


            this._d = (this.kD - prev_error) / dt;



            return this._p + this._i + this._d;
        }

        public void resetValues()
        {
            this.integral = 0;
            this.prev_error = 0;
        }




        private float _p;
        private float _i;
        private float _d;


        private float error = 0;
        private float prev_error;
        private float integral = 0;

        private float kP;
        private float kI;
        private float kD;

        private float iMin, iMax;
        private float clampedI;
    }
}
