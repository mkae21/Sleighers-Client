using UnityEngine;

namespace Ilumisoft.ArcardeRacingKit.Effects
{
    /// <summary>
    /// Effect that can be used to make the wheels of the vehicle turn when the vehicle is turning.
    /// The component should be dierctly attached to the mesh of the wheel.
    /// </summary>
    public class VehicleWheelTurnEffect : VehicleComponent
    {
        [SerializeField]
        [Tooltip("Max angle used to rotate the wheels")]
        float maxAngle = 30.0f;

        [SerializeField]
        [Tooltip("Optional offset applied to the rotation")]
        float rotationOffset = 0f;

        void Update()
        {
            if (Vehicle != null)
            {
                float smoothing = 5.0f;

                Quaternion targetRotation = Vehicle.transform.rotation * Quaternion.Euler(0f, rotationOffset + maxAngle * Vehicle.Input.x, 0);

                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * smoothing);
            }
        }
    }
}