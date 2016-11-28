/**
 * Adapted to TypeScript from https://raw.githubusercontent.com/maxogden/geojson-js-utils/master/geojson-utils.js
 */
export class GeoJSONUtils {
  // adapted from http://www.kevlindev.com/gui/math/intersection/Intersection.js
  public static lineStringsIntersect(l1, l2) {
    let intersects = [];
    for (let i = 0; i <= l1.coordinates.length - 2; ++i) {
      for (let j = 0; j <= l2.coordinates.length - 2; ++j) {
        let a1 = {
          x: l1.coordinates[i][1],
          y: l1.coordinates[i][0]
        };
        let a2 = {
          x: l1.coordinates[i + 1][1],
          y: l1.coordinates[i + 1][0]
        };
        let b1 = {
          x: l2.coordinates[j][1],
          y: l2.coordinates[j][0]
        };
        let b2 = {
          x: l2.coordinates[j + 1][1],
          y: l2.coordinates[j + 1][0]
        };
        let uaT = (b2.x - b1.x) * (a1.y - b1.y) - (b2.y - b1.y) * (a1.x - b1.x);
        let ubT = (a2.x - a1.x) * (a1.y - b1.y) - (a2.y - a1.y) * (a1.x - b1.x);
        let uB = (b2.y - b1.y) * (a2.x - a1.x) - (b2.x - b1.x) * (a2.y - a1.y);
        if (uB !== 0) {
          let ua = uaT / uB;
          let ub = ubT / uB;
          if (0 <= ua && ua <= 1 && 0 <= ub && ub <= 1) {
            intersects.push({
              'type': 'Point',
              'coordinates': [a1.x + ua * (a2.x - a1.x), a1.y + ua * (a2.y - a1.y)]
            });
          }
        }
      }
    }
    return intersects;
  }

  /**
   * Compute the bounding box
   * 
   * @static
   * @param {Array<Array<number[]>>} coords
   * @returns
   * 
   * @memberOf GeoJSONUtils
   */
  public static boundingBoxAroundPolyCoords(coords: Array<Array<number[]>>) {
    let xAll: number[] = [];
    let yAll: number[] = [];

    for (let i = 0; i < coords[0].length; i++) {
      xAll.push(coords[0][i][1]);
      yAll.push(coords[0][i][0]);
    }

    xAll = xAll.sort((a, b) => { return a - b; });
    yAll = yAll.sort((a, b) => { return a - b; });

    return [
      [xAll[0], yAll[0]],
      [xAll[xAll.length - 1], yAll[yAll.length - 1]]
    ];
  }

  public static pointInBoundingBox = function (point, bounds) {
    return !(point.coordinates[1] < bounds[0][0] || point.coordinates[1] > bounds[1][0] || point.coordinates[0] < bounds[0][1] || point.coordinates[0] > bounds[1][1]);
  };

  // Point in Polygon
  // http://www.ecse.rpi.edu/Homepages/wrf/Research/Short_Notes/pnpoly.html#Listing the Vertices

  public static pnpoly(x, y, coords) {
    let vert = [[0, 0]];

    for (let i = 0; i < coords.length; i++) {
      for (let j = 0; j < coords[i].length; j++) {
        vert.push(coords[i][j]);
      }
      vert.push(coords[i][0]);
      vert.push([0, 0]);
    }

    let inside = false;
    for (let i = 0, j = vert.length - 1; i < vert.length; j = i++) {
      if (((vert[i][0] > y) !== (vert[j][0] > y)) && (x < (vert[j][1] - vert[i][1]) * (y - vert[i][0]) / (vert[j][0] - vert[i][0]) + vert[i][1])) {
        inside = !inside;
      }
    }

    return inside;
  }

