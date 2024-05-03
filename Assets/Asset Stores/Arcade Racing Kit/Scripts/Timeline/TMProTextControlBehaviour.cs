using System;
using TMPro;
using UnityEngine.Playables;

namespace Ilumisoft.ArcardeRacingKit.Timeline
{
    [Serializable]
    public class TMProTextControlBehaviour : PlayableBehaviour
    {
        // The text which is applied to the TMPro text component
        public string text;

        bool isFirstFrame = false;

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            isFirstFrame = true;
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            // We only want this behaviour being applied once, when it is started
            if (isFirstFrame == false)
            {
                return;
            }

            // Get the bound text component from the track
            var textComponent = playerData as TextMeshProUGUI;

            // Apply the text value
            if (textComponent != null)
            {
                textComponent.text = text;
            }

            isFirstFrame = false;
        }
    }
}