import * as express from 'express';
import * as path from 'path';
import * as cors from 'cors';
import * as ip from 'ip';
import { ITile } from './models/tile-service';
import { TileServer } from './services/tile-server';
import { ICommandLineOptions } from './cli';

const log = console.log;

// console.log(__dirname);
// console.log(__filename);
// console.log(process.cwd());
export class Server {
  constructor(options: ICommandLineOptions) {
    const app = express();

    app.use(cors());
    app.use(express.static(path.join(process.cwd(), 'public')));

    let tileServer = new TileServer(options.port, options.url, options.cache, options.server);

    app.get('/:layers/:z/:x/:y.json', (req, res) => {
      let tile = <ITile> {
        layers: (<string> req.params.layers).split(','),
        x: req.params.x,
        y: req.params.y,
        zoom: req.params.z
      };
      tileServer.getTile(tile, (err, collection) => {
        if (err) {
          console.error('Error getting tile: ' + JSON.stringify(tile, null, 2));
          console.error(err);
          return;
        }
        res.setHeader('Content-Type', 'application/json');
        res.send(JSON.stringify(collection));
      });
    });

    app.listen(options.port, () => {
      log(`Listening on ${options.server || 'http://' + ip.address()}:${options.port}...`);
    });
  }
}
