import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';

@Component({
  selector: 'app-not-found-page',
  templateUrl: './not-found-page.component.html',
  styleUrls: ['./not-found-page.component.css'],
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    class: 'd-flex flex-grow-1'
  }
})
export class NotFoundPageComponent implements OnInit {

  constructor() { }

  ngOnInit() {
  }

}
