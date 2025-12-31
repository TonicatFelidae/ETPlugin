using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ET.SupportKit.ETPhysic
{
    public static class ETForce
    {
        public static float GravityConst;
        public static float ForceModified;
        public static float RepelDistance = 0.1f;
        public static float[,] forceInfos;
        public static void ApplyNewPhysicWorldState(float gravityConst, float forceModified)
        {
            GravityConst = gravityConst; 
            ForceModified = forceModified;
        }
        /// <summary>
        /// Apply force that depend on distance and mass.
        /// </summary>
        /// <param name="moA"></param>
        /// <param name="moB"></param>
        public static void ApplyGravityBetween(ETBody2D moA, ETBody2D moB)
        {
            Vector2 vector = moA.position - moB.position;
            float distance = vector.x * vector.x + vector.y * vector.y;
            if (distance == 0) return;
            if (distance > Mathf.Max(moA.repelDistance, moB.repelDistance))
            {
                //float force = (GravityConst * moA.mass * moB.mass) /
                //    (distance * distance * distance);
                vector = Mathf.Clamp((GravityConst * moA.mass * moB.mass) /
                   (distance * distance), -1f, 1f) * vector.normalized * ForceModified;
                moA.AddForce(-vector);
                moB.AddForce(vector);
            }
            else
            {
                //float force = (GravityConst * moA.mass * moB.mass) /
                //    (distance * distance * distance);
                vector = Mathf.Clamp((GravityConst * moA.mass * moB.mass) /
                   (distance * distance), -0.01f, 0.01f) * vector.normalized * ForceModified;
                moB.AddForce(-vector);
                moA.AddForce(vector);
            }
        }
        /// <summary>
        ///  Apply force that depend on distance and mass. Mass equal to 1.
        /// </summary>
        /// <param name="moA"></param>
        /// <param name="moB"></param>
        public static void ApplyFixedGravityBetween(ETBody2D moA, ETBody2D moB, bool applyRepel = false)
        {
            Vector2 vector = moA.position - moB.position;
            float distance = vector.x * vector.x + vector.y * vector.y;

            if (distance == 0) return;
            if (applyRepel)
            {
                float repelDistance = Mathf.Max(moA.repelDistance, moB.repelDistance);
                if (distance < 1)
                if (distance > repelDistance)
                {
                    vector = Mathf.Clamp((GravityConst) / (distance* distance), -0.01f, 0.01f) * vector.normalized * ForceModified;
                    moA.AddForce(-vector);
                    moB.AddForce(vector);
                }
                else
                {
                    //float force = (GravityConst * moA.mass * moB.mass) /
                    //    (distance * distance * distance);
                    vector = Mathf.Clamp((GravityConst)/(distance * distance), -0.1f, 0.1f) * vector.normalized * ForceModified;
                    moB.AddForce(-vector);
                    moA.AddForce(vector);
                }
            }
            else
            {
                vector = Mathf.Clamp((GravityConst) / (distance * distance), -0.001f, 0.001f) * vector.normalized * ForceModified;
                moA.AddForce(-vector);
                moB.AddForce(vector);
            }
        }
        public static Vector2 GetFixedGravityBetween(Vector2 moA, Vector2 moB, int iA, int iB, bool applyRepel = false)
        {
            Vector2 vector = moA - moB;
            float distance = vector.x * vector.x + vector.y * vector.y;
            float directionForce = forceInfos[iA, iB];
            Vector2 ret;
            if (distance == 0) return Vector2.zero; 
            if (applyRepel)
            {
                //if (distance < 10)
                //{
                    ret = -Mathf.Clamp((GravityConst) / (distance * distance), -0.1f, 0.1f) * vector.normalized * ForceModified * directionForce; //incorrect dirrection
                    if (distance < RepelDistance)
                    {
                        //float force = (GravityConst * moA.mass * moB.mass) /
                        //    (distance * distance * distance);
                        ret += Mathf.Clamp((GravityConst) / (distance * distance), -0.1f, 0.1f) * vector.normalized * ForceModified;
                    }
                    return ret;
                //}
                //    
                //
                //return Vector2.zero;
            }
            else
            {
                return Mathf.Clamp((GravityConst) / (distance * distance), -0.0001f, 0.0001f) * vector.normalized * ForceModified;
            }
        }
        public static void ApplyFixedGravityBetween(ETBody2D moA, ETBody2D moB, float bound)
        {
            Vector2 vector = moA.position - moB.position;
            float distance = vector.x * vector.x + vector.y * vector.y;

            if (distance == 0) return;
            float repelDistance = Mathf.Max(moA.repelDistance, moB.repelDistance);
            if (distance < 1)
                vector = Mathf.Clamp((GravityConst) / (distance * distance), -0.01f, 0.01f) * vector.normalized * ForceModified;

        }
    }

	[Serializable]
	public struct ETBody2D
	{
		//passive
		public float mass;
        public float repelDistance;
        //active
        public float acceleration;
		public float velocity;
        public Vector2 position;
        //
        public Vector2 curentForce;
		List<Vector2> forces;
        public void Init(Vector2 position)
        {
            forces = new List<Vector2>();
            this.position = position;
        }
		public void CalculateForce()
		{
			Vector2 ret = Vector2.zero;
			if (forces.Count > 0)
				foreach (Vector2 force in forces)
				{
					ret += force;
				}
			curentForce = ret;
			forces = new();
        }
        public void AddForce(Vector2 force)
        {
            forces.Add(force);

        }

    }

    enum MinimumForceSetup
    {
        None,
        RepelField,
        Bound
    }
}
