using UnityEngine;
using UnityEngine.Events;

namespace Ilumisoft.ArcardeRacingKit
{
    [RequireComponent(typeof(Collider))]
    public class Checkpoint : MonoBehaviour
    {
        public UnityAction<Vehicle, Checkpoint> OnVehicleEnterCheckpoint;

        new Collider collider;

        private void Awake()
        {
            collider = GetComponent<Collider>();
        }

        private void Start()
        {
            // Ensure the collider is a trigger
            collider.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            var vehicle = other.GetComponentInParent<Vehicle>();

            if (vehicle != null)
            {
                OnVehicleEnterCheckpoint?.Invoke(vehicle, this);
            }
        }
    }
}