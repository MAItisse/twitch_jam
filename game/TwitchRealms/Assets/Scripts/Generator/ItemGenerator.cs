using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemGenerator : MonoBehaviour
{
    public float range;

    private MapConnector connector;
    private void Start()
    {
        connector = FindObjectOfType<MapConnector>();
    }

    public void GenerateGameObject(MapObject combinable)
    {
        Vector3 spawnPosition = transform.position + new Vector3(Random.Range(-range, range), 0.5f, Random.Range(-range, range));
        MapObject mapObject = Instantiate(combinable, spawnPosition, Quaternion.identity, transform.parent);
        mapObject.extraCss = combinable.extraCss;
        mapObject.mapColor = combinable.mapColor;
        connector.AddCombinable(mapObject);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, Vector3.one * range);
    }
}
