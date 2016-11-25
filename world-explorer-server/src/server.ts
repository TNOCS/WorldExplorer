import * as express from 'express';
import * as path from 'path';
import * as cors from 'cors';
import * as ip from 'ip';
import { ITile } from './models/tile-service';
import { TileServer } from './services/tile-server';
import { IServerConfig } from './models/config';

console.log(__dirname);
console.log(__filename);
console.log(process.cwd());

const config: IServerConfig = require(process.cwd() + '/config.json');
config.cache = path.resolve(config.cache ? config.cache : 'cache');

const app = express();

app.use(cors());
app.use(express.static(path.join(process.cwd(), 'public/assets')));

let tileServer = new TileServer(config.url, config.cache);

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

app.listen(config.port, () => {
  console.log(`Listening on http://${ip.address()}:${config.port}...`);
});
