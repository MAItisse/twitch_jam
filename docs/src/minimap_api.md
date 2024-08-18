# Api

## Game to Extension

### Css

format: `{"data": {"css": "..."}}`.

where the `css` key holds a css string that will be injected into the page.
Every time this event is recieved from the extension the previous css will be replaced.

### Units

messagge format: `{"data": [...]}`.

unit format: `{"id": "1234", "kind": "Sphere", "x": 0.34, "y": 0.35}`
* `id`: This is a unique id for the entity, this is also added as a css class to the entity in the format of `_id` (i.e in the example above it would be `_1234`)
* `kind`: This is a css class that will be added to the entity and is a nice way to reuse css across multiple entities,
* `x` & `y`: these are the positions of the entities, in the range 0-1. 0,0 being in the top left.

## Extension to Game

### Click
format: `{"x": 0.34, "y": 0.12, "userId": "12312", "bubbleColor": "#00ff00", "bubbleSize": 0.23, "itemType": "Random"}`
* `x` & `y`: position of the click in range 0-1, 0,0 in top left.
* `userId`: twitch id of the user who clicked
* `bubbleColor`: The user selected color
* `bubbleSize`: The user selected size
* `itemType`: User selected item, one of `Random`, `Sphere`, `Cube`
