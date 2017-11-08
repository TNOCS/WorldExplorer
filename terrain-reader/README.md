# Terrain-reader

A simple C# utility class to read the [Cesium terrain height v1.0 format](.https://github.com/AnalyticalGraphicsInc/cesium/wiki/heightmap-1.0-terrain-format).

## Prerequisites

Install [Visual Studio Code](https://code.visualstudio.com/download) with the C# and code runner extension: https://docs.microsoft.com/en-us/dotnet/core/tutorials/with-visual-studio-code

## Generating Cesium .terrain tiles from a WGS84 GeoTiff

Using Docker for Windows, install the [Cesium terrain builder docker image](https://hub.docker.com/r/homme/cesium-terrain-builder/). You probably need to create a Docker account for that, and optionally use `docker login` from the command line to enter your credentials.

Run the following commands to pull the latest CTB image from Docker (once), and run an interactive command prompt. The folder `C:\tmp` on the host is mapped to `/data` within the container.
```console
docker pull homme/cesium-terrain-builder:latest
docker run --mount type=bind,source="C:\tmp",target=/data -t -i homme/cesium-terrain-builder:latest bash
```

Inside the container, you can now convert the source.tif (GeoTIFF in WGS84 format) to Cesium terrain tiles using the following command:

```bash
mkdir /data/tiles && ctb-tile -s 21 -o /data/tiles /data/source.tif
```

Other options are documented [here](https://github.com/geo-data/cesium-terrain-builder). In this case, the only non-default option is to start at zoom level 21 and convert upwards. Now, drink a coffee or two and wait for the conversion to finish.

## Serving .terrain files

For development purposes, run a local `http-server` to host your files, e.g. install `node.js` from [here](https://nodejs.org/en/), and next, install a local `http-server` globally with the following command.

```console
npm i -g http-server
```

 In the local folder with the tiles (e.g. `C:\tmp\tiles`) run:

```console
http-server --cors ./
```

After which you can retreive the tiles from `http://YOUR_IP_ADDRESS:8080/{z}/{x}/{y}.terrain`. Optionally, you can change the port with the `-p` option, e.g. `-p 8888`. Note that you should use an actual IP address instead of `localhost` in case you are dealing with CORS issues.

## Using the application example

Open a command terminal (View: Toggle Integrated Terminal), and execute `dotnet run` in the project folder. Note that the first time, you may need to run `dotnet restore` to download any dependencies. It will convert the unzipped 25993.terrain file to a text file `converted.txt`, which contains the height at the 65x65 locations (East to West, North to South, starting at the North-West corner of the tile).

To use this code from Unity, you would need to:
- Fetch the `{zoom}/x/{y}.terrain` file from the server
- Unzip it
- Convert the binary data to heights according to the [Cesium terrain height v1.0 format](.https://github.com/AnalyticalGraphicsInc/cesium/wiki/heightmap-1.0-terrain-format).
- Apply the heights to the Unity mesh.

