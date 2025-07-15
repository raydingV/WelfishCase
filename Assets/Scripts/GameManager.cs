using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject platform;
    [SerializeField] private float spawnSpeed = 1.5f;
    private int newValue;
    private int randValue;

    private void Start()
    {
        StartCoroutine(spawnPlatform());
    }

    IEnumerator spawnPlatform()
    {
        do
        {
            newValue = Random.Range(-6, 7);
        } 
        while (Mathf.Abs(randValue - newValue) <= 3 && Mathf.Abs(randValue - newValue) >= 5);

        randValue = newValue;
        
        Instantiate(platform, new Vector2(transform.position.x + randValue, transform.position.y), Quaternion.identity);

        yield return new WaitForSeconds(2f);

        StartCoroutine(spawnPlatform());
    }
}
