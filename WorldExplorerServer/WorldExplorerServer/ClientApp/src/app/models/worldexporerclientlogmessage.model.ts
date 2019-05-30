
export class WorldExplorerClientLogMessage {

  topic: string;
  message: string;
 

  constructor(topic: string = '', message: string = '') {
    this.topic = topic;
    this.message = message;
 
  }
}
