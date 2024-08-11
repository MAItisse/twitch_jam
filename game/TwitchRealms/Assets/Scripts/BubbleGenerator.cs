using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubbleGenerator : MonoBehaviour
{
    public Material baseMaterial;
    public GameObject bubblePrefab;

    public void SpawnBubble(string color, float size, Vector2 position)
    {
        StartCoroutine(SpawnBubbleCoroutine(color, size, position));
    }

    private IEnumerator SpawnBubbleCoroutine(string color, float size, Vector2 position)
    {
        yield return new WaitForEndOfFrame(); // Ensure this runs on the main thread

        // Instantiate the bubble at the given position
        GameObject bubble = Instantiate(bubblePrefab, new Vector3(position.x, 0, position.y), Quaternion.identity);

        // Set bubble size
        Bubble bubbleComp = bubble.GetComponent<Bubble>();
        if (bubbleComp != null)
        {
            bubbleComp.size = size;
        }
        else
        {
            Debug.LogError("The instantiated object does not have a Bubble component.");
        }

        // Create a new material instance
        Material material = new Material(baseMaterial);

        // Parse the color string
        Color matColor;
        if (ColorUtility.TryParseHtmlString(color, out matColor))
        {
            material.color = matColor;
        }
        else
        {
            Debug.LogWarning("Invalid color string: " + color);
            material.color = Color.white; // Set a default color if the string is invalid
        }

        // Apply the new material to the bubble's MeshRenderer
        MeshRenderer renderer = bubble.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.material = material;
        }
        else
        {
            Debug.LogError("The instantiated object does not have a MeshRenderer component.");
        }
    }
}
