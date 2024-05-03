using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Ilumisoft.Editor.ArcadeRacingKit
{
    public static class ScriptableObjectUtility
    {
        /// <summary>
        /// Returns the first found instance of the asset of the given type or returns null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Find<T>() where T : ScriptableObject
        {
            var loadedInstances = Resources.FindObjectsOfTypeAll<T>();

            // Search loaded instances first
            if (loadedInstances.Length > 0)
            {
                return loadedInstances[0];
            }
            // Otherwise look for all assets in the asset database
            else
            {
                return Load<T>().FirstOrDefault();
            }
        }

        /// <summary>
        /// Loads all assets in the AssetDatabase of the given type.
        /// Returns an empty list if none is found
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<T> Load<T>() where T : ScriptableObject
        {
            var result = new List<T>();

            var guids = AssetDatabase.FindAssets($"t: {typeof(T)}");

            for (int i = 0; i < guids.Length; i++)
            {
                if (TryLoadAsset<T>(guids[i], out var asset))
                {
                    result.Add(asset);
                }
            }

            return result;
        }

        /// <summary>
        /// Tries to load the scriptable object with the given guid and return true on success
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="guid"></param>
        /// <param name="asset"></param>
        /// <returns></returns>
        static bool TryLoadAsset<T>(string guid, out T asset) where T : ScriptableObject
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);

            return asset != null;
        }
    }
}