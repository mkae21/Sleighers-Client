using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;

namespace Ilumisoft.ArcardeRacingKit
{
    [DefaultExecutionOrder(-2)]
    [RequireComponent(typeof(_LapManager), typeof(VehicleManager))]
    [HelpURL("https://ilumisoft.gitbook.io/arcade-racing-kit/managers/race-manager")]
    public class RaceManager : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Automatically plays the intro timeline and starts the race if set to true")]
        bool autoStart = true;

        [SerializeField]
        [Tooltip("An intro timeline played before the race starts (optional)")]
        PlayableDirector introTimeline = null;

        [SerializeField]
        [Tooltip("An outro timeline played when the player completed the race (optional)")]
        PlayableDirector outroTimeline = null;

        VehicleManager vehicleManager;
        _LapManager lapManager;

        /// <summary>
        /// Action invoked when the race has started (after the intro timeline has been played)
        /// </summary>
        public UnityAction OnRaceStarted { get; set; }

        /// <summary>
        /// Action invoked when the race has been finished
        /// </summary>
        public UnityAction OnRaceFinished { get; set; }

        /// <summary>
        /// Get whether the race is finished or not
        /// </summary>
        public bool RaceFinished { get; private set; } = false;

        private void Awake()
        {
            vehicleManager = GetComponent<VehicleManager>();
            lapManager = GetComponent<_LapManager>();

            lapManager.OnLapComplete += OnLapComplete;
        }

        private void Start()
        {
            if (autoStart)
            {
                StartRace();
            }
        }

        /// <summary>
        /// Starts the race and plays the intro timeline
        /// </summary>
        /// <returns></returns>
        public Coroutine StartRace()
        {
            return StartCoroutine(StartRaceCoroutine());
        }

        IEnumerator StartRaceCoroutine()
        {
            RaceFinished = false;

            // Lock all vehicles, so that they cannot move until the intro is complete
            vehicleManager.LockVehicleMovement(true);

            // Wait for intro
            if (introTimeline != null && introTimeline.isActiveAndEnabled)
            {
                yield return new WaitWhile(() => introTimeline.state == PlayState.Playing);
            }

            yield return null;

            // Unlock all vehicles
            vehicleManager.LockVehicleMovement(false);

            OnRaceStarted?.Invoke();
        }

        /// <summary>
        /// Finishes the race and plays the outro timeline
        /// </summary>
        public void FinishRace()
        {
            // If it wasn't already finished fire event
            if (!RaceFinished)
            {
                RaceFinished = true;
                OnRaceFinished?.Invoke();

                if (outroTimeline != null && outroTimeline.isActiveAndEnabled)
                {
                    outroTimeline.Play();
                }
            }
        }

        /// <summary>
        /// Callback invoked when the player finihsed a lap
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="lapInfo"></param>
        private void OnLapComplete(Vehicle vehicle, _LapInfo lapInfo)
        {
            // Has the player finished the race?
            if (vehicle.CompareTag("Player") && lapInfo.Completed == lapManager.Laps)
            {
                // If it wasn't already finished, finish the race
                if (!RaceFinished)
                {
                    FinishRace();
                }
            }
        }
    }
}