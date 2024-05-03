using UnityEngine;

namespace Ilumisoft.ArcardeRacingKit
{
    /// <summary>
    /// Base class for all of our vehicle components, like vehicle UI, vehicle effect components etc.
    /// Allows inheriting classes to easily access the vehicle and avoid boilercode.
    /// </summary>
    public abstract class VehicleComponent : MonoBehaviour
    {
        [SerializeField]
        Vehicle vehicle;

        public Vehicle Vehicle
        {
            get => vehicle;
            set => vehicle = value;
        }

        private void Reset()
        {
            if (vehicle == null)
            {
                vehicle = GetComponentInParent<Vehicle>();
            }
        }
    }
}