using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Combinable : MonoBehaviour
{
    private WebSocketManager websocket;

    private void Start()
    {
        websocket = GameObject.FindObjectOfType<WebSocketManager>();
    }

    private IEnumerator UpdateLobby()
    {
        while (websocket != null)
        {
            yield return new WaitForSeconds(1);
            websocket.SendMessage("{" + "'x:'" + transform.position.x + ", 'y':" + transform.position.z + "}");
        }
    }
   private void OnTriggerEnter(Collider other)
    {
        if (other.transform.parent != null && other.transform.parent.TryGetComponent<CharacterBuilder>(out var characterBuilder)) 
        {
            if (!characterBuilder.ContainsCombinable(this))
            {
                characterBuilder.AddCombinable(this);
            }
        }
    }
}
