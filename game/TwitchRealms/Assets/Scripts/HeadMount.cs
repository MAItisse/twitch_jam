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
            try
            {
                MeshRenderer doorRenderer = gameObject.transform.parent.transform.parent.GetComponentInChildren<Door>().GetComponentsInChildren<MeshRenderer>()[1];
                Material doorMaterial = doorRenderer.material;
                Debug.Log("character is battle ready " + characterBuilder.isBattleReady);
                Debug.Log(doorMaterial.GetFloat("_Passable"));
                doorMaterial.SetFloat ("_Passable", characterBuilder.isBattleReady ? 1f : 0f);
                Debug.Log(doorMaterial.GetFloat("_Passable"));
            }
            catch (UnityException e)
            {
                Debug.Log(e);
            }
        }
    }
}
