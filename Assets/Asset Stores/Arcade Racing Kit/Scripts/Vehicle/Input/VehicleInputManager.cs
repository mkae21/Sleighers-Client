using UnityEngine;

namespace Ilumisoft.ArcardeRacingKit.Input
{
    public class VehicleInputManager
    {
        /// <summary>
        /// List of all input sources of the vehicle (e.g. Player, AI)
        /// </summary>
        VehicleInputSource[] inputSources;

        /// <summary>
        /// The currently active input
        /// </summary>
        public Vector2 Input { get; private set; }

        /// <summary>
        /// Collects all input sources available on the vehicle
        /// </summary>
        /// <param name="vehicle"></param>
        public void Initialize(Vehicle vehicle)
        {
            // Get all input source components
            inputSources = vehicle.GetComponentsInChildren<VehicleInputSource>();
        }

        /// <summary>
        /// Collects the input from all input sources
        /// </summary>
        public void CollectInput()
        {
            Input = Vector2.zero;

            foreach (var inputSource in inputSources)
            {
                if (inputSource.IsEnabled)
                {
                    Vector2 value = CollectInputFromSource(inputSource);

                    // Only apply non zero input. This ensures that when multiple sources are available, active sources are not overwritten by inactive ones
                    if (value.sqrMagnitude > 0)
                    {
                        Input = value;
                    }
                }
            }
        }

        /// <summary>
        /// Collects the input from the given input source
        /// </summary>
        /// <param name="inputSource"></param>
        /// <returns></returns>
        Vector2 CollectInputFromSource(VehicleInputSource inputSource)
        {
            Vector2 value = inputSource.GetInput();

            // Ensure input is in range [-1,1]
            value.x = Mathf.Clamp(value.x, -1, 1);
            value.y = Mathf.Clamp(value.y, -1, 1);

            return value;
        }
    }
}
