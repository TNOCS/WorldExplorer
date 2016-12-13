import * as ip from 'ip';
import { GeoConverter } from '../utils/geo-converter';
import { ITile, FeatureCollection } from '../models/tile-service';

const urljoin = require('url-join');

/**
 * Split a GeoJSON file into tiles.
 */
export class AssetTileService {
  private isActive = false;
  /** A list of IDs that should be removed: the key is the collection name, the number[] is an array with IDs */
  private idsToRemove = <{ [key: string]: number[] }> {};
  private tiles: { [zoom: number]: { [key: string]: FeatureCollection } } = {};

  constructor(private port: number, private name: string, private server?: string, private assets?: FeatureCollection) {
    if (!assets || !assets.hasOwnProperty('features') || assets.features.length === 0) { return; }
    this.setAssetBundleUrl(server);
    this.getIdsToRemove();
    // Create tiles for the most popular zoom levels
    [15, 16, 17, 18].forEach(zoom => this.createTiles(zoom));
    this.isActive = true;
  }

  public loadTile(tile: ITile, collection: { [key: string]: FeatureCollection }, cb: (error: Error, collection: { [key: string]: FeatureCollection }) => void) {
    if (!this.isActive) { return cb(null, collection); }

    this.addAssetsToCollection(tile, collection);
    this.removeIdsFromCollection(collection);

    cb(null, collection);
  }

  private addAssetsToCollection(tile: ITile, collection: { [key: string]: FeatureCollection }) {
    if (!this.tiles.hasOwnProperty(tile.zoom)) { this.createTiles(+tile.zoom); }
    // add file to collection
    let key = this.createKey(tile);
    let zc = this.tiles[tile.zoom];
    if (zc.hasOwnProperty(key)) {
      collection[this.name] = zc[key];
    }
  }

  private removeIdsFromCollection(collection: { [key: string]: FeatureCollection }) {
    for (let k in collection) {
      if (!collection.hasOwnProperty(k) || !this.idsToRemove.hasOwnProperty(k)) { continue; }
      let ids = this.idsToRemove[k];
      let fc = collection[k];
      fc.features = fc.features.filter(f => {
        return f.hasOwnProperty('properties') && f.properties.hasOwnProperty('id') && ids.indexOf(f.properties.id) < 0;
      });
    }
  }

  private createKey(tile: { x: string | number, y: string | number }) { return `${tile.x}-${tile.y}`; }

  private addAssets(tile: ITile, collection: { [key: string]: FeatureCollection }) {
  }

  /**
   * Obtain all the IDs that must be removed, and store them in idsToRemove;
   * 
   * @private
   * 
   * @memberOf AssetTileService
   */
  private getIdsToRemove() {
    this.assets.features.forEach(f => {
      if (!(f.hasOwnProperty('properties') && f.properties.hasOwnProperty('remove'))) { return; }
      let r: { [key: string]: number[] } = f.properties.remove;
      for (let key in r) {
        if (!r.hasOwnProperty(key)) { continue; }
        if (!this.idsToRemove.hasOwnProperty(key)) { 
          this.idsToRemove[key] = r[key];
        } else {
          let c = this.idsToRemove[key];
          r[key].forEach(id => {
            if (c.indexOf(id) >= 0) { return; }
            c.push(id);
          });
        }
      }
    });
  }

  /**
   * As an asset bundle is stored relative to the server, add the local IP address to the assetbundle property (unless it has an absolute address).
   * 
   * @private
   * 
   * @memberOf AssetTileService
   */
  private setAssetBundleUrl(server?: string) {
    let ipAddress: string;
    if (server) {
      ipAddress = `http://${server}:${this.port}`;
    } else {
      ipAddress = `http://${ip.address()}:${this.port}`;
    }
    this.assets.features.forEach(f => {
      if (!f.hasOwnProperty('properties')
        || !f.properties.hasOwnProperty('assetbundle')
        || (<string> f.properties.assetbundle).indexOf('http') >= 0) { return; }
      f.properties.assetbundle = urljoin(ipAddress, f.properties.assetbundle);
    });
  }

  /**
   * Create all tiles for a certain zoom level.
   * 
   * @private
   * @param {number} zoom
   * 
   * @memberOf SplitGeoJSON
   */
  private createTiles(zoom: number) {
    let collection: { [key: string]: FeatureCollection } = {};
    this.tiles[zoom] = collection;
    this.assets.features.forEach(f => {
      if (!f.hasOwnProperty('properties')) { f.properties = {}; }
      if ((f.properties.hasOwnProperty('min_zoom') && zoom < f.properties.min_zoom)
        || (f.properties.hasOwnProperty('max_zoom') && zoom > f.properties.max_zoom)) { return; }

      let centroid: GeoJSON.GeometryObject;
      if (f.properties.hasOwnProperty('centroid')) {
        centroid = f.properties.centroid;
      } else {
        centroid = GeoConverter.getCenter(f);
        f.properties.centroid = centroid;
      }
      let tile = GeoConverter.latlonToTile(centroid.coordinates[1], centroid.coordinates[0], zoom);
      let key = this.createKey(tile);
      if (!collection.hasOwnProperty(key)) { collection[key] = <FeatureCollection> { type: 'FeatureCollection', features: [] }; }
      collection[key].features.push(f);
    });
  }
}
