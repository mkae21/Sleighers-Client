using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleHolder : MonoBehaviour
{
#region PublicVariables
    public Transform obstacleHolder;
#endregion

#region PrivateVariables
    private List<Obstacle> obstacles;
    private bool isPlayingSound = false;
#endregion

#region PrivateMethod
    private void Start()
    {
        int totalObstacles = obstacleHolder.childCount;
        SetObstacles();
        foreach (Obstacle obstacle in obstacles)
        {
            obstacle.OnPlayerEnterObstacle += OnPlayerEnterObstacle;
        }
    }
    private void SetObstacles()
    {
        obstacles = new List<Obstacle>();
        for (int i = 0; i < obstacleHolder.childCount; i++)
        {
            obstacles.Add(obstacleHolder.GetChild(i).GetComponent<Obstacle>());
        }
    }
    private void OnPlayerEnterObstacle(Player _player, Obstacle _obstacle)
    {
        if (!isPlayingSound)
        {
            StartCoroutine(PlaySoundCoroutine());
        }
    }
    
    private IEnumerator PlaySoundCoroutine()
    {
        isPlayingSound = true;
        GameManager.Instance().soundManager.Play("Effect/Crash", SoundType.EFFECT);

        yield return new WaitForSeconds(1.0f);
        isPlayingSound = false;
    }
#endregion
}