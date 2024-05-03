using UnityEngine;

namespace Ilumisoft.ArcardeRacingKit
{
    /// <summary>
    /// Disables the UI of the vehicle until the race has started, to prevent the UI (like speed UI) being aready visible when the intro is played.
    /// </summary>
    public class VehicleUIController : MonoBehaviour
    {
        [SerializeField]
        GameObject canvas = null;

        RaceManager raceManager;

        private void Awake()
        {
            raceManager = FindObjectOfType<RaceManager>();

            if (raceManager != null)
            {
                raceManager.OnRaceStarted -= OnRaceStarted;
                raceManager.OnRaceStarted += OnRaceStarted;
            }
        }

        private void Start()
        {
            // Disable the vehicle UI before the race starts
            if (canvas != null)
            {
                canvas.SetActive(false);
            }
        }

        private void OnRaceStarted()
        {
            // Enable the vehicle UI when the race starts
            if (canvas != null)
            {
                canvas.SetActive(true);
            }
        }
    }
}