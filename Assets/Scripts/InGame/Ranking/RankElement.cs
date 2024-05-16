using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class RankElement : MonoBehaviour
{
#region PrivateVariables
    private TMP_Text rankText;
    private TMP_Text nicknameText;
#endregion

#region PrivateMethod
    private void Awake()
    {
        rankText = transform.Find("RankText").GetComponent<TMP_Text>();
        nicknameText = transform.Find("NicknameText").GetComponent<TMP_Text>();
    }
#endregion

#region PublicMethod
    public void SetRankElement(int rank, string nickname)
    {
        rankText.text = rank.ToString();
        nicknameText.text = nickname;
    }
    public void SetMyRankElement()
    {
        Color myColor = GetComponent<Image>().color;
        myColor.a = 1f;
        GetComponent<Image>().color = myColor;
        nicknameText.color = Color.black;
    }
#endregion
}
