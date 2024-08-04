using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadMount : MonoBehaviour
{
    public bool isEquipped = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<CharacterBuilder>(out var characterBuilder))
        {
            isEquipped = true;
            characterBuilder.BattleReady(true);
        }
    }
}
