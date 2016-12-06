import * as path from 'path';
import { Server } from './server';
import { IServerConfig } from './models/config';

const log = console.log;
const commandLineArgs = require('command-line-args');
const config = require(path.join(process.cwd(), 'config.json'));

export interface ICommandLineOptions {
  /**
   * Cache folder (default is `./cache`)
   * 
   * @type {string}
   * @memberOf ICommandLineOptions
   */
  cache: string;
  /**
   * Mapzen URL (default from config)
   * 
   * @type {string}
   * @memberOf ICommandLineOptions
   */
  url: string;
  /**
   * Server port
   * 
   * @type {number}
   * @memberOf IConfig
   */
  port: number;
  /**
   * Show help information.
   * 
   * @type {boolean}
   * @memberOf ICommandLineOptions
   */
  help: boolean;
  /**
   * Configuration options
   * 
   * @type {IConfig}
   * @memberOf ICommandLineOptions
   */
  config: IServerConfig;
}

export class CommandLineInterface {
  public static optionDefinitions = [
    { name: 'help', alias: '?', type: Boolean, multiple: false, typeLabel: '[underline]{Help}', description: 'Display help information.' },
    { name: 'cache', alias: 'c', defaultValue: config.cache || 'cache', type: String, multiple: false, typeLabel: '[underline]{Cache folder}', description: 'Folder for caching the tiles (default ./cache).' },
    { name: 'port', alias: 'p', defaultValue: config.port || 10733, type: Number, multiple: false, typeLabel: '[underline]{Server port}', description: 'Port for the server to use (default 10733).' }
  ];

  public static sections = [{
    header: 'TileServer proxy',
    content: 'Run a tile server proxy, which either retreives vectortiles locally, or uses Mapzen to retreive them. In addition, it adds the assets, as specified in assets.json, to the tiles, and filters existing buildings from the data.'
  }, {
    header: 'Options',
    optionList: CommandLineInterface.optionDefinitions
  }];
}

let options: ICommandLineOptions = commandLineArgs(CommandLineInterface.optionDefinitions);

if (options.help) {
  const getUsage = require('command-line-usage');
  const usage = getUsage(CommandLineInterface.sections);
  log(usage);
  process.exit(1);
}

if (!options || typeof options !== 'object') { options = <ICommandLineOptions>{}; }
options.cache = path.resolve(options.cache);
options.config = config;
options.url = config.url;

process.on('SIGINT', () => {
  log('Exiting...');
  process.exit(0);
});

const server = new Server(options);
