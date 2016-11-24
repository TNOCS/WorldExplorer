import { GeoConverter } from '../utils/geo-converter';
import { GeoJSONUtils } from '../utils/geojson-utils';
import { ITile, FeatureCollection } from '../models/tile-service';

/**
 * Split a GeoJSON file into tiles.
 */
export class AssetTileService {
  private isActive = false;
  private tiles: { [zoom: number]: { [key: string]: FeatureCollection } } = {};

  constructor(private name: string, private geojson?: FeatureCollection) {
    if (!geojson || !geojson.hasOwnProperty('features') || geojson.features.length === 0) { return; }
    // Create tiles for the most popular zoom levels
    [15, 16, 17, 18].forEach(zoom => this.createTiles(zoom));
    this.isActive = true;
  }

  public loadTile(tile: ITile, collection: { [key: string]: FeatureCollection }, cb: (error: Error, collection: { [key: string]: FeatureCollection }) => void) {
    if (!this.isActive) { return cb(null, collection); }
    if (!this.tiles.hasOwnProperty(tile.zoom)) { this.createTiles(+tile.zoom); }
    // add file to collection
    let key = this.createKey(tile);
    let zc = this.tiles[tile.zoom];
    if (zc.hasOwnProperty(key)) {
      collection[this.name] = zc[key];
    }

    for (let i in zc) {
      if (!zc.hasOwnProperty(i)) { continue; }
      let fc = zc[i];
      fc.features.forEach(f => {
        if (!(f.properties.hasOwnProperty('remove_from') && f.properties.hasOwnProperty('id'))) { return; }
        let id: number = f.properties.id;
        let filter: string | string[] = f.properties.remove_from;
        if (typeof filter === 'string') {
          this.removeFeatureFromFeatureCollection(id, filter, collection);
        } else {
          filter.forEach(ff => this.removeFeatureFromFeatureCollection(id, ff, collection));
        }
      });
    }
    cb(null, collection);
  }

  private createKey(tile: { x: string | number, y: string | number }) { return `${tile.x}-${tile.y}`; }


  /**
   * Remove a feature by id from a feature collection.
   * 
   * @private
   * @param {GeoJSON.Feature<GeoJSON.GeometryObject>} f
   * @param {FeatureCollection} c
   * 
   * @memberOf SplitGeoJSON
   */
  private removeFeatureFromFeatureCollection(id: number, name: string, c: { [key: string]: FeatureCollection }) {
    if (!c.hasOwnProperty(name)) { return; }
    let fc = c[name];
    let i = fc.features.length;
    while (i--) {
      let f = fc.features[i];
      if (f.hasOwnProperty('properties') && f.properties.hasOwnProperty('id') && f.properties.id === id) {
        fc.features.splice(i, 1);
      }
    }
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
    this.geojson.features.forEach(f => {
      if (!f.hasOwnProperty('properties')) { f.properties = {}; }
      if ((f.properties.hasOwnProperty('min_zoom') && zoom < f.properties.min_zoom)
        || (f.properties.hasOwnProperty('max_zoom') && zoom > f.properties.max_zoom)) { return; }

      // Compute the bounds of the polygon 
      // let bounds: number[][];
      // if (f.properties.hasOwnProperty('bounds')) {
      //   bounds = f.properties.bounds;
      // } else {
      //   bounds = GeoJSONUtils.boundingBoxAroundPolyCoords(f.geometry.coordinates);
      //   f.properties.bounds = bounds;
      // }
      // let tiles: string[] = [];
      // [ bounds[0], bounds[1], [bounds[0][0], bounds[1][1]], [bounds[1][0], bounds[0][1]] ].forEach(p => {
      //   let tile = GeoConverter.latlonToTile(p[1], p[0], zoom);
      //   let key = this.createKey(tile);
      //   if (tiles.indexOf(key) < 0) { tiles.push(key); }
      // });
      // tiles.forEach(key => {
      //   if (!collection.hasOwnProperty(key)) { collection[key] = <FeatureCollection> { type: 'FeatureCollection', features: [] }; }
      //   collection[key].features.push(f);
      // });

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
