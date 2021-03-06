import { Injectable } from '@angular/core';
import { CanActivate, Router } from '@angular/router';
import { Store, select } from '@ngrx/store';
import * as fromAuth from '../../auth/reducers';
import { Observable, of } from 'rxjs';
import { tap, map } from 'rxjs/operators';
import { LoadSelf } from '../../user/actions/user.actions';
import { LoadFriendshipRequests } from 'src/app/user/actions/friendship-request.actions';

@Injectable({
  providedIn: 'root'
})
export class ChatGuardService implements CanActivate {

  constructor(private store: Store<fromAuth.State>, private router: Router) { }

  canActivate(): Observable<boolean> {
    return this.store.pipe(
      select(fromAuth.getSignedIn),
      tap(signedIn => {
        if (signedIn) {
          this.store.dispatch(new LoadSelf());
        }
        else {
          this.router.navigate(['auth', 'login']);
        }
      })
    );
  }
}
