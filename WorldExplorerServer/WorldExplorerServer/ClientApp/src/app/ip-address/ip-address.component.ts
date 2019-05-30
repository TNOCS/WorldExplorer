import { Component, OnInit, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';


@Component({
  selector: 'app-ip-address',
  templateUrl: './ip-address.component.html',
  styleUrls: ['./ip-address.component.css']
})
export class IpAddressComponent implements OnInit {

  url : string = "";
  constructor(public http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
    http.get<string>(baseUrl + 'api/Configuration/Url').subscribe(result => {
      this.url = result;
      debugger;
    }, error => { this.url = error; debugger; });
  }

  ngOnInit() {
    
  }
  

}
