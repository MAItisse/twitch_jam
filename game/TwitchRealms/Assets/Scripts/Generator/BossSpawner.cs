using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossSpawner : MonoBehaviour
{
    public MapObject boss;

    private void OnEnable()
    {
        Instantiate(boss, transform.position + new Vector3(0, .5f, 0), Quaternion.identity, transform.parent);
    }
}
