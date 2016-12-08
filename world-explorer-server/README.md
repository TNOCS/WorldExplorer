# WorldExplorerServer
Tile server proxy for WorldExplorer, serving cached Mapzen vector tiles, or your own GeoJSON tiles. 
In addition, it can serve as an assetbundle server too (serving assets from the `public/assets` folder). Furthermore, it runs a small map application, with which you can specify the location on the map to look at in the Hololens.

In case you wish to have a collaborative session, please install an MQTT server too. For example, you can use:
```console
npm i -g mosca
mosca -p 8026 --http-port 8028 # or on Windows, use mosca_start.bat
```
Which starts an MQTT server on port 8026, and a WebSockets client on port 8028.

Features:
- Serve GeoJSON as tiles from the cache folder (folder can be configured in `config.json`). For example, when requesting [http://localhost:10733/buildings,roads,landuse,places,pois/17/67530/43625.json](http://localhost:10733/buildings,roads,landuse,places,pois/17/67530/43625.json), a JSON object is returned containing the keys `buildings`, `roads` etc. whose value is a GeoJSON feature collection.
- Download GeoJSON tiles from Mapzen if not in cache (API key needs to be configured in download url in `config.json`)
- Serve asset bundles from the `assets` folder. Read more [here](https://docs.unity3d.com/Manual/SettinguptheAssetServer.html) and [here](https://unity3d.com/unity/team-license).
- Specify your own assets as a GeoJSON FeatureCollection in `assets.json`. Each point feature in this GeoJSON is loaded, and served as tile to WorldExplorer (key = 'assets').

```json
{
    "geometry": {
      "type": "Point",
      "coordinates": [5.479269,51.436914]
    },
    "type": "Feature",
    "properties": {
      "asset": "catharina kerk",
      "remove_from": "buildings",
      "name": "Catharina kerk",
      "min_zoom": 16,
      "id": 271573902,
      "bundle": "http://localhost/buildings/eindhoven"
    }
  }
  ```
    - `coordinates`: where to place the 3D model
    - `asset`: name of the asset in the asset bundle (see below to read how to create your own asset bundle)
    - `remove_from`: name of the GeoJSON collection in the exported tile where we should remove an item from, e.g. the original building. Can be a string or array of strings.
    - `min_zoom` and `max_zoom`: when the requested zoom level is outside of these bounds, ignore this asset.
    - `id`: when iterating through all `remove_from` collections, remove all items that have this id, so the original building is not drawn too.
    - `bundle`: URL of the asset bundle server and endpoint (points to the `assets` folder). When sending the data to the Hololens app, it will be preceded with the IP address (unless you specify a full address already).

## Configuration
Before you can serve tiles, you need to [obtain a valid Mapzen key](https://mapzen.com/developers) and add it to `config.json` url value. 

## Installation
Assuming that you have a working node.js version installed, you also need to install `typescript` (`npm i typescript -g`) and `typings` (`npm i typings -g`). Now you can go to the folder and, install the dependencies with:

```console
npm install           # Install dependencies
typings install       # Install the typings files
tsc                   # Transpile the Typescript code (in the src folder) to Javascript (in the dist folder)
node dist\cli.js      # Run the server
```
Optionally, you can install the tile server locally running `npm link`, which makes the tile_server.cmd available. Please note that, if you have several IP addresses, the tile server's address (which is reported on startup) may not be the correct one. In that case, please start the server with:
```console
node dist\cli.js -s xxx.xxx.xxx.xxx   # Where XXX is your IP address
```
We need the correct address, since assets are served relative to your IP address. If this is reported incorrectly, you won't be able to download the asset bundles.

