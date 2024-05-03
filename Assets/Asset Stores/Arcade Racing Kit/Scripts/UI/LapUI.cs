using TMPro;
using UnityEngine;

namespace Ilumisoft.ArcardeRacingKit.UI
{
    public class LapUI : VehicleComponent
    {
        [SerializeField]
        protected TextMeshProUGUI text = null;

        LapManager lapManager;

        private void Start()
        {
            lapManager = FindObjectOfType<LapManager>();

            if (lapManager != null)
            {
                lapManager.OnLapComplete += OnLapComplete;
            }
            else
            {
                Debug.Log("LapUI requires the LapManager in order to work as expected, but no Lap Manager has been added to the scene.", this);
            }

            UpdateLapText(1);
        }

        /// <summary>
        /// Callback invoked when a vehicle completes a lap
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="lapInfo"></param>
        private void OnLapComplete(Vehicle vehicle, LapInfo lapInfo)
        {
            // Return if another vehicle completed the lap
            if (this.Vehicle != vehicle)
            {
                return;
            }
            // Update the lap text
            else
            { 
                int lapsCompleted = lapManager.GetLapInfo(vehicle).Completed;

                int currentLap = Mathf.Min(lapsCompleted + 1, lapManager.Laps);

                UpdateLapText(currentLap);
            }
        }

        protected virtual void UpdateLapText(int currentLap)
        {
            if (lapManager != null)
            {
                text.text = $"LAP {currentLap}/{lapManager.Laps}";
            }
        }
    }
}