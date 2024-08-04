using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Combinable : MonoBehaviour
{
    private Transform previousTransform;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<CharacterBuilder>(out var characterBuilder)) 
        {
            if (!characterBuilder.ContainsCombinable(this))
            {
                characterBuilder.AddCombinable(this);
            }
        }
    }
}
