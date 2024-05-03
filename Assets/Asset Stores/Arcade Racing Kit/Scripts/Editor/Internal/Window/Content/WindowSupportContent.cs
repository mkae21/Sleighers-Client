using UnityEditor;
using UnityEngine;

namespace Ilumisoft.Editor.ArcadeRacingKit
{
    public class WindowSupportContent : WindowContent
    {
        public override void OnGUI()
        {
            var headerStyle = new GUIStyle(EditorStyles.label)
            {
                wordWrap = true
            };

            GUILayout.Space(14);
            DrawHeadline("Support");
            GUILayout.Label($"If you have any question, feel free to get in touch with us via support@ilumisoft.de", headerStyle);
        }
    }
}