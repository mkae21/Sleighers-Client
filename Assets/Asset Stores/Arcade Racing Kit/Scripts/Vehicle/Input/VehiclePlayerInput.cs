using UnityEngine;
using UnityEngine.InputSystem;

namespace Ilumisoft.ArcardeRacingKit.Input
{
    /// <summary>
    /// The default vehicle input source, collecting input from Unity's new input system via the PlayerInput component.
    /// To modify the input, you can simply edit the action scheme of the PlayertInput component.
    /// You can also create a custom input source by overriding the 'VehicleInputSource' class and attach the new component to the vehicle (e.g. to create AI input).
    /// </summary>
    public class VehiclePlayerInput : VehicleInputSource
    {
        Vector2 movementInput = Vector2.zero;

        /// <summary>
        /// Returns the input this input source has collected from the PlayerInput component. This method is mainly used by the vehicle, when it collects the input from the input sources.
        /// </summary>
        /// <returns></returns>
        public override Vector2 GetInput()
        {
            return movementInput;
        }

        /// <summary>
        /// This method is called by the PlayerInput component and updates the input data.
        /// </summary>
        /// <param name="movementValue"></param>
        public void OnMove(InputValue movementValue)
        {
            movementInput = movementValue.Get<Vector2>();
        }

        private void Reset()
        {
            // Automatically add and setup the PlayerInput component if none exists
            if(GetComponent<PlayerInput>() == null)
            {
                var playerInput = gameObject.AddComponent<PlayerInput>();

                // Use our default input scheme
                playerInput.actions = Resources.Load<InputActionAsset>("Ilumisoft/Arcade Racing Kit/Input");
            }
        }
    }
}
