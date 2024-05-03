using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Ilumisoft.ArcardeRacingKit
{
    [DefaultExecutionOrder(-1)]
    [HelpURL("https://ilumisoft.gitbook.io/arcade-racing-kit/managers/lap-manager")]
    public class _LapManager : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Number of laps required to pass in order to finish the race.")]
        int laps;

        _Finish finish;

        /// <summary>
        /// Dictionary containing the lap info of all vehicles
        /// </summary>
        Dictionary<Vehicle, _LapInfo> LapInfoDictionary = new Dictionary<Vehicle, _LapInfo>();

        /// <summary>
        /// Action invoked, when a vehicle completes a lap
        /// </summary>
        public UnityAction<Vehicle, _LapInfo> OnLapComplete { get; set; }

        /// <summary>
        /// Gets the number of laps requried to finish the race
        /// </summary>
        public int Laps { get => this.laps; set => this.laps = value; }

        private void Awake()
        {
            finish = FindObjectOfType<_Finish>();

            if (finish != null)
            {
                finish.OnVehicleEnter += OnVehicleEnter;
            }
            else
            {
                Debug.Log("Lap Manager requires a finish line in order to work as expected, but no finish line has been added to the scene", this);
            }
        }

        /// <summary>
        /// Callback invoked when a vehicle enters the fiish line (after completing a lap)
        /// </summary>
        /// <param name="vehicle"></param>
        private void OnVehicleEnter(Vehicle vehicle)
        {
            // Cancel if vehicle is null
            if (vehicle == null)
            {
                return;
            }

            // Create new dictionary entry if none exists
            if (!LapInfoDictionary.ContainsKey(vehicle))
            {
                LapInfoDictionary.Add(vehicle, new _LapInfo());
            }

            // Increase and update the number of completed laps
            var lapInfo = LapInfoDictionary[vehicle];

            lapInfo.Completed = Mathf.Clamp(lapInfo.Completed + 1, 0, laps);

            LapInfoDictionary[vehicle] = lapInfo;

            OnLapComplete?.Invoke(vehicle, lapInfo);
        }

        /// <summary>
        /// Gets the lap info for the given vehicle
        /// </summary>
        /// <param name="vehicle"></param>
        /// <returns></returns>
        public _LapInfo GetLapInfo(Vehicle vehicle)
        {
            var lapInfo = new _LapInfo()
            {
                Completed = 0
            };

            if (vehicle != null && LapInfoDictionary.ContainsKey(vehicle))
            {
                lapInfo = LapInfoDictionary[vehicle];
            }

            return lapInfo;
        }
    }
}