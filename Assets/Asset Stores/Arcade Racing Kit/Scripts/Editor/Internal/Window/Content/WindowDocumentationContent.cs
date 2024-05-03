using UnityEditor;
using UnityEngine;

namespace Ilumisoft.Editor.ArcadeRacingKit
{
    public class WindowDocumentationContent : WindowContent
    {
        public override void OnGUI()
        {
            GUILayout.Space(14);
            DrawHeadline("Get Started");

            GUILayout.BeginHorizontal();

            GUILayout.Label(new GUIContent("Documentation"));

            if (GUILayout.Button("Open", GUILayout.Width(100)))
            {
                if (!string.IsNullOrEmpty(PackageInfo.DocumentationURL))
                {
                    Application.OpenURL(PackageInfo.DocumentationURL);
                }
                else
                {
                    AssetDatabase.OpenAsset(PackageInfo.Documentation);
                }
            }

            GUILayout.EndHorizontal();
        }
    }
}