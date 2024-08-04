using UnityEngine;
using TwitchSDK;
using TwitchSDK.Interop;

public class CheerManager : MonoBehaviour
{
    private GameTask<EventStream<ChannelCheerEvent>> _cheerEvents;
    // Start is called before the first frame update
    private void Start()
    {
        _cheerEvents = Twitch.API.SubscribeToChannelCheerEvents();
    }

    // Update is called once per frame
    private void Update()
    {

        if (_cheerEvents.MaybeResult.TryGetNextEvent(out var curCheerEvent))
        {
            // Do something
            Debug.Log($"{curCheerEvent.UserDisplayName} has cheered with {curCheerEvent.Bits} and they said {curCheerEvent.Message}!");
        }
    }
}
