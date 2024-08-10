using System.Collections.Generic;
using UnityEngine;

public class CharacterBuilder : MonoBehaviour
{
    private List<Combinable> character;
    private List<Vector3> body;
    private bool battleReady = false;

    private void Start()
    {
        character = new List<Combinable>();
        body = new List<Vector3>
        {
            new(0, .5f, 0),
            new(.2f, 1.4f, 0),
            new(.7f, 2.2f, 0),
            new(1.2f, 1.3f, 0),
            new(1.44f, .5f, 0),
            new(1.44f, 2.65f, 0),
            new(2.2f, 2.3f, 0),
            new(0, 2.7f, 0),
            new(-.9f, 2.2f, 0),
            new(.7f, 3.2f, 0),
            new(.7f, 4.1f, 0),
        };

        // Initialize with the existing Combinables
        Combinable[] initialCombinables = gameObject.GetComponentsInChildren<Combinable>();
        foreach (var combinable in initialCombinables)
        {
            AddCombinable(combinable);
        }
    }

    public void BattleReady(bool isBattleReady)
    {
        battleReady = isBattleReady;
    }

    public bool isBattleReady => battleReady;

    public void AddCombinable(Combinable combinable)
    {
        int partIndex = character.Count;
        Vector3 targetPosition;

        if (partIndex < body.Count)
        {
            // Use exact points
            targetPosition = body[partIndex];
        }
        else if (partIndex < 2 * (body.Count - 1))
        {
            // Use middle points between existing points
            int index = partIndex - body.Count;
            targetPosition = (body[index] + body[index + 1]) / 2.0f;
        }
        else
        {
            // Further refine positions
            int index = (partIndex - 2 * (body.Count - 1)) / 2;
            float t = (partIndex - 2 * (body.Count - 1)) % 2 == 0 ? 0.25f : 0.75f;
            targetPosition = Vector3.Lerp(body[index], body[index + 1], t);
        }

        // Set the relative positions
        combinable.transform.SetParent(transform, false);
        combinable.transform.localPosition = targetPosition;

        // Add to the list if we want to see this later
        combinable.gameObject.GetComponent<Collider>().enabled = false;
        character.Add(combinable);
    }

    public bool ContainsCombinable(Combinable combinable) { return character.Contains(combinable); }
}
