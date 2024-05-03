using UnityEngine;

namespace Ilumisoft.ArcardeRacingKit
{
    [System.Serializable]
    public class VehicleGroundDetection
    {
        [Tooltip("Which layers should be handled as ground")]
        public LayerMask GroundLayers = Physics.DefaultRaycastLayers;

        [Tooltip("How far to raycast when checking for ground")]
        public float RaycastDist = 0.25f;

        Vehicle vehicle;

        /// <summary>
        /// Gets whether the vehicle has been grounded the last time CheckGround was called.
        /// </summary>
        public bool IsGrounded { get; private set; }

        public void Initialize(Vehicle vehicle)
        {
            this.vehicle = vehicle;
        }

        /// <summary>
        /// Checks whether the vehicle is grounded and store the value in the IsGrounded property
        /// </summary>
        public void CheckGround()
        {
            // Create a ray pointing downwards (relative to the vehicle)
            Ray ray = new Ray(vehicle.transform.position, -vehicle.transform.up);

            // If the ray hits a ground layer, the vehicle is grounded, otherwise not
            IsGrounded = Physics.Raycast(ray, RaycastDist, GroundLayers);
        }

        /// <summary>
        /// Draws a ray for debugging purposes (only in editor)
        /// </summary>
        public void OnDrawGizmosSelected(Vehicle vehicle)
        {
#if UNITY_EDITOR
            var direction = -vehicle.transform.up;
            var length = RaycastDist;

            Debug.DrawRay(vehicle.transform.position, direction * length, Color.magenta);
#endif
        }
    }
}
