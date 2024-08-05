using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossSpawner : MonoBehaviour
{
    public GameObject boss;

    private void Awake()
    {
        Instantiate(boss, transform.position, Quaternion.identity, transform.parent);
    }
}
