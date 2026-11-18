using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class ItemSpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] spawnObjects;
    [SerializeField] float timeBetweenSpawnsSeconds;
    private float timeLeftTillSpawnSeconds = 100f;

    private void Start()
    {
        timeLeftTillSpawnSeconds = timeBetweenSpawnsSeconds;
    }

    // Update is called once per frame
    void Update()
    {
        timeLeftTillSpawnSeconds -= Time.deltaTime;
        if (timeLeftTillSpawnSeconds <= 0)
        {
            int spawnObject = Random.Range(0, spawnObjects.Length);
            GameObject currentObject = Instantiate(spawnObjects[spawnObject], transform.position, spawnObjects[spawnObject].transform.rotation);
            timeLeftTillSpawnSeconds = timeBetweenSpawnsSeconds;
        }
    }
}
