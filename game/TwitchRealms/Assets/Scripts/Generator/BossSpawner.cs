using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossSpawner : MonoBehaviour
{
    public GameObject boss;
    public string bossName = "Bozz";

    private void Awake()
    {
        var go = Instantiate(boss, transform.position + new Vector3(0, .5f, 0), Quaternion.identity, transform.parent);
        var mapObject = go.AddComponent<MapObject>();
        mapObject.cssClassName = bossName;
        mapObject.mapColor = Color.red;
    }
}
