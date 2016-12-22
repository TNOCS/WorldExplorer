import { GeoJSONUtils } from '../utils/geojson-utils';

export type IPoint = { x: number, y: number, z?: number };

/**
 * Helper class with useful geo routines. Adapted from http://stackoverflow.com/questions/12896139/geographic-coordinates-converter
 */
export class GeoConverter {
  public static resolution(zoom: number) {
    return GeoConverter.InitialResolution / (Math.pow(2, zoom));
  }

  /**
   * Converts given lat/lon in WGS84 Datum to XY in Spherical Mercator EPSG:900913
   *
   * @static
   * @param {number} lat
   * @param {number} lon
   * @returns
   *
   * @memberOf GeoConverter
   */
  public static LatLonToMeters(lat: number, lon: number) {
    const go = GeoConverter.OriginShift / 180;
    let p = <IPoint> {};
    p.x = (lon * go);
    p.y = (Math.log(Math.tan((90 + lat) * Math.PI / 360)) / (Math.PI / 180));
    p.y = (p.y * go);
    return p;
  }

  /**
   * Converts EPSG:900913 to pyramid pixel coordinates in given zoom level
   *
   * @static
   * @param {IPoint} p
   * @param {number} zoom
   * @returns
   *
   * @memberOf GeoConverter
   */
  public static metersToPixels(m: IPoint, zoom: number) {
    let res = GeoConverter.resolution(zoom);
    let pix = <IPoint> {};
    pix.x = ((m.x + GeoConverter.OriginShift) / res);
    pix.y = ((-m.y + GeoConverter.OriginShift) / res);
    return pix;
  }

  /**
   *  Converts pixel coordinates in given zoom level of pyramid to EPSG:900913
   */
  public static pixelsToMeters(p: IPoint, zoom: number) {
    let res = GeoConverter.resolution(zoom);
    let met = <IPoint> {};
    met.x = (p.x * res - GeoConverter.OriginShift);
    met.y = -(p.y * res - GeoConverter.OriginShift);
    return met;
  }

  /**
   * Returns a TMS (NOT Google!) tile covering region in given pixel coordinates
   */
  public static pixelsToTile(p: IPoint) {
    let t = <IPoint> {};
    t.x = Math.ceil(p.x / GeoConverter.TileSize) - 1;
    t.y = Math.ceil(p.y / GeoConverter.TileSize) - 1;
    return t;
  }

  /**
   * Returns tile for given Lat/lon coordinates
   *
   * @static
   * @param {IPoint} m
   * @param {number} zoom
   * @returns
   *
   * @memberOf GeoConverter
   */
  public static latlonToTile(lat: number, lon: number, zoom: number) {
    let m = GeoConverter.LatLonToMeters(lat, lon);
    let p = GeoConverter.metersToPixels(m, zoom);
    let tile = GeoConverter.pixelsToTile(p);
    tile.z = zoom;
    return tile;
  }

  /**
   * Returns tile for given mercator coordinates
   *
   * @static
   * @param {IPoint} m
   * @param {number} zoom
   * @returns
   *
   * @memberOf GeoConverter
   */
  public static metersToTile(m: IPoint, zoom: number) {
    let p = GeoConverter.metersToPixels(m, zoom);
    return GeoConverter.pixelsToTile(p);
  }

  public static getCenter(feature: GeoJSON.Feature<GeoJSON.GeometryObject>): GeoJSON.GeometryObject {
    if (feature.geometry.type === 'Point') { return feature.geometry; }
    return GeoJSONUtils.centroid(feature.geometry.coordinates);
  }

  private static TileSize = 256;
  private static EarthRadius = 6378137;
  private static InitialResolution = 2 * Math.PI * GeoConverter.EarthRadius / GeoConverter.TileSize;
  private static OriginShift = 2 * Math.PI * GeoConverter.EarthRadius / 2;
}
