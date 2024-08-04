using UnityEngine;
using TwitchSDK;
using TwitchSDK.Interop;

public class RaidManager : MonoBehaviour
{
    private GameTask<EventStream<ChannelRaidEvent>> _raidEvents;
    // Start is called before the first frame update
    private void Start()
    {
        _raidEvents = Twitch.API.SubscribeToChannelRaidEvents();
    }

    // Update is called once per frame
    private void Update()
    {

        if (_raidEvents.MaybeResult.TryGetNextEvent(out var curRaidEvent))
        {
            // Do something
            Debug.Log($"{curRaidEvent.FromBroadcasterName} has raided with {curRaidEvent.Viewers} people!");
        }
    }
}
