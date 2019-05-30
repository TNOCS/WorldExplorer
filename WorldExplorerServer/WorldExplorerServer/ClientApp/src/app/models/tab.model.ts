import { WorldExplorerClientLogMessage } from '../Models/worldexporerclientlogmessage.model';

/** Represent Tab class */
export class Tab {
  messageHistory: WorldExplorerClientLogMessage[];
  heading: string;
  title: string;

  constructor(
    heading: string = '',
    title: string = ''
  ) {
    this.heading = heading;
    this.title = title;
    this.messageHistory = [];
  }
}
