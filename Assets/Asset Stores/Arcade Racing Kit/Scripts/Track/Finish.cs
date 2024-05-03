using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Ilumisoft.ArcardeRacingKit
{
    public class Finish : MonoBehaviour
    {
        [SerializeField]
        List<Checkpoint> checkpoints = new List<Checkpoint>();

        public UnityAction<Vehicle> OnVehicleEnter;

        private void Start()
        {
            foreach (var checkpoint in checkpoints)
            {
                checkpoint.OnVehicleEnterCheckpoint += OnVehicleEnterCheckpoint;
            }
        }

        /// <summary>
        /// Callback invoked when a vehicle enters a checkpoint
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="checkpoint"></param>
        private void OnVehicleEnterCheckpoint(Vehicle vehicle, Checkpoint checkpoint)
        {
            var checkpointInfo = GetOrAddCheckpointInfo(vehicle);

            // Has the vehicle not passed any checkpoint yet?
            if(checkpointInfo.LastCheckpoint == null)
            {
                // Only accept the first one
                if (checkpoint == checkpoints.First())
                {
                    checkpointInfo.SetLastCheckpoint(checkpoint);
                    checkpointInfo.SetNextCheckpoint(GetNextCheckpoint(checkpoint));
                }
            }
            // Otherwise if the passed checkpoint is the next checkpoint which needs to be past,
            // unlock its successor
            else if(checkpointInfo.NextCheckpoint == checkpoint)
            {
                // Enable the finish line if the last checkpoint has been passed
                if(checkpoint == checkpoints.Last())
                {
                    checkpointInfo.CanPassFinish = true;
                }

                checkpointInfo.SetLastCheckpoint(checkpoint);
                checkpointInfo.SetNextCheckpoint(GetNextCheckpoint(checkpoint));
            }
        }

        /// <summary>
        /// Returns the successor of the given checkpoint
        /// </summary>
        /// <param name="checkpoint"></param>
        /// <returns></returns>
        Checkpoint GetNextCheckpoint(Checkpoint checkpoint)
        {
            int index = checkpoints.IndexOf(checkpoint);

            return checkpoints[(index + 1) % checkpoints.Count];
        }

        /// <summary>
        /// Gets the checkpoint info of the given vehicle or adds one if none exists
        /// </summary>
        /// <param name="vehicle"></param>
        /// <returns></returns>
        CheckpointInfo GetOrAddCheckpointInfo(Vehicle vehicle)
        {
            var result = vehicle.GetComponent<CheckpointInfo>();

            if(result == null)
            {
                result = vehicle.gameObject.AddComponent<CheckpointInfo>();
            }

            return result;
        }

        private void OnTriggerEnter(Collider other)
        {
            if(other.attachedRigidbody != null && other.attachedRigidbody.TryGetComponent<Vehicle>(out var vehicle))
            {
                OnVehicleEnterFinish(vehicle);
            }
        }

        /// <summary>
        /// Callback invoked when a vehicle enters the finish line
        /// </summary>
        /// <param name="vehicle"></param>
        void OnVehicleEnterFinish(Vehicle vehicle)
        {
            var checkpointInfo = GetOrAddCheckpointInfo(vehicle);

            // When passing the finish line, reset the last checkpoint passed. 
            // This prevents the player from cheating, like driving backwards and pass the last checkpoint and then the finish line again!
            checkpointInfo.SetLastCheckpoint(null);

            if (checkpointInfo.CanPassFinish)
            {
                checkpointInfo.CanPassFinish = false;
                OnVehicleEnter?.Invoke(vehicle);
            }
        }
    }
}