  /**
   * Get point in polygon
   * 
   * @static
   * @param {any} p
   * @param {any} poly
   * @returns
   * 
   * @memberOf GeoJSONUtils
   */
  public static pointInPolygon(p, poly: GeoJSON.GeometryObject) {
    let coords = (poly.type === 'Polygon') ? [poly.coordinates] : poly.coordinates;

    let insideBox = false;
    for (let i = 0; i < coords.length; i++) {
      if (GeoJSONUtils.pointInBoundingBox(p, GeoJSONUtils.boundingBoxAroundPolyCoords(coords[i]))) { insideBox = true; }
    }
    if (!insideBox) { return false; }

    let insidePoly = false;
    for (let i = 0; i < coords.length; i++) {
      if (GeoJSONUtils.pnpoly(p.coordinates[1], p.coordinates[0], coords[i])) { insidePoly = true; }
    }

    return insidePoly;
  }

  // support multi (but not donut) polygons
  public static pointInMultiPolygon(p, poly) {
    let coordsArray = (poly.type === 'MultiPolygon') ? [poly.coordinates] : poly.coordinates;

    let insideBox = false;
    let insidePoly = false;
    for (let i = 0; i < coordsArray.length; i++) {
      let coords = coordsArray[i];
      for (let j = 0; j < coords.length; j++) {
        if (!insideBox) {
          if (GeoJSONUtils.pointInBoundingBox(p, GeoJSONUtils.boundingBoxAroundPolyCoords(coords[j]))) {
            insideBox = true;
          }
        }
      }
      if (!insideBox) { return false; }
      for (let j = 0; j < coords.length; j++) {
        if (!insidePoly) {
          if (GeoJSONUtils.pnpoly(p.coordinates[1], p.coordinates[0], coords[j])) {
            insidePoly = true;
          }
        }
      }
    }

    return insidePoly;
  }

  public static numberToRadius(x: number) {
    return x * Math.PI / 180;
  }

  public static numberToDegree(x: number) {
    return x * 180 / Math.PI;
  }

  // written with help from @tautologe
  public static drawCircle(radiusInMeters, centerPoint, steps = 15) {
    let center = [centerPoint.coordinates[1], centerPoint.coordinates[0]];
    let dist = (radiusInMeters / 1000) / 6371;
    // convert meters to radiant
    let radCenter = [GeoJSONUtils.numberToRadius(center[0]), GeoJSONUtils.numberToRadius(center[1])];
    // 15 sided circle
    let poly = [[center[0], center[1]]];
    for (let i = 0; i < steps; i++) {
      let brng = 2 * Math.PI * i / steps;
      let lat = Math.asin(Math.sin(radCenter[0]) * Math.cos(dist)
        + Math.cos(radCenter[0]) * Math.sin(dist) * Math.cos(brng));
      let lng = radCenter[1] + Math.atan2(Math.sin(brng) * Math.sin(dist) * Math.cos(radCenter[0]),
        Math.cos(dist) - Math.sin(radCenter[0]) * Math.sin(lat));
      poly[i] = [];
      poly[i][1] = GeoJSONUtils.numberToDegree(lat);
      poly[i][0] = GeoJSONUtils.numberToDegree(lng);
    }
    return {
      'type': 'Polygon',
      'coordinates': [poly]
    };
  }

  // assumes rectangle starts at lower left point
  public static rectangleCentroid(rectangle) {
    let bbox = rectangle.coordinates[0];
    let xmin = bbox[0][0];
    let ymin = bbox[0][1];
    let xmax = bbox[2][0];
    let ymax = bbox[2][1];
    let xwidth = xmax - xmin;
    let ywidth = ymax - ymin;
    return {
      'type': 'Point',
      'coordinates': [xmin + xwidth / 2, ymin + ywidth / 2]
    };
  }

