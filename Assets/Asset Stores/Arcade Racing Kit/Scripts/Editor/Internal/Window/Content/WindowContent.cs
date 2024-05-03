using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Ilumisoft.Editor.ArcadeRacingKit
{
    public abstract class WindowContent
    {
        public EditorWindow EditorWindow { get; set; }

        public PackageInfo PackageInfo { get; set; }

        public virtual void Initialize(EditorWindow editorWindow, PackageInfo packageInfo)
        {
            this.EditorWindow = editorWindow;
            this.PackageInfo = packageInfo;
        }

        public abstract void OnGUI();

        public void DrawHeadline(string text)
        {
            Rect line = GUILayoutUtility.GetRect(EditorWindow.position.width, 0);

            line.height = 22;

            EditorGUI.DrawRect(line, new Color(0.2f, 0.2f, 0.2f));

            GUILayout.Space(1);
            GUILayout.Label(text, EditorStyles.boldLabel);

            GUILayout.Space(8);
        }

        public static List<WindowContent> CreateInstances (Type[] types)
        {
            var result = new List<WindowContent>();

            foreach (var type in types)
            {
                if(type.IsSubclassOf(typeof(WindowContent)))
                {
                    var instance = Activator.CreateInstance(type) as WindowContent;

                    result.Add(instance);
                }
            }

            return result;
        }
    }
}