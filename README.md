# WorldExplorer
A Hololens application, based on [MapzenGo](https://github.com/brnkhy/MapzenGo), to render 3D worlds as hologram.

![Example](http://i.imgur.com/MEwIH56.png)

# Build instructions

- Open project in Unity (for Hololens, of course). 
- Press CTRL-B (Build project), create a new Folder (I called it App - please do it too, as this folder should be ignored in Git), and let it run. 
- Open the newly created Visual Studio solution in the App folder, set the build to Release | x86 | Hololens Emulator, and press Ctrl-F5

As in the original project, you can create more time (increasing the loading time), by changing some settings in Unity. 

World|Tiles|CachedTileManager:
- Specify the start coordinate
- Specify the zoom level (higher numbers means less buildings)
- Specify the range of tiles, e.g. range 2 means that we load 2 tiles on each side of our normal one.

World|Tiles|...Factory
- Specify the mapping between OSM building|road|etc type, the material to use, and the randomly generated building height (only used if it is missing in OSM).
