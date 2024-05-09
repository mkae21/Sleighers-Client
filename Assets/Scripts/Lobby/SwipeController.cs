using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwipeController : MonoBehaviour
{

#region PrivateVariables
    [SerializeField] private int maxPage;
    private int currentPage;
    private Vector3 targetPos;
    [SerializeField] private Vector3 pageStep;
    [SerializeField] private RectTransform levelSledRect;
    [SerializeField] private float tweenTime;
    [SerializeField] private LeanTweenType tweenType;
#endregion

#region PrivateMethods
    private void Awake()
    {
        currentPage = 1;
        targetPos = levelSledRect.localPosition;
    } 
    void MovePage()
    {
        levelSledRect.LeanMoveLocal(targetPos,tweenTime).setEase(tweenType);
    }
#endregion

#region 
    public void Next()
    {
        if(currentPage < maxPage)
        {
            currentPage++;
            targetPos += pageStep;
            MovePage();
        }
    }

    public void Previous()
    {
        if(currentPage > 1)
        {
            currentPage--;
            targetPos -= pageStep;
            MovePage();
        
        }
    }
#endregion
}
