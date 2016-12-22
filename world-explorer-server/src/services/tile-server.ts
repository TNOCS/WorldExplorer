import * as fs from 'fs';
import * as path from 'path';
import * as mkdirp from 'mkdirp';
import * as request from 'request';
import { AssetTileService } from '../services/asset-tile-service';
import { ITile, ITileService, FeatureCollection } from '../models/tile-service';

const assets: FeatureCollection = require(path.join(process.cwd(), 'assets.json'));

export type FeatureCollectionCollection = { [key: string]: FeatureCollection };
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
   * @param {number} port
   * @param {string} url
   * @param {string} path: absolute base path to the cached data
   * @param {string} [server] IP address, e.g. http://...
   *
   * @memberOf TileServer
   */
  constructor(port: number, private osmUrl: string, private dutchBuildingUrl: string, private path: string, private server?: string) {
    if (!osmUrl) { console.error('URL is not defined!'); }
    this.useInternet = !osmUrl;
    if (!fs.existsSync(path)) { fs.mkdirSync(path); }
    if (!assets.hasOwnProperty('features') || assets.features.length === 0) { return; }
    this.assetService = new AssetTileService(this, port, 'assets', server, assets);
  }

  public getTile(tile: ITile, cb: (error: Error, collection: FeatureCollectionCollection) => void) {
    // console.log(`Getting tile: ${tile.zoom}/${tile.x}/${tile.y}`);
    let emptyCollection: FeatureCollectionCollection = {};
    this.loadOsmTile(tile, emptyCollection, (error1, collection1) => {
      if (error1) { return cb(error1, null); }
      this.loadDutchBuildingsTile(tile, collection1, (error2, collection2) => {
        if (error2) { return cb(error2, null); }
        this.assetService.loadTile(tile, collection2, cb);
      });
    });
  }


  private loadDutchBuildingsTile(tile: ITile, collection: FeatureCollectionCollection, cb: (error: Error, collection: FeatureCollectionCollection) => void) {
    const key = 'gebouwen';
    if (tile.layers.indexOf(key) < 0) { return cb(null, collection); }
    collection[key] = <FeatureCollection> {};
    let folder = path.join(this.path, tile.zoom, tile.x, tile.y);
    let filename = path.join(folder, `${key}.json`);
    fs.exists(filename, exists => {
      if (!exists) {
        return this.downloadTile(tile, this.dutchBuildingUrl, collection, folder, cb);
      }
      fs.readFile(filename, 'utf8', (error, data) => {
        if (error || !data) {
          console.error(`Couldn't read layer ${filename}.`);
        } else {
          collection[key] = <FeatureCollection> JSON.parse(data);
        }
        cb(error, collection);
      });
    });
  }

  /**
   * Load the tile from the cache.
   *
   * @private
   * @param {ITile} tile
   * @param {{ error: Error, tile: FeatureCollectionCollection }} cb
   *
   * @memberOf TileServer
   */
  private loadOsmTile(tile: ITile, collection: FeatureCollectionCollection, cb: (error: Error, collection: FeatureCollectionCollection) => void) {
    const osmLayers = ['water', 'earth', 'landuse', 'roads', 'pois', 'transit', 'buildings', 'places', 'boundaries'];
    let folder = path.join(this.path, tile.zoom, tile.x, tile.y);
    fs.exists(folder, exists => {
      if (!exists) {
        return mkdirp(folder, () => {
          this.downloadTile(tile, this.osmUrl, collection, folder, cb);
        });
      }
      fs.readdir(folder, (err, files) => {
        if (err) { return this.downloadTile(tile, this.osmUrl, collection, folder, cb); }
        let count = tile.layers.length;
        let ok = 0;
        let failed = 0;
        let transmit = () => {
          if (ok + failed === count) {
            cb(null, collection);
          }
        };
        tile.layers.forEach(layer => {
          if (osmLayers.indexOf(layer) < 0) {
            failed++;
            return transmit();
          }
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
              if (error || !data) {
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

  private tileToSlippyMapUrl(url: string, tile: ITile) {
    return url
      .replace(/{z}/, tile.zoom)
      .replace(/{x}/, tile.x)
      .replace(/{y}/, tile.y);
  }

  /**
   * Download the tile from the Internet.
   *
   * @private
   * @param {ITile} tile
   * @param {string} folder Path to cache folder
   * @param {{ error: Error, tile: FeatureCollectionCollection }} cb
   *
   * @memberOf TileServer
   */
  private downloadTile(tile: ITile, baseUrl: string, collection: FeatureCollectionCollection, folder: string, cb: (error: Error, collection: FeatureCollectionCollection) => void) {
    let url = this.tileToSlippyMapUrl(baseUrl, tile);
    request({
      url: url,
      json: true
    }, (error, response, layers: FeatureCollectionCollection) => {
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
