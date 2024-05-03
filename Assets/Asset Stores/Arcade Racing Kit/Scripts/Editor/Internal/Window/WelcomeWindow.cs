using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Ilumisoft.Editor.ArcadeRacingKit
{
    public class WelcomeWindow : EditorWindow
    {
        [NonSerialized]
        List<WindowContent> contentList;

        public static void CreateWindow()
        {
            var window = GetWindow<WelcomeWindow>(utility: true);

            window.titleContent = new GUIContent(Config.Window.Title);
            window.maxSize = Config.Window.Size;
            window.minSize = Config.Window.Size;
        }

        private void OnEnable()
        {
            contentList = WindowContent.CreateInstances(Config.Window.Content);

            var packageInfo = ScriptableObjectUtility.Find<PackageInfo>();

            foreach (var content in contentList)
            {
                content.Initialize(this, packageInfo);
            }
        }

        void OnGUI()
        {
            foreach(var content in contentList)
            {
                content.OnGUI();
            }
        }
    }
}