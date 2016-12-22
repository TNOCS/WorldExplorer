import * as path from 'path';
import { Process } from './process';
import * as commandLineArgs from 'command-line-args';

export interface ICommandLineOptions {
  /**
   * Input folder
   *
   * @type {string}
   * @memberOf ICommandLineOptions
   */
  folder: string;
  /** Specify the output file (default 'assets.json') */
  output: string;
  /** If true, show the help information */
  help: boolean;
}

export class CommandLineInterface {
  static optionDefinitions = [
    { name: 'help', alias: '?', type: Boolean, multiple: false, typeLabel: '[underline]{Help}', description: 'Display help information.' },
    { name: 'folder', alias: 'f', type: String, multiple: false, typeLabel: '[underline]{Input folder}', description: 'Folder containing the *.meta files (default is current folder)', defaultValue: process.cwd() },
    { name: 'output', alias: 'o', type: String, multiple: false, typeLabel: '[underline]{Output GeoJSON}', description: 'Output GeoJSON file', defaultValue: path.join(process.cwd(), 'assets.json') }
  ];

  static sections = [{
    header: 'Extract geo-location from Unity assets',
    content: 'Process a folder with Unity assets (text-based *.meta files) and convert it to a GeoJSON file.'
  }, {
    header: 'Options',
    optionList: CommandLineInterface.optionDefinitions
  }
  ];
}

let options: ICommandLineOptions = commandLineArgs(CommandLineInterface.optionDefinitions);

if (options.help) {
    const getUsage = require('command-line-usage');
    const usage = getUsage(CommandLineInterface.sections);
    console.log(usage);
    process.exit(1);
}

if (!options || typeof options !== 'object') options = <ICommandLineOptions>{};
options.folder = options.folder ? path.resolve(options.folder) : path.resolve('data');
const p = new Process(options);
