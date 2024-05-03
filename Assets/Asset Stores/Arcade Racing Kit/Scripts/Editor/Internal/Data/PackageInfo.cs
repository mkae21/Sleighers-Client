namespace Ilumisoft.Editor.ArcadeRacingKit
{
    using UnityEngine;

    public class PackageInfo : ScriptableObject
    {
        /// <summary>
        /// Image of the package
        /// </summary>
        [SerializeField]
        public Texture PackageImage = null;

        /// <summary>
        /// Title of the package
        /// </summary>
        [SerializeField]
        public string PackageTitle = string.Empty;

        /// <summary>
        /// Asset ID of the package
        /// </summary>
        [SerializeField]
        public string PackageID = string.Empty;

        /// <summary>
        /// Reference to the documentation of the package
        /// </summary>
        [SerializeField]
        public Object Documentation = null;

        /// <summary>
        /// Link to the online documentation
        /// </summary>
        [SerializeField]
        public string DocumentationURL = string.Empty;

        /// <summary>
        /// Defines whether the welcome window should be shown at startup
        /// </summary>
        [SerializeField]
        public bool ShowAtStartup;

        /// <summary>
        /// Gets the asset store URL of the package
        /// </summary>
        public string AssetStoreURL => $"https://assetstore.unity.com/packages/slug/{PackageID}?aid=1100l9P3D";
    }
}