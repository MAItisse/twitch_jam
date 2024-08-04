using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterBuilder : MonoBehaviour
{

    private List<Combinable> character;
    private List<Vector3> body;
    private bool battleReady = false;

    private void Start()
    {
        character = new List<Combinable>
        {
            gameObject.GetComponentInChildren<Combinable>()
        };
        body = new List<Vector3>
        {
            new(0,1.4f,.2f),
            new(0,2.2f, .7f),
            new(0,1.3f, 1.2f),
            new(0,.5f, 1.44f),
            new(0,2.65f, 1.44f),
            new(0,2.3f,2.2f),
            new(0,2.7f,0),
            new(0,2.2f,-.9f),
            new(0,3.2f, .7f),
            new(0,4.1f, .7f),
        };
    }

    public void BattleReady(bool isBattleReady)
    {
        battleReady = isBattleReady;
    }

    public bool isBattleReady => battleReady;
    public void AddCombinable(Combinable combinable)
    {
        int partIndex = character.Count - 1;
        Vector3 targetPosition;

        if (partIndex < body.Count)
        {
            // Step 1: Use exact points
            targetPosition = body[partIndex];
        }
        else if (partIndex < 2 * (body.Count - 1))
        {
            // Step 2: Use middle points between existing points
            int index = partIndex - body.Count;
            targetPosition = (body[index] + body[index + 1]) / 2.0f;
        }
        else
        {
            // Step 3: Further refine positions
            int index = (partIndex - 2 * (body.Count - 1)) / 2;
            float t = (partIndex - 2 * (body.Count - 1)) % 2 == 0 ? 0.25f : 0.75f;
            targetPosition = Vector3.Lerp(body[index], body[index + 1], t);
        }

        // Set the relative positions
        combinable.transform.SetParent(transform, false);
        combinable.transform.position = transform.position + targetPosition;

        // Add to the list if we want to see this later
        character.Add(combinable);
        combinable.gameObject.GetComponent<Collider>().enabled = false;
    }

    public bool ContainsCombinable(Combinable combinable) { return character.Contains(combinable); }
}
