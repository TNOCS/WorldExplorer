import * as fs from 'fs';
import * as path from 'path';
import { ICommandLineOptions } from './cli';
const readline = require('readline');

export class Process {
  private fileCounter = 0;

  private geojson: GeoJSON.FeatureCollection<GeoJSON.Point> = {
    type: 'FeatureCollection',
    features: []
  };

  constructor(private options: ICommandLineOptions) {
    console.log(`Processing ${options.folder} to ${options.output}...`);

    this.processFolder(options.folder);
  }

  private processFolder(folder: string) {
    fs.readdir(folder, (err, files) => {
      if (err) {
        console.error(err.message);
        process.exit(1);
      }
      files.forEach(f => {
        if (path.extname(f) !== '.meta') return;
        this.fileCounter++;
        this.processMetaFile(path.join(folder, f));
      });
    });
  }

  private processMetaFile(file: string) {
    const lonRegex = /longitude:\s*([.\d]+)/i;
    const latRegex = /latitude:\s*([.\d]+)/i;
    const assetRegex = /assetBundleName:\s*([\w_.\/]+)/i;

    var lineReader = readline.createInterface({
      input: fs.createReadStream(file)
    });

    let feature: GeoJSON.Feature<GeoJSON.Point> = {
      type: 'Feature',
      geometry: {
        type: 'Point',
        coordinates: [0, 0]
      },
      properties: {
        "asset": path.basename(file, '.skp.meta'),
        "min_zoom": 16
      }
    }
    lineReader.on('line', (line: string) => {
      if (line) {
        let matches = line.match(lonRegex);
        if (matches) feature.geometry.coordinates[0] = +matches[1];

        matches = line.match(latRegex);
        if (matches) feature.geometry.coordinates[1] = +matches[1];

        matches = line.match(assetRegex);
        if (matches) feature.properties.assetbundle = matches[1];
      }
    });

    lineReader.on('close', () => {
      // Check if everything is present
      if (feature.geometry.coordinates[0] !== 0 && feature.geometry.coordinates[1] !== 0 && feature.properties.assetbundle) {
        this.geojson.features.push(feature);
      }
      this.fileCounter--;
      if (this.fileCounter <= 0) this.save();
    })
  }

  /** Save the GeoJSON file */
  private save() {
    fs.writeFile(this.options.output, JSON.stringify(this.geojson, null, 2), (err) => {
      if (err) {
        console.error(err.message);
        process.exit(1);
      }
      console.log(`Saved ${this.geojson.features.length} features to ${this.options.output}.`);
      process.exit(0);
    })
  }
}