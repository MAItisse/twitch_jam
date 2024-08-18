# Game Side

First do a post request to `/lobby/new?user=123`, where `123` is the channel id of the lobby that should be created. The body of the request will be a simple plaintext key you should store for the next step.

Establish a websocket connection to `/lobby/connect/streamer?user=123&key=your_key`, where `your_key` is the key you got in the last step. You will now recieve any messages sent by an extension, and the extension will get any messages you send.

For the expected format for the minimap extension see [Minimap Api](minimap_api.md)
