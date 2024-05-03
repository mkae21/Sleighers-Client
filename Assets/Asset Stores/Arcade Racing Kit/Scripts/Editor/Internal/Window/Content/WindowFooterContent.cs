using UnityEditor;
using UnityEngine;

namespace Ilumisoft.Editor.ArcadeRacingKit
{
    public class WindowFooterContent : WindowContent
    {
        public override void OnGUI()
        {
            GUILayout.FlexibleSpace();

            Rect line = GUILayoutUtility.GetRect(EditorWindow.position.width, 1);

            EditorGUI.DrawRect(line, Color.black);

            using (new GUILayout.HorizontalScope())
            {
                EditorGUI.BeginChangeCheck();
                bool show = GUILayout.Toggle(PackageInfo.ShowAtStartup, " Show on startup");

                if (EditorGUI.EndChangeCheck())
                {
                    PackageInfo.ShowAtStartup = show;
                }

                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Close"))
                {
                    EditorWindow.Close();
                }
            }
        }
    }
}