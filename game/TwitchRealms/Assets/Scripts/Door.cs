using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    GameStateManager stateManager;
    MapConnector connector;
    public GameObject locationToGo;
    // Start is called before the first frame update
    void Start()
    {
        var colloder = gameObject.AddComponent<BoxCollider>();
        colloder.isTrigger = true;
        stateManager = FindObjectOfType<GameStateManager>();
        connector = FindObjectOfType<MapConnector>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<CharacterBuilder>(out var character))
        {
            if (character.isBattleReady)
            {
                gameObject.transform.parent.gameObject.SetActive(false);
                locationToGo.SetActive(true);
                character.transform.parent = locationToGo.transform;
                connector.UpdateMapWorld();
            }
        }
        // else we should play a sound or something
    }
}
