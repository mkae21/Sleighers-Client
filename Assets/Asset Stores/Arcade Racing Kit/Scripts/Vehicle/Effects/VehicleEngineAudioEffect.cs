using UnityEngine;

namespace Ilumisoft.ArcardeRacingKit.Effects
{
    /// <summary>
    /// Effect creating a dynamic engine sfx effect by adjusting the pitch of the engine audio source depending on the velocity of the vehicle
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class VehicleEngineAudioEffect : VehicleComponent
    {
        [SerializeField]
        float minPitch = 0.25f;

        [SerializeField]
        float maxPitch = 1.1f;

        [SerializeField]
        float multiplier = 1.2f;

        AudioSource audioSource;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
        }

        void Update()
        {
            if (Vehicle != null && audioSource != null)
            {
                audioSource.pitch = Mathf.Lerp(minPitch, maxPitch, Vehicle.NormalizedForwardSpeed * multiplier);
            }
        }
    }
}