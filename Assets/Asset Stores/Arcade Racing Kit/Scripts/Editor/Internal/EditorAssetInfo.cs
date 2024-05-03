using UnityEngine;

namespace Ilumisoft.ArcardeRacingKit.Editor.Internal
{
    /// <summary>
    /// Scriptable Object containing information (like link to the documentation) of the asset
    /// </summary>
    public class EditorAssetInfo : ScriptableObject
    {
        [SerializeField]
        Object documentation;

        [SerializeField]
        string documentationURL = string.Empty;

        [SerializeField]
        string rateURL = string.Empty;

        [SerializeField]
        string moreAssetsURL = string.Empty;

        public Object Documentation { get => this.documentation; set => this.documentation = value; }
        public string RateURL { get => this.rateURL; set => this.rateURL = value; }
        public string MoreAssetsURL { get => this.moreAssetsURL; set => this.moreAssetsURL = value; }
        public string DocumentationURL { get => this.documentationURL; set => this.documentationURL = value; }

        public static EditorAssetInfo Find() => ScriptableObjectUtility.Find<EditorAssetInfo>();
    }
}