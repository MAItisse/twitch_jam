using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemGenerator : MonoBehaviour
{
    public float range;

    private MapConnector connector;
    private void Start()
    {
        connector = GameObject.FindObjectOfType<MapConnector>();
    }

    public void GenerateGameObject(GameObject combinable)
    {
        Vector3 spawnPosition = transform.position + new Vector3(Random.Range(-range, range), 0.5f, Random.Range(-range, range));
        GameObject go = Instantiate(combinable, spawnPosition, Quaternion.identity, transform.parent);
        connector.AddCombinable(go.GetComponent<MapObject>());
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, Vector3.one * range);
    }
}
