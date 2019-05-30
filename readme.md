# WorldExplorer
A Hololens application to increase your situational awareness. Using OpenStreetMap data, it renders the world. You can share a session with multiple users, editing layers of information. Even though we can share our world, different users can display different layers of information.
Is it based on [MapzenGo](https://github.com/brnkhy/MapzenGo), to render 3D worlds as hologram. [Video 1](https://youtu.be/dyVc-hZAiek), [Video 2](https://vimeo.com/187078876). 

[Thom van de Moosdijk](http://www.thomvdm.com/tno) extended this project for his graduation at Fontys Hogeschool in Eindhoven, and especially improved the user interaction, and his final presentation can be seen [here](https://vimeo.com/253818533). We also gave a presentation at the [Innovation in Defence 2017](https://vimeo.com/253819446).

# Features
- Render the world based on several datasets.
- Collaboration between users around the work, allowing them to edit the maps together.
- Navigation around the world, including multiple zoom levels.
- Visualization of 3D buildings and objects.
- Dynamic user interface and compass system.
- Load buildings and objects based on JSON data.
- Visualize terrain heights based on JSON data.
- Voice commands.
- Interaction:
	- Place, move, copy, delete and edit objects.
	- Move, scale and rotate the table.
	- Zoom and pan to navigate on the map.
	- Switch between bookmarked locations.
	- An inventory system.

![Example](https://cloud.githubusercontent.com/assets/3140667/21022014/5d8de41e-bd7b-11e6-99f5-6e265df12726.png)
### Prerequisites
Required software:

* Unity 2018.2.18f1 

- Windows SDK **10.0.17134** is required for 2017.2+
- Unity component "Windows store .NET scripting backend" and "Windows store IL2CPP Scripting backend" (option in unity installer)
- Visual Studio 2017.
- Windows 10 Fall Creators Update.

Required hardware:
- Microsoft HoloLens.
- Clicker (suggested).

# Mapzen

The company Mapzen has shutdown its hosted APIs and all related support and services on February 1, 2018.  

Alternatives are:

- [Nextzen](https://mapzen.com/blog/long-term-support-mapzen-maps/) (A long-term support version of own own Tilezen)
- [Mapbox](https://www.mapbox.com/maps/)
- [OpenMapTiles](https://openmaptiles.org/)
- [Thunderforest](https://thunderforest.com/docs/vector-maps-api/)
- [MAPCAT](http://mapcat.com/)

**Nextzen** - I was so lucky to get an api key for Nextzen. The url format for the geoJSON  is "https://tile.nextzen.org/tilezen/vector/v1/all/{zoom}/{x}/{y}.json?api_key=<key>". All layer types are returned, it not possible to request a section of the types (e.g. buildings, roads). A workaround is to use the proxy server 'world-explorer-server' provided in this project (this will download all layers and split them up into multiple layers). The new service returns also MultiPolygon, which are not parsed in current application.

**OpenMapTiles** - The OpenMapTiles server implementation can be found on https://hub.docker.com/r/klokantech/openmaptiles-server/ To run the server use the docker command "docker run --rm -it -v c:/openmaptiles/data:/data -p 8080:80 klokantech/openmaptiles-server". Then open a webpage on http://localhost:8080/". After setup the geoJSON can be retrieved with "http://localhost:8080/data/v3/{zoom}/{x}/{y}.geojson". However this geoJSON format doesn't match the mapzen geoJSON format, and can therefore not be used in this application (without rewrite some code). 



## Getting Started - Development
1. Open Unity (if there is a version mismatch warning, make sure there is a backup and automatically upgrade the project).
2. Open the MainScene.scene.

The `Managers` gameobject contains many script that allow for easy editing, debugging and adding voice commands.

The generation of buildings and objects are done through Factories.
Interaction is called through the relevant InputHandlers.
Interaction and user interface functionalities are performed through the relevant scripts in the Scripts/Interaction folder.

## Getting Started - Deploying to the HoloLens
1. Press CTRL-B (Build Project).
2. Add the current scene.
3. Make sure the following settings are selected:
	- `Universal Windows Platform`.
	- Target device: `HoloLens`.
	- Build Type: `D3D`.
	- SDK: `Latest installed`.
	- Build and run on: `Local machine`.
	- Unity C# projects checked (for debugging purposes only).
4. Make sure "Virtual Reality Supported" is checked under Player Settings.
5. Press Build and select the App folder, and choose a name.
6. Open the Visual Studio Solution in the created folder.
7. Build as `Release`, `x86` and either to `Device` (when deploying over USB) or `Remote Machine` (when deploying over Wi-Fi). 

NOTE: In `initialize.cs`, the config.json is specified. Replace this with a version of your own if you want to edit anything related to the configuration.

## Creating AssetBundles
In order to show 3D models, you need to create an asset bundle that contains these models. Currently, I do the following:
- Go to [3dwarehouse.sketchup.com](3dwarehouse.sketchup.com) and download your models
- Create a new Unity project
- Optionally, if you cannot open them in Unity, or the model has a terrain: Open them in Sketchup, e.g. to remove the underground (unlock the model), and save them as Sketchup 2015 project
- Import the model into Unity (menu Assets|Import New Asset...): in the inspector, underneath the image, specify a name for the asset bundle (e.g. buildings/eindhoven). If you don't specify the name, it won't be exported. Also note the geo location in the inspector, which should be used in the `assets.json` file in the `world-explorer-server` project folder.
- Optionally, change the model name, as this is how you load it from Unity: e.g. I only use lowercase names.
- Import all models until you are done.
- In the `Assets` folder, create a new folder named `AssetBundles` (the name used in the script below).
- Create a script with the following content: it will add a new command, `Build AssetBundles`, to the `Assets` menu. Click it to build your asset bundle. You will find your asset bundle in the created folder. Copy it to world-explorer-server assets folder (or any FTP server) to use it.

```C#
#if UNITY_EDITOR
using UnityEditor;

public class CreateAssetBundles
{
    [MenuItem("Assets/Build AssetBundles")]
    static void BuildAllAssetBundles()
    {
        BuildPipeline.BuildAssetBundles("Assets/AssetBundles", BuildAssetBundleOptions.None, BuildTarget.WSAPlayer);
    }
}
#endif
```

## Configuration file
[Pastebin](https://pastebin.com/ECm6yGM2)
```
userName : the user name for shared sessions.
selectionColor : color of the cursor.
tileServer : server that provided the tiles.
heightServer : server that provides height data of each tile.
vmgBuildingServer : server that provided JSON data of buildings in the german "virtuele missie gebieden" (german training grounds).
vmgObjectServer : server that provided JSON data of objects in the german "virtuele missie gebieden" (german training grounds).
mqttServer : the server used for MQTT connection between users.z
mqttPort : the port used for the MQTT connection.
initialView : the view loaded upon launch of application.
sessionName : name of the Session.
table : size and height of the table.
```

- `layers` are the layers used for the tile images.
- `view` is the bookmark locations: 
	- `Lat` and `Lon` can be changed to adjust the location. 
	- Zoom levels can changed to adjust initial zoom level.
	- `tileSize` and `range` can be changed to change the size of the tiles.
	- `mapzen`, `layers` and `tileLayers` can be adjusted to enable or disable certain visualizations.
	- `terrainHeightAvailable` should be enabled when terrain height data is available.

## Adding inventory objects
To add inventory objects.

1. Import new object (.fbx preferred) into the Resources/Prefabs/Inventory map folder.
2. Copy all components of any other inventory object to new object.
3. Check if the scale is realistic.
4. Add the object to the inventory in hierarchy as a new item (break prefab link). Make sure the name matches.

## Additional notes
- Button names should always end in ‘Btn’.
- Buttons should always have the tag `uibutton`.
- Objects with the tag `uistatic` are ignored by raycasts and other interactions.
- The `Managers` object, or a child of this object, contains all singleton scripts. A few exceptions exist (like the `initialize.cs` on the `Main Camera`).

- Most new custom scripts (exceptions are custom scripts that are direct extensions of used libraries/frameworks, like the `VMGFactory.cs` in the Factories folder) are categorized in the `Scripts` folder.

**Interaction:**
- InputHandler scripts handle direct user-input and communicate with relevant managers.
- Interaction (`…Interaction.cs`) scripts perform the actions related to the input.
- Manager (`…Manager.cs`) scripts handle any indirect functions caused by user input, like the adjustment of the UI.

- Inventory items are saved under `Prefabs/Inventory`. When adding new items, make sure the components are identical to existing inventory items.
- Sprite rendering is based on the ‘Order in Layer’ and range between 0 and 5. (Higher number = rendered on top of others).
- If you want to add text in the scene, use the `3DTextPrefab` object in the project.


## Authors
* **Erik Vullings** (initial work) - *Initial work including generation of tiles, mqtt connection, integration of MapZenGo, buildings and objects, basic navigation.*
* **Thom van de Moosdijk** (graduation project) - *Addition of interaction system, user interface and other enhancements.*
