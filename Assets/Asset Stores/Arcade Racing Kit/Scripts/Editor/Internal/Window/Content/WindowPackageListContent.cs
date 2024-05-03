using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Ilumisoft.Editor.ArcadeRacingKit
{
    public class WindowPackageListContent : WindowContent
    {
        List<PackageData> packageList;

        public override void Initialize(EditorWindow editorWindow, PackageInfo packageInfo)
        {
            base.Initialize(editorWindow, packageInfo);

            LoadPackageList();
        }

        private void LoadPackageList()
        {
            packageList = ScriptableObjectUtility.Load<PackageData>();
        }

        public override void OnGUI()
        {
            var headerStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                wordWrap = true
            };

            GUILayout.Space(14);

            DrawHeadline("Upgrade Options");

            GUILayout.Label($"You can upgrade with a discount to the following packages:", headerStyle);
            GUILayout.Space(8);

            foreach (var package in packageList)
            {
                using (var horizontalScope = new GUILayout.HorizontalScope())
                {
                    GUILayout.Label(new GUIContent(package.Name));

                    if (GUILayout.Button("Show", GUILayout.Width(100)))
                    {
                        Application.OpenURL(package.AssetStoreURL);
                    }
                }
            }
        }
    }
}