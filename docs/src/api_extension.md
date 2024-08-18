# Extension Side

The extension/client should establish a websocket connection to `/lobby/connect?user=123`, where `123` is the user id of the twitch channel the extension is running on.

You will now get any messages sent by the game, and the game will get any messages you send over the connection.

## Example

```js
window.Twitch.ext.onAuthorized((auth) => {
    const wsUrl = `wss://${HOST}/lobby/connect?user=${auth.channelId}`;
    socket = new WebSocket(wsUrl);

    socket.addEventListener("open", function (event) {
      console.log("Connected to the WebSocket server");
    });

    socket.addEventListener("message", function (event) {
      let data = JSON.parse(event.data);
      console.log(data);
    });
})
```

