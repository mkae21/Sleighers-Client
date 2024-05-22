using UnityEngine;

public class EndPoint : MonoBehaviour
{
    private void OnTriggerEnter(Collider _other)
    {
        if(_other.gameObject.tag == "End")
        {
            GameObject.FindWithTag("Map2").SetActive(false);
        }
    }
}
