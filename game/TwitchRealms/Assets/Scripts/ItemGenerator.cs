using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemGenerator : MonoBehaviour
{
    public float range;
    public void GenerateGameObject(GameObject go)
    {
        Vector3 spawnPosition = transform.position + new Vector3(Random.Range(-range, range), 0, Random.Range(-range, range));
        Instantiate(go, spawnPosition, Quaternion.identity, transform.parent);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, Vector3.one * range);
    }
}
