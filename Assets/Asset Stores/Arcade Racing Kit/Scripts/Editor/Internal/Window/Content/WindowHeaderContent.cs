using UnityEditor;
using UnityEngine;

namespace Ilumisoft.Editor.ArcadeRacingKit
{
    public class WindowHeaderContent : WindowContent
    {
        public override void OnGUI()
        {
            var headerStyle = new GUIStyle(EditorStyles.label)
            {
                wordWrap = true
            };

            GUILayout.Label(new GUIContent(PackageInfo.PackageImage));

            GUILayout.Label($"Welcome to {PackageInfo.PackageTitle}! We hope you will enjoy this package.\nTo get a quick overview about the project please check out the get started guide.\n\nIf you like this asset, we would be happy if you would take the time to give us a rating later on and check out our other games and assets in the Asset Store!", headerStyle);
        }
    }
}