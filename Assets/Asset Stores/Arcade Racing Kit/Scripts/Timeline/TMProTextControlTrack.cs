using UnityEngine.Timeline;
using TMPro;

namespace Ilumisoft.ArcardeRacingKit.Timeline
{
    /// <summary>
    /// Custom track asset allowing to control TMPro Text via timeline
    /// </summary>
    [TrackClipType(typeof(TMProTextControlAsset))]
    [TrackBindingType(typeof(TextMeshProUGUI))]
    public class TMProTextControlTrack : TrackAsset {}
}