using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] GameObject enemyPrefab;
    [SerializeField] Vector3 spawnPosition;

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.P))
        {
            Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        }
    }
}
