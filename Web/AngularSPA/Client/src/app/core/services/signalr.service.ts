import { Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder, LogLevel, HubConnectionState } from '@microsoft/signalr';
import { Store, select } from '@ngrx/store';
import { getSignedIn, getOidcUser } from 'src/app/auth/reducers';
import { getAppSettings } from '../reducers';
import { tap, withLatestFrom } from 'rxjs/operators';

@Injectable()
export class SignalrService {

    private hubConnection: HubConnection;
    private signalrHubUrl: string;
    private accessToken: string
    private handlers: {[key: string]: any} = {};

    constructor(private store: Store<any>) {
        this.store.select(getSignedIn).pipe(
            withLatestFrom(this.store.pipe(select(getAppSettings)), this.store.select(getOidcUser)),
            tap(([signedIn, appSettings, user]) => {
                this.signalrHubUrl = appSettings.messagingUrl;
                this.accessToken = user.access_token;
                if (signedIn && (!this.hubConnection || this.hubConnection.state != HubConnectionState.Connected)) {
                    this.start();
                }
                else if (!signedIn) {
                    this.stop();
                }
            })
        ).subscribe();
    }

    public stop() {
        if (this.hubConnection) {
            this.hubConnection.stop();
        }
    }

    public start() {
        if (this.hubConnection && this.hubConnection.state == HubConnectionState.Connected) {
            return;
        }
        this.register();
        this.establishConnection();
        this.registerHandlers();
    }

    public addHandler(method: string, handler: any) {
        this.handlers[method] = handler;
        if (this.hubConnection && this.hubConnection.state == HubConnectionState.Connected) {
            this.hubConnection.on(method, handler);
        }
    }

    private register() {
        this.hubConnection = new HubConnectionBuilder()
            .withUrl(this.signalrHubUrl + '/hub/messaginghub', {
                accessTokenFactory: () => this.accessToken
            })
            .configureLogging(LogLevel.Information)
            .withAutomaticReconnect()
            .build();
    }

    private establishConnection() {
        this.hubConnection.start()
            .then(() => {
                console.log('Hub connection started')
            })
            .catch(() => {
                console.log('Error while establishing connection')
            });
    }

    private registerHandlers() {
        for (let [method, handler] of Object.entries(this.handlers)) {
            this.hubConnection.on(method, handler);
        }
    }

    // private registerHandlers() {
    //     this.hubConnection.on('FriendshipRequestReceived', (msg: FriendshipRequest) => {
    //         console.log(`Friendship request received from ${msg.requester.id}`);
    //         const normalized = normalize(msg, friendshipRequestSchema);
    //         const friendshipRequest = Object.values(normalized.entities.friendshipRequests)[0] as FriendshipRequestEntity;
    //         const requestee = normalized.entities.users[msg.requester.id] as UserEntity;
    //         this.store.dispatch(new AddEntity(FriendshipRequestEntity.name, {entity: friendshipRequest}));
    //         this.store.dispatch(new UpsertEntity(UserEntity.name, {entity: requestee}));
    //     });
    // }
}