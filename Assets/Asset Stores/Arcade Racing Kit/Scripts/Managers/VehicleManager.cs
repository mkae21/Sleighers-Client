using System.Collections.Generic;
using UnityEngine;

namespace Ilumisoft.ArcardeRacingKit
{
    [DefaultExecutionOrder(-1)]
    [HelpURL("https://ilumisoft.gitbook.io/arcade-racing-kit/managers/vehicle-manager")]
    public class VehicleManager : MonoBehaviour
    {
        public List<Vehicle> Vehicles { get; } = new List<Vehicle>();

        /// <summary>
        /// Adds the given vehicle to the list of active vehicles
        /// </summary>
        /// <param name="vehicle"></param>
        public void Register(Vehicle vehicle)
        {
            if (Vehicles.Contains(vehicle) == false)
            {
                Vehicles.Add(vehicle);
            }
        }

        /// <summary>
        /// Removes the given vehicle from the list of active vehicles
        /// </summary>
        /// <param name="vehicle"></param>
        public void Deregister(Vehicle vehicle)
        {
            if (Vehicles.Contains(vehicle))
            {
                Vehicles.Remove(vehicle);
            }
        }

        /// <summary>
        /// Locks or unlocks the movement of all vehicles. If true, no vehicle can move.
        /// </summary>
        /// <param name="isLocked"></param>
        public void LockVehicleMovement(bool isLocked)
        {
            foreach (var vehicle in Vehicles)
            {
                vehicle.CanMove = !isLocked;
            }
        }
    }
}