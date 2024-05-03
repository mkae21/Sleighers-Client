using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Ilumisoft.ArcardeRacingKit.UI
{
    /// <summary>
    /// Reloads the current scene when the attached button component is clicked
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class RestartButton : MonoBehaviour
    {
        Button button;

        private void Awake()
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(OnClick);
        }

        private void OnClick()
        {
            SceneManager.LoadScene(gameObject.scene.name);
        }
    }
}