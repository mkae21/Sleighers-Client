using UnityEngine;

public class SledAudioEffect : MonoBehaviour
{
        Player player;
        [SerializeField]
        float minPitch = 0.25f;

        [SerializeField]
        float maxPitch = 1.1f;

        [SerializeField]
        float multiplier = 1.2f;

        AudioSource audioSource;

        private void Awake()
        {
            player = GetComponentInParent<Player>();
            audioSource = GetComponent<AudioSource>();
        }

        void Update()
        {
            if (player != null && audioSource != null)
            {
                audioSource.pitch = Mathf.Lerp(minPitch, maxPitch, player.NormalizedForwardSpeed * multiplier);
            }
        }
}
