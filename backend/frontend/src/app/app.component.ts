import { Component } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent {
  title = 'CognifyCommerce-Frontend';

  constructor(private http: HttpClient) {}

  sendApiRequest() {
    this.http.get(environment.apiUrl + 'endpoint').subscribe(response => {
      console.log('API yanıtı:', response);
    });
  }
}
