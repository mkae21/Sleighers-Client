using UnityEngine;

namespace Ilumisoft.ArcardeRacingKit.Input
{
    /// <summary>
    /// Base class of all vehicle input sources. To create a custom input source, inherit from this!
    /// </summary>
    public abstract class VehicleInputSource : MonoBehaviour
    {
        /// <summary>
        /// Whether the input source should be used or not
        /// </summary>
        public virtual bool IsEnabled => enabled;

        /// <summary>
        /// Returns the input data collected by the input source
        /// </summary>
        /// <returns></returns>
        public abstract Vector2 GetInput();
    }
}
