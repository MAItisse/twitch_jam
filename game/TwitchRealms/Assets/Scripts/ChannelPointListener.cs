using System.Collections.Generic;
using UnityEngine;
using TwitchSDK;
using TwitchSDK.Interop;

public class ChannelPointListener : MonoBehaviour
{
    private GameTask<EventStream<CustomRewardEvent>> _customRewardEvents;
    private Dictionary<string, string> _viewers = new();
    private ItemGenerator _itemGenerator;
    public Combinable spawnable;

    private void Start()
    {
        _customRewardEvents = Twitch.API.SubscribeToCustomRewardEvents();
        _itemGenerator = GameObject.FindObjectOfType<ItemGenerator>();
    }

    private void Update()
    {
        try
        {
            _customRewardEvents.MaybeResult.TryGetNextEvent(out var curRewardEvent);
            if (curRewardEvent == null) return;
            //Do something
            Debug.Log($"{curRewardEvent.RedeemerName} has brought {curRewardEvent.CustomRewardTitle} for {curRewardEvent.CustomRewardCost} {curRewardEvent.UserInput}!");
            switch (curRewardEvent.CustomRewardTitle)
            {
                case "Add Sphere To World":
                    _itemGenerator.GenerateGameObject(spawnable.gameObject);
                    break;
                default:
                    Debug.Log("Reward not found!");
                    break;
            }
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
            _customRewardEvents = Twitch.API.SubscribeToCustomRewardEvents();
        }
    }
}
