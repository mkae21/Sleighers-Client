using System;
using UnityEngine;
using UnityEngine.Playables;

namespace Ilumisoft.ArcardeRacingKit.Timeline
{
    /// <summary>
    /// Custom playable asset allowing to control TMPro Text via timeline
    /// </summary>
    [Serializable]
    public class TMProTextControlAsset : PlayableAsset
    {
        public string text;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<TMProTextControlBehaviour>.Create(graph);

            var textBehaviour = playable.GetBehaviour();
            textBehaviour.text = text;

            return playable;
        }
    }
}