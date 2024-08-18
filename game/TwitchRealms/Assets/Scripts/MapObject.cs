using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapObject : MonoBehaviour
{
    public Color mapColor = Color.white;
    public string extraCss = "";

    private void OnTriggerEnter(Collider other)
    {
        if (gameObject.TryGetComponent<Combinable>(out var combinable))
        {
            extraCss = "";
        }
    }
}
