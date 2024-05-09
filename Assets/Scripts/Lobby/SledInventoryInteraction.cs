using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class SledInventoryInteraction : MonoBehaviour
{

#region PublicVariables
    public enum SledState
    {
        None,
        Has,
        Equipped
    }
    public Button right, left;
    public SledState currentState;
    public int currentIndex = 0;
    public List<SledState> sledInfoList;
#endregion

#region PrivateVariables
    private string[] btnText = { "구매", "장착", "장착중"};
#endregion

#region PrivateMethods
    private void Start()
    {
        sledInfoList = new List<SledState>
        {
            SledState.None,
            SledState.None,
            SledState.None,
            SledState.None,
        };

        currentState = sledInfoList[currentIndex];
        SetButton(currentState);
        left.interactable = false;

        right.onClick.AddListener(() => ChangeSled(1));
        left.onClick.AddListener(() => ChangeSled(-1));
    }

    private void SetButton(SledState state)
    {
        transform.GetChild(0).GetComponent<TMP_Text>().text = btnText[(int)state];
        if(state == SledState.Equipped)
            transform.GetComponent<Button>().interactable = false;
        else
            transform.GetComponent<Button>().interactable = true;
    }
    
#endregion

#region PublicMethods
    public void ChangeSled(int num)
    {
        if(currentIndex + num <= 0)
        {
            currentIndex = 0;
            left.interactable = false;
        }
        else if(currentIndex + num >= sledInfoList.Count - 1)
        {
            currentIndex = sledInfoList.Count - 1;
            right.interactable = false;
        }
        else
        {
            currentIndex += num;
            left.interactable = true;
            right.interactable = true;
        }

        currentState = sledInfoList[currentIndex];
        SetButton(currentState);
    }
    public void OnClick()
    {   
        ChangeSledState(currentIndex, currentState + 1);
    }
    public void ChangeSledState(int sledIdx, SledState state)
    {
        if(state <= 0){
            state = SledState.None;
        }
        else if(state >= SledState.Equipped){
            state = SledState.Equipped;
        }

        sledInfoList[sledIdx] = state; // TODO: DB에 반영
        currentIndex = sledIdx;
        currentState = state;

        SetButton(currentState);

        switch(currentState)
        {
            case SledState.None:
                /*                
                3. 구매한 썰매 데이터 저장
                */
                break;
            case SledState.Has:
                break;
            case SledState.Equipped:
                /*
                4. 현재 장착한 썰매 데이터를 이걸로 바꾸기
                */
                for(int i = 0; i < sledInfoList.Count; i++)
                {
                    if(i != sledIdx && sledInfoList[i] == SledState.Equipped)
                    {
                        sledInfoList[i] = SledState.Has;
                        break;
                    }
                }
                break;
        }
    }
#endregion
}

