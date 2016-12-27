# WorldExplorer
A Hololens application to increase your situational awareness. Using OpenStreetMap data, it renders the world. You can share a session with multiple users, editing layers of information. Even though we can share our world, different users can display different layers of information.  
Is it based on [MapzenGo](https://github.com/brnkhy/MapzenGo), to render 3D worlds as hologram. [Preview](https://vimeo.com/187078876).

![Example](https://cloud.githubusercontent.com/assets/3140667/21022014/5d8de41e-bd7b-11e6-99f5-6e265df12726.png)

# Features
- Render the world based on open data
- Collaborative: users can join a session and share their world. Also, you can edit layers together.
- Navigate the world using your browser: you can update your location using a web browser. See the [world-explorer-server](https://github.com/TNOCS/WorldExplorer/tree/master/world-explorer-server) for more information on this.
- Show 3D models of the world
- Voice commands
  - switch to [NAMED_VIEW]: in the config.json, the views are defined
  - toggle buildings | roads | water | [LAYER_NAME]: toggle the visibility of the buildings. etc.
  - zoom in | out
  - increase | decrease range: increase or decrease the displayed area
  - move north | south | east | west | north east | etc. : move one tile in that direction
  - show | hide commands: displays a HUD with some of these commands
  - place: when an item is selected, places the item
  - clear cache: clears the local cache

# Lessons Learned / known issues
Also see the [Lessons Learned](https://github.com/TNOCS/WorldExplorer/wiki/Lessons-Learned) section in the Wiki, which contains some tips to develop Hololens applications for Unity.

# Build instructions

First, make sure that you can run the local tile server. See the build instructions in the [world-explorer-server](https://github.com/TNOCS/WorldExplorer/tree/master/world-explorer-server) folder.

- Open project in Unity (for Hololens, of course). Currently, I'm using a Beta version of Unity, v5.5f03.
- Import the MainScene from `Assets\Scenes`. Please check that the Main camera has a black solid color background.
- Press CTRL-B (Build project), and: 
  - adjust the build settings (Windows Store project, SDK: Universal 10, Target device: Hololens, UWP: D3D, C# project).
  - open the menu `Player Settings` and check that in `Other Settings`, the Virtual Reality SDK is set to Hololens. 
  - Run `build`: create a new Folder (I called it App - please do it too, as this folder should be ignored in Git), and let it run. 
- Open the newly created Visual Studio solution in the App folder, set the build to Release | x86 | Hololens Emulator (or device), and press Ctrl-F5

NOTE: In Initialize.cs, we specify the config.json that is loaded. Please replace this with a version of your own, as the IP addresses of tile server and MQTT server will certainly not match yours! 

# Creating asset bundles
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
