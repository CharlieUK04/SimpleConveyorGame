using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class ItemSpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] spawnObjects;
    [SerializeField] private float TimeBetweenSpawnsSeconds;
    private float TimeUntilNextSpawnSeconds = 100f;

    private void Start()
    {
        TimeUntilNextSpawnSeconds = TimeBetweenSpawnsSeconds;
    }

    // Update is called once per frame
    void Update()
    {
        TimeUntilNextSpawnSeconds -= Time.deltaTime;
        if (TimeUntilNextSpawnSeconds <= 0)
        {
            int spawnObject = Random.Range(0, spawnObjects.Length);
            GameObject currentObject = Instantiate(spawnObjects[spawnObject], transform.position, spawnObjects[spawnObject].transform.rotation);
            TimeUntilNextSpawnSeconds = TimeBetweenSpawnsSeconds;
        }
    }
}
