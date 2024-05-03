using UnityEditor;
using UnityEngine;

namespace Ilumisoft.ArcardeRacingKit.Editor.Internal
{
    public static class MenuItems
    {
        [MenuItem("Arcade Racing Kit/Documentation")]
        static void ShowDocumentation()
        {
            var config = EditorAssetInfo.Find();

            if (config != null)
            {
                if (!string.IsNullOrEmpty(config.DocumentationURL))
                {
                    Application.OpenURL(config.DocumentationURL);
                }
                else
                {
                    AssetDatabase.OpenAsset(config.Documentation);
                }
            }
        }

        [MenuItem("Arcade Racing Kit/Rate")]
        static void Rate()
        {
            var config = EditorAssetInfo.Find();

            if (config != null && !string.IsNullOrEmpty(config.RateURL))
            {
                Application.OpenURL(config.RateURL);
            }
        }

        [MenuItem("Arcade Racing Kit/More Assets")]
        static void MoreAssets()
        {
            var config = EditorAssetInfo.Find();

            if (config != null && !string.IsNullOrEmpty(config.MoreAssetsURL))
            {
                Application.OpenURL(config.MoreAssetsURL);
            }
        }
    }
}