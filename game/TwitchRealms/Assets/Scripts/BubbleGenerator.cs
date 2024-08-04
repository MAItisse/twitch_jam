using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubbleGenerator : MonoBehaviour
{
    public Material baseMaterial;
    public GameObject bubblePrefab;
    
    public void SpawnBubble(string color, float size, Vector2 position) {
        Vector3 spawnPosition = transform.position + new Vector3(position.x, 0, position.y);
        GameObject bubble = Instantiate(bubblePrefab, spawnPosition, Quaternion.identity, transform.parent);
        Bubble bubbleComp = bubble.AddComponent<Bubble>();
        bubbleComp.size = size;
        Material material = new Material(baseMaterial);
        Color matColor = new Color(0,0,0);
        ColorUtility.TryParseHtmlString(color, out matColor);
        material.color = matColor;
        MeshRenderer renderer = bubble.GetComponent<MeshRenderer>();
        renderer.material = material;
    }
}
