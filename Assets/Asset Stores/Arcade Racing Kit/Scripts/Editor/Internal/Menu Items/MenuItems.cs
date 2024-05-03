using UnityEditor;
using UnityEngine;

namespace Ilumisoft.Editor.ArcadeRacingKit
{
    public static class MenuItems
    {
        [MenuItem(Config.MenuItems.PathOpenWindow, priority =-1)]
        static void OpenPackageUtility()
        {
            WelcomeWindow.CreateWindow();
        }
    }
}