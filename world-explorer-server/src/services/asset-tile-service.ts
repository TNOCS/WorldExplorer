import * as fs from 'fs';
import * as path from 'path';
import * as ip from 'ip';
import { GeoConverter } from '../utils/geo-converter';
import { GeoJSONUtils } from '../utils/geojson-utils';
import { TileServer } from './tile-server';
import { ITile, FeatureCollection } from '../models/tile-service';

const urljoin = require('url-join');
const log = console.log;

/**
 * Split a GeoJSON file into tiles.
 */
export class AssetTileService {
  private isActive = false;
  /** A list of IDs that should be removed: the key is the collection name, the number[] is an array with IDs */
  private idsToRemove = <{ [key: string]: number[] }>{};
  private tiles: { [zoom: number]: { [key: string]: FeatureCollection } } = {};

  constructor(private tileServer: TileServer, private port: number, private name: string, private server?: string, private assets?: FeatureCollection) {
    if (!assets || !assets.hasOwnProperty('features') || assets.features.length === 0) { return; }
    this.setIdsToRemove(() => {
      this.getIdsToRemove();
      this.setAssetBundleUrl(server);
    });
    // Create tiles for the most popular zoom levels
    [15, 16, 17, 18].forEach(zoom => this.createTiles(zoom));
    this.isActive = true;
  }

  public loadTile(tile: ITile, collection: { [key: string]: FeatureCollection }, cb: (error: Error, collection: { [key: string]: FeatureCollection }) => void) {
    if (!this.isActive || tile.layers.indexOf(this.name) < 0) { return cb(null, collection); }

    this.addAssetsToCollection(tile, collection);
    this.removeIdsFromCollection(collection);

    cb(null, collection);
  }

  /**
   * Add assets to collection
   *
   * @private
   * @param {ITile} tile
   * @param {{ [key: string]: FeatureCollection }} collection
   *
   * @memberOf AssetTileService
   */
  private addAssetsToCollection(tile: ITile, collection: { [key: string]: FeatureCollection }) {
    if (!this.tiles.hasOwnProperty(<string>tile.zoom)) { this.createTiles(+tile.zoom); }
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

  // private addAssets(tile: ITile, collection: { [key: string]: FeatureCollection }) {
  // }

  /**
   * Based on the location (geo-point) of an asset, determine which IDs to remove from buildings or gebouwen.
   * Only process assets whose 'remove' property hasn't been set. In addition, as sometimes an asset's location is outside the
   * boundary of a building, try to enlarge the scope.
   *
   * @private
   * @memberOf AssetTileService
   */
  private setIdsToRemove(cb: () => void) {
    const layers = ['buildings', 'gebouwen', 'pois'];
    const zoom = 18;
    let counter = this.assets.features.length;
    let save = () => {
      counter--;
      if (counter === 0) {
        this.saveAssetsFile();
        cb();
      }
    };
    this.assets.features.forEach(f => {
      if (!f.hasOwnProperty('properties') || f.properties.hasOwnProperty('remove')) { return save(); }
      let point = <GeoJSON.Point> f.geometry;
      // determine tile
      let t = GeoConverter.latlonToTile(point.coordinates[1], point.coordinates[0], zoom);
      let tile: ITile = {
        x: `${t.x}`,
        y: `${t.y}`,
        zoom: `${t.z}`,
        layers: layers
      };
      this.tileServer.getTile(tile, (err, collection) => {
        if (err) {
          console.error(err.message);
          save();
        }
        for (let key in collection) {
          if (!collection.hasOwnProperty(key)) { continue; }
          let geojson = collection[key];
          this.findAssetInFeatureCollection(geojson, f, point, key);
        }
        save();
      });
    });
  }

  /**
   * Find the item that occupies the asset's location.
   * Since sometimes we do not find anything at the location, e.g. due to the shape of the building, we move the point at the most in 4 directions
   * (four corners of a cube, with the original point at its centre) to see if we find a building there.
   *
   * @private
   * @param {FeatureCollection} geojson
   * @param {GeoJSON.Feature<GeoJSON.GeometryObject>} feature
   * @param {GeoJSON.Point} point
   * @param {string} key
   * @param {number} [tries=1]
   * @returns
   *
   * @memberOf AssetTileService
   */
  private findAssetInFeatureCollection(geojson: FeatureCollection, feature: GeoJSON.Feature<GeoJSON.GeometryObject>, point: GeoJSON.Point, key: string, tries = 1) {
    const MaxDistance = 5;
    const DeltaX = 0.00005;
    const DeltaY = 0.00005;
    let isFound = false;
    geojson.features.forEach(b => {
      if (!b.properties.hasOwnProperty('id')
        || (b.properties.hasOwnProperty('kind') && b.properties.kind !== 'building')
        || (b.geometry.type === 'Point' && !GeoJSONUtils.geometryWithinRadius(point, <GeoJSON.Point> b.geometry, MaxDistance))
        || (b.geometry.type === 'Polygon' && !GeoJSONUtils.pointInPolygon(point, <GeoJSON.Polygon> b.geometry))
        || (b.geometry.type === 'MultiPolygon' && !GeoJSONUtils.pointInMultiPolygon(point, <GeoJSON.MultiPolygon> b.geometry))) { return; }
      isFound = true;
      let id = b.properties.id;
      if (!feature.properties.hasOwnProperty('remove')) { feature.properties.remove = {}; }
      if (!feature.properties.remove.hasOwnProperty(key)) { feature.properties.remove[key] = []; }
      if (feature.properties.remove[key].indexOf(id) >= 0) { return; }
      feature.properties.remove[key].push(id);
    });
    if (isFound || tries >= 4 || key === 'pois') { return; }
    switch (tries) {
      case 1:
        point = <GeoJSON.Point> { coordinates: [ point.coordinates[0] + DeltaX, point.coordinates[1] - DeltaY ] };
        break;
      case 2:
        point = <GeoJSON.Point> { coordinates: [ point.coordinates[0], point.coordinates[1] + 2 * DeltaY ] };
        break;
      case 3:
        point = <GeoJSON.Point> { coordinates: [ point.coordinates[0] - 2 * DeltaX, point.coordinates[1] ] };
        break;
      case 4:
        point = <GeoJSON.Point> { coordinates: [ point.coordinates[0], point.coordinates[1] - 2 * DeltaY ] };
        break;
      default: throw new Error('findAssetInFeatureCollection shouldn\'t be tried more than four times');
    }
    tries++;
    this.findAssetInFeatureCollection(geojson, feature, point, key, tries);
  }

  private saveAssetsFile() {
    fs.writeFile(path.join(process.cwd(), this.name + '.json'), JSON.stringify(this.assets, null, 2), err => {
      if (err) { return console.error(err.message); }
      log(`Assets file updated.`);
    });
  }
  /**
   * Obtain all the IDs that must be removed, and store them in idsToRemove;
   *
   * @private
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
        || (<string>f.properties.assetbundle).indexOf('http') >= 0) { return; }
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
      if (!collection.hasOwnProperty(key)) { collection[key] = <FeatureCollection>{ type: 'FeatureCollection', features: [] }; }
      collection[key].features.push(f);
    });
  }
}
