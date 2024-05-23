using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class ObstacleHolder : MonoBehaviour
{
#region PublicVariables
    public static ObstacleHolder instance;
    public Transform obstacleHolder;
    public Transform obstacleHolder2;
#endregion

#region PrivateVariables
    private List<Obstacle> obstacles;
    private bool isPlayingSound = false;
#endregion

#region PrivateMethod
    private void Awake()
    {
        if (!instance)
            instance = this;
    }

    public void SetObstacles(Dictionary<string, Player> _players)
    {
        int totalObstacles = obstacleHolder.childCount;
        int totalObstacles2 = obstacleHolder2.childCount;
        obstacles = new List<Obstacle>();
        
        for (int i = 0; i < totalObstacles; i++)
        {
            obstacles.Add(obstacleHolder.GetChild(i).GetComponent<Obstacle>());
        }

        for (int i= 0; i < totalObstacles2; i++)
        {
            obstacles.Add(obstacleHolder2.GetChild(i).GetComponent<Obstacle>());
        }
        
        foreach (KeyValuePair<string, Player> player in _players)
        {
            obstacles.Add(player.Value.GetComponentInChildren<Obstacle>());
        }

        foreach (Obstacle obstacle in obstacles)
        {
            obstacle.OnPlayerEnterObstacle += OnPlayerEnterObstacle;
        }
    }

    private void OnPlayerEnterObstacle(Player _player, Obstacle _obstacle)
    {
        if (!isPlayingSound && _player.isMe)
        {   
            StartCoroutine(PlaySoundCoroutine());
        }
    }
    
    private IEnumerator PlaySoundCoroutine()
    {
        isPlayingSound = true;
        GameManager.Instance().soundManager.Play("Effect/Crash", SoundType.EFFECT);

        yield return new WaitForSeconds(0.7f);
        isPlayingSound = false;
    }
#endregion
}