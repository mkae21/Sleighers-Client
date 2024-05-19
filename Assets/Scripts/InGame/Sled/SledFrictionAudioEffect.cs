using UnityEngine;

public class SledFrictionAudioEffect : MonoBehaviour
{
#region PrivateVariables
        [SerializeField] private float minPitch = 0.25f;
        [SerializeField] private float maxPitch = 1.5f;
        [SerializeField] private float multiplier = 1.2f;
        private Player player;
#endregion

#region PublicVariables
        public AudioSource sledFrictionAudioSource;
#endregion

#region PrivateMethod
    private void Awake()
        {
            player = GetComponentInParent<Player>();
            sledFrictionAudioSource = GetComponent<AudioSource>();
        }

        private void Update()
        {
            if (player.moveVector == Vector3.zero || player.GetVelocity().magnitude < 0.5f)
                sledFrictionAudioSource.pitch = 0;
            else if (player != null && sledFrictionAudioSource != null)
                sledFrictionAudioSource.pitch = Mathf.Lerp(minPitch, maxPitch, player.NormalizedForwardSpeed * multiplier);
        }
#endregion
}