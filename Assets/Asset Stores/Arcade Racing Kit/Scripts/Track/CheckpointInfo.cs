using UnityEngine;

namespace Ilumisoft.ArcardeRacingKit
{
    /// <summary>
    /// This component is attached to vehicles, to store information about the checkpoints the vehicle has passed.
    /// Via the info it can be determined by the Finish component whether a vehicle has successfully passed a lap or not.
    /// This prevents cheating like driving through the finish line, driving backwards and driving through it again.
    /// </summary>
    public class _CheckpointInfo : MonoBehaviour
    {
        /// <summary>
        /// Info whether the vehicle is allowed to pass the finish line to complete it's current lap or needs to pass a checkpoint first.
        /// </summary>
        public bool CanPassFinish { get; set; }

        /// <summary>
        /// Last checkpoint passed
        /// </summary>
        public _Checkpoint LastCheckpoint { get; private set; } = null;

        /// <summary>
        /// The next checkpoint that needs to be passed
        /// </summary>
        public _Checkpoint NextCheckpoint { get; private set; } = null;

        public void SetLastCheckpoint(_Checkpoint checkpoint)
        {
            LastCheckpoint = checkpoint;
        }

        public void SetNextCheckpoint(_Checkpoint checkpoint)
        {
            NextCheckpoint = checkpoint;
        }
    }
}