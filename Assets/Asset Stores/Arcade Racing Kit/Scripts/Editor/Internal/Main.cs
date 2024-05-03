using UnityEditor;
using UnityEngine;

namespace Ilumisoft.Editor.ArcadeRacingKit
{
    public static class Main
    {
        [InitializeOnLoadMethod]
        static void Initialize()
        {
            // Only show once per editor session
            if (!HasBeenShownThisSession())
            {
                // Remember that the popup window has been shown
                SessionState.SetBool(Config.Session.Key, true);

                EditorApplication.update += CreatePopupWindow;
            }
        }

        /// <summary>
        /// Creates the popup window
        /// </summary>
        static void CreatePopupWindow()
        {
            EditorApplication.update -= CreatePopupWindow;
            // Create the window if ShowAtStartup option is enabled
            if (TryGetPackageInfo(out var packageInfo) && packageInfo.ShowAtStartup)
            {
                if (!Application.isPlaying)
                {
                    WelcomeWindow.CreateWindow();
                }
            }
        }

        /// <summary>
        /// Returns true if the poup window has already been shown in the current editor session, false otherwise
        /// </summary>
        /// <returns></returns>
        static bool HasBeenShownThisSession()
        {
            return SessionState.GetBool(Config.Session.Key, false);
        }

        /// <summary>
        /// Tries to find the package info object and returns true on success, false otherwise
        /// </summary>
        /// <param name="packageInfo"></param>
        /// <returns></returns>
        static bool TryGetPackageInfo(out PackageInfo packageInfo)
        {
            packageInfo = ScriptableObjectUtility.Find<PackageInfo>();

            return packageInfo != null;
        }
    }
}