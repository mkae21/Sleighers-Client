using UnityEngine;
using UnityEngine.Events;

public class Obstacle : MonoBehaviour
{
#region PublicVariables
    public UnityAction<Player, Obstacle> OnPlayerEnterObstacle;
#endregion

#region PrivateMethod

    private void OnCollisionEnter(Collision _other) 
    {
        if(_other.gameObject.tag != "Player" && _other.gameObject.tag != "Obstacle")
            return;
        Player player = _other.gameObject.GetComponentInParent<Player>();

        if (player != null)
            OnPlayerEnterObstacle?.Invoke(player, this);
    }
#endregion
}
