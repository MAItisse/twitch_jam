using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    GameStateManager stateManager;
    // Start is called before the first frame update
    void Start()
    {
        var colloder = gameObject.AddComponent<BoxCollider>();
        colloder.isTrigger = true;
        stateManager = FindObjectOfType<GameStateManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<CharacterBuilder>(out var character))
        {
            if (character.isBattleReady)
            {
                stateManager.LoadScene("Arena");
            }
        }
        // else we should play a sound or something
    }
}
