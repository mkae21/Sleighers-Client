using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class RankElement : MonoBehaviour
{
#region PrivateVariables
    private TMP_Text rankText;
    private TMP_Text nicknameText;
    private static readonly Vector3 myScale = new Vector3(1.1f, 1.1f, 1.1f);
    private static readonly Vector2 myPivot = new Vector2(0f, 0.5f);
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
        Color imageColor =  new Color(221f / 255f, 122f / 255f, 53f / 255f, 1f);
        GetComponentsInChildren<Image>()[1].color = imageColor;
        nicknameText.color = Color.black;

        transform.localScale = myScale;
        RectTransform rectTransform = GetComponent<RectTransform>();
        rectTransform.pivot = myPivot;
    }
#endregion
}
