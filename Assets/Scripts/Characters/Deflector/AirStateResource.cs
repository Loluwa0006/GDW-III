using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AirStateResource", menuName = "Scriptable Objects/AirStateResource")]
public class AirStateResource : ScriptableObject
{
   
    
    [System.Serializable]
    public class JumpInfo
    {
        public float jumpTimeToPeak = 0.4f;
        public float jumpTimeToDecent = 0.5f;
        public float jumpHeight = 5.0f;

        [HideInInspector] public float jumpVelocity;
        [HideInInspector] public float jumpGravity;
        [HideInInspector] public float fallGravity;

        public float maxFallSpeed = 10.0f;

        public void InitJumpInfo()
        {
         
                jumpGravity = (2.0f * jumpHeight) / (jumpTimeToPeak * jumpTimeToPeak);
                fallGravity = (2.0f * jumpHeight) / (jumpTimeToDecent * jumpTimeToDecent);
                jumpVelocity = (2.0f * jumpHeight) / jumpTimeToPeak;

                maxFallSpeed = Mathf.Abs(maxFallSpeed) * -1; //force it to be negative
        }

    }




    public float airAcceleration = 2.0f;
    public float airStrafeSpeed = 20.0f;

  
    
}
