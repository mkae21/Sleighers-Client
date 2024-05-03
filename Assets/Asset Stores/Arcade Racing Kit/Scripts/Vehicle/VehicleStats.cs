using UnityEngine;

namespace Ilumisoft.ArcardeRacingKit
{
    /// <summary>
    /// Data class containing the main stats of the vehicle, like it's maximum speed, acceleration etc.
    /// </summary>
    [System.Serializable]
    public class VehicleStats
    {
        [Tooltip("The maximum speed forwards")]
        public float MaxSpeed = 50;

        [Tooltip("How fast the vehicle accelerates.")]
        public float Acceleration = 20;

        [Tooltip("How quickly the vehicle can turn left and right."), Range(0,3)]
        public float SteeringPower = 1.5f;

        [Tooltip("How much side friction is applied"), Range(0,1)]
        public float Grip = 1;
    }
}
