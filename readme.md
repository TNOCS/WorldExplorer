# WorldExplorer
A HoloLens application that allows users to view and interact with automatically generated area's. The world is rendered using OpenStreepMapData, and shows the users the world on a squared table. Sessions can be shared with other users, allowing for collaboration anywhere in the world. 
Is it based on [MapzenGo](https://github.com/brnkhy/MapzenGo), to render 3D worlds as hologram. 
[Video](https://vimeo.com/247794077)
![Example](https://imgur.com/a/zaLWt)

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

### Prerequisites
Required software:
- Unity v5.6 or higher. (Currently recommended: latest Unity 2017).
- Visual Studio 2017.
- Windows 10 Fall Creators Update.

Required hardware:
- Microsoft HoloLens.

## Getting Started - Development
1. Open Unity (if there is a version mismatch waring, make sure there is a backup and automatically upgrade the project).
2. Open the MainScene.scene.

The Manager gameobject contains many script that allow for easy editting, debugging and adding voice commands.

The generation of buildings and objects are done through Factories.
Interaction is called through the relevant InputHandlers.
Interaction and user interface functionalities are performed through the relevant scripts in the Scripts/Interaction folder.

## Getting Started - Deploying to the HoloLens
1. Press CTRL-B (Build Project).
2. Add the current scene.
3. Make sure the following settings are selected:
	- Universal Windows Platform.
	- Target device: HoloLens.
	- Build Type: D3D.
	- SDK: Latest installed.
	- Build and run on: Local machine.
	- Unity C# projects checked (for debugging purposes only).
4. Make sure "Virtual Reality Supported" is checked under Player Settings.
5. Press Build and select the App folder, and choose a name.
6. Open the Visual Studio Solution in the created folder.
7. Build as "Release", "x86" and either to "Device" (when deploying over USB) or "Remote Machine" (when deploying over Wi-Fi). 

NOTE: In Initialize.cs, the config.json is specified. Replace this with a version of your own if you want to edit anything related to the configuration.

## Creating asset bundles
In order to show 3D models, you need to create an asset bundle that contains these models. Currently, I do the following:
- Go to [3dwarehouse.sketchup.com](3dwarehouse.sketchup.com) and download your models
- Create a new Unity project
- Optionally, if you cannot open them in Unity, or the model has a terrain: Open them in Sketchup, e.g. to remove the underground (unlock the model), and save them as Sketchup 2015 project
- Import the model into Unity (menu Assets|Import New Asset...): in the inspecter, underneath the image, specify a name for the asset bundle (e.g. buildings/eindhoven). If you don't specify the name, it won't be exported. Also note the geo location in the inspector, which should be used in the `assets.json` file in the `world-explorer-server` project folder.
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

- Layers are the layers used for the tile images.
- Views are the bookmark locations: 
	- 'Lat' and 'Lon' can be changed to adjust the location. 
	- Zoom levels can changed to adjust initial zoom level.
	- tileSize and range can be changed to change the size of the tiles.
	- mapzen, layers and tileLayers can be adjusted to enable or disable certain visualizations.
	- terrainHeightAvailable should be enabled when terrain height data is available.

## Adding inventory objects
To add inventory objects.

1. Import new object (.fbx preferred) into the Resources/Prefabs/Inventory map folder.
2. Copy all components of any other inventory object to new object.
3. Check if the scale is realistic.
4. Add the object to the inventory in hierarchy as a new item (break prefab link). Make sure the name matches.

## Authors
* **Erik Vullings** (initial work) - *Including generation of tiles, mqtt connection, integration of MapZenGo, buildings and objects, basic navigation.*
* **Thom van de Moosdijk** (graduation project) - *Addition of interaction system, user interfaction and other enhancements.*