  // from http://www.movable-type.co.uk/scripts/latlong.html
  public static pointDistance(pt1, pt2) {
    let lon1 = pt1.coordinates[0];
    let lat1 = pt1.coordinates[1];
    let lon2 = pt2.coordinates[0];
    let lat2 = pt2.coordinates[1];
    let dLat = GeoJSONUtils.numberToRadius(lat2 - lat1);
    let dLon = GeoJSONUtils.numberToRadius(lon2 - lon1);
    let a = Math.pow(Math.sin(dLat / 2), 2) + Math.cos(GeoJSONUtils.numberToRadius(lat1))
      * Math.cos(GeoJSONUtils.numberToRadius(lat2)) * Math.pow(Math.sin(dLon / 2), 2);
    let c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a));
    return (6371 * c) * 1000; // returns meters
  }

  // checks if geometry lies entirely within a circle
  // works with Point, LineString, Polygon
  public static geometryWithinRadius(geometry, center, radius) {
    if (geometry.type === 'Point') {
      return GeoJSONUtils.pointDistance(geometry, center) <= radius;
    } else if (geometry.type === 'LineString' || geometry.type === 'Polygon') {
      let point = <{ coordinates: any }>{};
      let coordinates: number[];
      if (geometry.type === 'Polygon') {
        // it's enough to check the exterior ring of the Polygon
        coordinates = geometry.coordinates[0];
      } else {
        coordinates = geometry.coordinates;
      }
      for (let i in coordinates) {
        if (coordinates.hasOwnProperty[i]) { continue; }
        point.coordinates = coordinates[i];
        if (GeoJSONUtils.pointDistance(point, center) > radius) {
          return false;
        }
      }
    }
    return true;
  }

  /**
   * Compute the area of a polygon
   * adapted from http://paulbourke.net/geometry/polyarea/javascript.txt
   * 
   * @static
   * @param {GeoJSON.GeometryObject} polygon
   * @returns
   * 
   * @memberOf GeoJSONUtils
   */
  public static area(polygon: GeoJSON.GeometryObject) {
    let area = 0;
    // TODO: polygon holes at coordinates[1]
    let points = polygon.coordinates[0];
    let j = points.length - 1;

    for (let i = 0; i < points.length; j = i++) {
      let p1 = {
        x: points[i][1],
        y: points[i][0]
      };
      let p2 = {
        x: points[j][1],
        y: points[j][0]
      };
      area += p1.x * p2.y;
      area -= p1.y * p2.x;
    }

    area /= 2;
    return area;
  }


  /**
   * Compute the (weighted) centroid of a polygon.
   * adapted from http://paulbourke.net/geometry/polyarea/javascript.txt
   * 
   * @static
   * @param {GeoJSON.GeometryObject} polygon
   * @returns
   * 
   * @memberOf GeoJSONUtils
   */
  public static centroid(polygon: GeoJSON.GeometryObject) {
    let x = 0;
    let y = 0;
    // TODO: polygon holes at coordinates[1]
    let points = polygon.coordinates[0];
    let j = points.length - 1;

    for (let i = 0; i < points.length; j = i++) {
      let p1 = {
        x: points[i][1],
        y: points[i][0]
      };
      let p2 = {
        x: points[j][1],
        y: points[j][0]
      };
      let f = p1.x * p2.y - p2.x * p1.y;
      x += (p1.x + p2.x) * f;
      y += (p1.y + p2.y) * f;
    }

    let a = GeoJSONUtils.area(polygon) * 6;
    return {
      'type': 'Point',
      'coordinates': [y / a, x / a]
    };
  }

  public static simplify(source, kink) { /* source[] array of geojson points */
    /* kink	in metres, kinks above GeoJSONUtils depth kept  */
    /* kink depth is the height of the triangle abc where a-b and b-c are two consecutive line segments */
    kink = kink || 20;
    source = source.map(function (o) {
      return {
        lng: o.coordinates[0],
        lat: o.coordinates[1]
      };
    });

    let nSource; let nStack; let nDest; let start; let end; let i; let sig;
    let devSqr; let maxDevSqr; let bandSqr;
    let x12; let y12; let d12; let x13; let y13; let d13; let x23; let y23; let d23;
    let F = (Math.PI / 180.0) * 0.5;
    let index = new Array(); /* aray of indexes of source points to include in the reduced line */
    let sigStart = new Array(); /* indices of start & end of working section */
    let sigEnd = new Array();

    /* check for simple cases */

    if (source.length < 3) return (source); /* one or two points */

    /* more complex case. initialize stack */

    nSource = source.length;
    bandSqr = kink * 360.0 / (2.0 * Math.PI * 6378137.0); /* Now in degrees */
    bandSqr *= bandSqr;
    nDest = 0;
    sigStart[0] = 0;
    sigEnd[0] = nSource - 1;
    nStack = 1;

    /* while the stack is not empty  ... */
    while (nStack > 0) {
      /* ... pop the top-most entries off the stacks */
      start = sigStart[nStack - 1];
      end = sigEnd[nStack - 1];
      nStack--;

      if ((end - start) > 1) { /* any intermediate points ? */
        /* ... yes, so find most deviant intermediate point to
        either side of line joining start & end points */
        x12 = (source[end].lng() - source[start].lng());
        y12 = (source[end].lat() - source[start].lat());
        if (Math.abs(x12) > 180.0) { x12 = 360.0 - Math.abs(x12); }
        x12 *= Math.cos(F * (source[end].lat() + source[start].lat())); /* use avg lat to reduce lng */
        d12 = (x12 * x12) + (y12 * y12);

        for (i = start + 1, sig = start, maxDevSqr = -1.0; i < end; i++) {

          x13 = source[i].lng() - source[start].lng();
          y13 = source[i].lat() - source[start].lat();
          if (Math.abs(x13) > 180.0) { x13 = 360.0 - Math.abs(x13); }
          x13 *= Math.cos(F * (source[i].lat() + source[start].lat()));
          d13 = (x13 * x13) + (y13 * y13);

          x23 = source[i].lng() - source[end].lng();
          y23 = source[i].lat() - source[end].lat();
          if (Math.abs(x23) > 180.0) { x23 = 360.0 - Math.abs(x23); }
          x23 *= Math.cos(F * (source[i].lat() + source[end].lat()));
          d23 = (x23 * x23) + (y23 * y23);

          if (d13 >= (d12 + d23)) {
            devSqr = d23;
          } else if (d23 >= (d12 + d13)) {
            devSqr = d13;
          } else {
            devSqr = (x13 * y12 - y13 * x12) * (x13 * y12 - y13 * x12) / d12; // solve triangle
          }
          if (devSqr > maxDevSqr) {
            sig = i;
            maxDevSqr = devSqr;
          }
        }

        if (maxDevSqr < bandSqr) { /* is there a sig. intermediate point ? */
          /* ... no, so transfer current start point */
          index[nDest] = start;
          nDest++;
        } else { /* ... yes, so push two sub-sections on stack for further processing */
          nStack++;
          sigStart[nStack - 1] = sig;
          sigEnd[nStack - 1] = end;
          nStack++;
          sigStart[nStack - 1] = start;
          sigEnd[nStack - 1] = sig;
        }
      } else { /* ... no intermediate points, so transfer current start point */
        index[nDest] = start;
        nDest++;
      }
    }

    /* transfer last point */
    index[nDest] = nSource - 1;
    nDest++;

    /* make return array */
    let r = new Array();
    for (let j = 0; i < nDest; j++) {
      r.push(source[index[j]]);
    }

    return r.map(function (o) {
      return {
        type: 'Point',
        coordinates: [o.lng, o.lat]
      };
    });
  }

  // http://www.movable-type.co.uk/scripts/latlong.html#destPoint
  public static destinationPoint(pt, brng, dist) {
    dist = dist / 6371;  // convert dist to angular distance in radians
    brng = GeoJSONUtils.numberToRadius(brng);

    let lon1 = GeoJSONUtils.numberToRadius(pt.coordinates[0]);
    let lat1 = GeoJSONUtils.numberToRadius(pt.coordinates[1]);

    let lat2 = Math.asin(Math.sin(lat1) * Math.cos(dist) +
      Math.cos(lat1) * Math.sin(dist) * Math.cos(brng));
    let lon2 = lon1 + Math.atan2(Math.sin(brng) * Math.sin(dist) * Math.cos(lat1),
      Math.cos(dist) - Math.sin(lat1) * Math.sin(lat2));
    lon2 = (lon2 + 3 * Math.PI) % (2 * Math.PI) - Math.PI;  // normalise to -180..+180º

    return {
      'type': 'Point',
      'coordinates': [GeoJSONUtils.numberToDegree(lon2), GeoJSONUtils.numberToDegree(lat2)]
    };
  };

}
