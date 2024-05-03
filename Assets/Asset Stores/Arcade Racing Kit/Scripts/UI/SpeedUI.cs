using System.Collections;
using TMPro;
using UnityEngine;

namespace Ilumisoft.ArcardeRacingKit.UI
{
    /// <summary>
    /// Updates the speed UI text of the vehicle
    /// </summary>
    public class SpeedUI : VehicleComponent
    {
        [System.Serializable]
        public enum Mode 
        {
            KPH,
            MPH
        }

        [SerializeField]
        protected TextMeshProUGUI text = null;

        [SerializeField]
        Mode mode = Mode.KPH;

        private void OnEnable()
        {
            StopAllCoroutines();
            StartCoroutine(UpdateTextCoroutine());
        }

        /// <summary>
        /// Updates the speed text in a regular interval
        /// </summary>
        /// <returns></returns>
        IEnumerator UpdateTextCoroutine()
        {
            // Wait one frame to ensure all vehicle systems are setup
            yield return null;

            var refreshDelay = new WaitForSeconds(0.1f);

            while(true)
            {
                UpdateText();

                yield return refreshDelay;
            }
        }
        
        /// <summary>
        /// Updates the UI text
        /// </summary>
        protected virtual void UpdateText()
        {
            int displayValue = GetDisplayValue();
            string metric = GetMetric();

            text.text = $"{displayValue} {metric}";
        }

        /// <summary>
        /// Gets the velocity in kph or mph depending on the selected mode
        /// </summary>
        /// <returns></returns>
        int GetDisplayValue()
        {
            float value = Vehicle.ForwardSpeed * 3.6f;

            if (mode == Mode.MPH)
            {
                // Multiply km/h by conversion factor to get mph
                value *= 0.6214f;
            }

            return (int) value;
        }

        /// <summary>
        /// Gets the metric depending on the selected mode
        /// </summary>
        /// <returns></returns>
        string GetMetric()
        {
            return (mode == Mode.MPH) ? "MPH" : "KPH";
        }
    }
}