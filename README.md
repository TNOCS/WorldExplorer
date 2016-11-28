# WorldExplorer
A Hololens application, based on [MapzenGo](https://github.com/brnkhy/MapzenGo), to render 3D worlds as hologram.

![Example](http://i.imgur.com/MEwIH56.png)

# Build instructions

First, make sure that you can run the local tile server. See the build instructions in the `world-explorer-server` folder.

- Open project in Unity (for Hololens, of course). Currently, I'm using a Beta version of Unity, v5.5b11.
- Import the MainScene from `Assets\Scenes`. Please check that the Main camera has a black solid color background.
- Press CTRL-B (Build project), and: 
  - adjust the build settings (Windows Store project, SDK: Universal 10, Target device: Hololens, UWP: D3D, C# project).
  - open the menu `Player Settings` and check that in `Other Settings`, the Virtual Reality SDK is set to Hololens  
  - Run `build`: create a new Folder (I called it App - please do it too, as this folder should be ignored in Git), and let it run. 
- Open the newly created Visual Studio solution in the App folder, set the build to Release | x86 | Hololens Emulator, and press Ctrl-F5

From Unity, open `config.json` and specify new views. In the Hololens app, you can switch views using the voice command `Switch to [VIEW NAME]`. 

# Creating asset bundles
In order to show 3D models, you need to create an asset bundle that contains these models. Currently, I do the following:
- Go to [3dwarehouse.sketchup.com](3dwarehouse.sketchup.com) and download your models
- Create a new Unity project
- Optionally, if you cannot open them in Unity, or the model has a terrain: Open them in Sketchup, e.g. to remove the underground (unlock the model), and save them as Sketchup 2015 project
- Import the model into Unity (menu Assets|Import New Asset...): in the inspecter, underneath the image, specify a name for the asset bundle (e.g. buildings/eindhoven). If you don't specify the name, it won't be exported. Also note the geo location in the inspector, which should be used in the `assets.json` file in the `world-explorer-server` project folder. 
- Optionally, change the model name, as this is how you load it from Unity: e.g. I only use lowercase names.
- Import all models until you are done. 
- In the `Assets` folder, create a new folder named `AssetBundles' (the name used in the script below).
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

# Known issues

## Build issue
Probably an issue with the beta version of Unity, but when building the project in Visual Studio 2015, I received the following error:
```
Severity	Code	Description	Project	File	Line	Suppression State
Error		Could not copy the file "C:\Users\USERNAME\WorldExplorer\App\WorldExplorer\Plugins\x86\MicStreamSelector.dll" because it was not found.	WorldExplorer	C:\Users\USERNAME\WorldExplorer\App\WorldExplorer\WorldExplorer.csproj	
```
Fix it by manually copying `YOUR_FOLDER\App\WorldExplorer\MicStreamSelector.dll` to `YOUR_FOLDER\App\WorldExplorer\Plugins\x86\` (you need to create the folder too).

## Deploying to the emulator issue
When deploying to the emulator does not work (emulator freezes with the 'OS loading...' message in view), you may try this fix which works for me. 
Open Hyper-V manager (press WINDOWS+S en enter Hyper-V), open Virtual Switch Manager (right menu) and remove the Windows Phone and Microsoft Emulator NAT Switch by selecting them, clicking `remove` and finally, clicking `OK`. Now, deploy the app again from VS and note that you will be asked by the emulator to run in elevated mode so it can retry the settings that you've just removed. 

You should also have Internet connection, as we are downloading tiles from the Internet. As the main view does not properly show the Internet connection (it looks like I'm offline), I normally open the feedback hub. If it asks me to sign in, I know I'm connected. If not, make sure that the `Microsoft Emulator NAT Switch` in the Hyper-V Virtual Switch manager is bridged with your Internet provider, and that the Hololens image also uses it (select it, open the Settings, and check the Network settings). 