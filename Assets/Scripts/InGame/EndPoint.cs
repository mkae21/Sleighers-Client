using UnityEngine;

public class EndPoint : MonoBehaviour
{
    public GameObject map2;
    private void OnTriggerEnter(Collider _other)
    {
        if(_other.gameObject.tag == "Player" && _other.GetComponentInParent<Player>().isMe)
            map2.SetActive(false);
    }
}
