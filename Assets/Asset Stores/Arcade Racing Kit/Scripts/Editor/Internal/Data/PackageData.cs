namespace Ilumisoft.Editor.ArcadeRacingKit
{
    using UnityEngine;

    public class PackageData : ScriptableObject
    {
        /// <summary>
        /// The asset ID of the package
        /// </summary>
        [SerializeField]
        public string ID = string.Empty;

        /// <summary>
        /// The visible name of the package
        /// </summary>
        public string Name => name; 

        /// <summary>
        /// Gets the asset store URL of the package
        /// </summary>
        public string AssetStoreURL => $"https://assetstore.unity.com/packages/slug/{ID}?aid=1100l9P3D";
    }
}