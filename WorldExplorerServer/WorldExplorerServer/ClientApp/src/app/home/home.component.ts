import { Component } from '@angular/core';

export class NgxQrCode {
  text: string;
}

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
})
export class HomeComponent {
  public qrdata: string = null;

  constructor() {
    console.log('AppComponent running');
    this.qrdata = 'http://localhost:56367/api/Configuration/default.json';
  }

  changeValue(newValue: string): void {
    this.qrdata = newValue;
  }
}
