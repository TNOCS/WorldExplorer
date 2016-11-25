import * as fs from 'fs';
import * as path from 'path';
import * as mkdirp from 'mkdirp';
import * as request from 'request';
import { AssetTileService } from '../services/asset-tile-service';
import { ITile, ITileService, FeatureCollection } from '../models/tile-service';

const assets: FeatureCollection = require(path.join(process.cwd(), 'assets.json'));
/**
 * The TileServer retreives data either from an external datasource like Mapzen, mbtiles,
 * or the cached version from the file system.
 * 
 * The file layout
 * - zoom
 *   - x
 *     - y.buildings.json
 *       y.roads.json
 *       y.water.json
 * @export
 * @class TileServer
 * @implements {ITileService}
 */
export class TileServer implements ITileService {
  private assetService: AssetTileService;
  /**
   * If false, do not use the Internet.
   * 
   * @private
   * @type {boolean}
   * @memberOf TileServer
   */
  private useInternet: boolean;

  /**
   * Creates an instance of TileServer.
   * 
   * @param {string} url: URL for requesting more data
   * @param {string} path: absolute base path to the cached data 
   * 
   * @memberOf TileServer
   */
  constructor(port: number, private url: string, private path: string) {
    this.useInternet = !url;
    if (!fs.existsSync(path)) { fs.mkdirSync(path); }
    if (!assets.hasOwnProperty('features') || assets.features.length === 0) { return; }
    this.assetService = new AssetTileService(port, 'assets', assets);
  }

  public getTile(tile: ITile, cb: (error: Error, collection: { [key: string]: FeatureCollection }) => void) {
    this.loadTile(tile, (error, collection) => {
      this.assetService.loadTile(tile, collection, cb);
    });
  }

  /**
   * Load the tile from the cache.
   * 
   * @private
   * @param {ITile} tile
   * @param {{ error: Error, tile: { [key: string]: FeatureCollection } }} cb
   * 
   * @memberOf TileServer
   */
  private loadTile(tile: ITile, cb: (error: Error, collection: { [key: string]: FeatureCollection }) => void) {
    let folder = path.join(this.path, tile.zoom, tile.x, tile.y);
    fs.exists(folder, exists => {
      if (!exists) {
        return mkdirp(folder, () => {
          this.downloadTile(tile, folder, cb);
        });
      }
      let collection: { [key: string]: FeatureCollection } = {};
      fs.readdir(folder, (err, files) => {
        if (err) { return this.downloadTile(tile, folder, cb); }
        let count = tile.layers.length;
        let ok = 0;
        let failed = 0;
        let transmit = () => {
          if (ok + failed === count) {
            cb(null, collection);
          }
        };
        tile.layers.forEach(layer => {
          let filename = `${layer}.json`;
          if (files.indexOf(filename) < 0) {
            failed++;
            if (layer !== 'assets') {
              collection[layer] = { type: 'FeatureCollection', features: [] };
              console.error(`Couldn't retreive layer ${path.join(folder, filename)}.`);
            }
            transmit();
          } else {
            fs.readFile(path.join(folder, filename), 'utf8', (error, data) => {
              if (error) {
                failed++;
                collection[layer] = { type: 'FeatureCollection', features: [] };
                console.error(`Couldn't read layer ${path.join(folder, filename)}.`);
                transmit();
              } else {
                ok++;
                collection[layer] = <FeatureCollection> JSON.parse(data);
                transmit();
              }
            });
          }
        });
      });
    });
  }

  /**
   * Download the tile from the Internet.
   * 
   * @private
   * @param {ITile} tile
   * @param {string} folder Path to cache folder
   * @param {{ error: Error, tile: { [key: string]: FeatureCollection } }} cb
   * 
   * @memberOf TileServer
   */
  private downloadTile(tile: ITile, folder: string, cb: (error: Error, collection: { [key: string]: FeatureCollection }) => void) {
    let collection: { [key: string]: FeatureCollection } = {};
    let url = this.url
      .replace(/{z}/, tile.zoom)
      .replace(/{x}/, tile.x)
      .replace(/{y}/, tile.y);
    request({
      url: url,
      json: true
    }, (error, response, layers: { [key: string]: FeatureCollection }) => {
      if (error || response.statusCode !== 200) {
        return cb(new Error(`Couldn't retreive the data from ${url}.\nError message: ${JSON.stringify(error)}.`), null);
      }
      for (let key in layers) {
        if (!layers.hasOwnProperty(key)) { continue; }
        let layer = layers[key];
        if (tile.layers.indexOf(key) >= 0) { collection[key] = layer; }
        fs.writeFile(path.join(folder, key + '.json'), JSON.stringify(layer), err => {
          if (err) { console.error(`Error saving layer ${key}: ${err.message}`); }
        });
      }
      cb(null, collection);
    });
  }
}
