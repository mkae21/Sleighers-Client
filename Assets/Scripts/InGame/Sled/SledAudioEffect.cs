using UnityEngine;

public class SledAudioEffect : MonoBehaviour
{
#region PrivateVariables
        [SerializeField] private float minPitch = 0.25f;

        [SerializeField] private float maxPitch = 1.5f;

        [SerializeField] private float multiplier = 1.2f;

        private AudioSource audioSource;
        private Player player;
#endregion

#region PrivateMethod
        private void Awake()
        {
            player = GetComponentInParent<Player>();
            audioSource = GetComponent<AudioSource>();
        }

        private void Update()
        {
            if (player.moveVector == Vector3.zero || player.GetVelocity().magnitude < 0.5f)
                audioSource.pitch = 0;
            else if (player != null && audioSource != null)
                audioSource.pitch = Mathf.Lerp(minPitch, maxPitch, player.NormalizedForwardSpeed * multiplier);
        }
#endregion

#region PublicMethod
    public void SledAudioOff()
    {
        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }
#endregion
}