import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { BehaviorSubject } from 'rxjs';
import { environment } from 'src/environments/environment';
import { Photo } from '../_models/photo';
import { User } from '../_models/user';

@Injectable({
  providedIn: 'root'
})

export class AdminService {
  baseUrl = environment.apiUrl;
  hubBaseUrl = environment.hubUrl;
  moderationHub: HubConnection | undefined;
  private moderationPhotosSource = new BehaviorSubject<Photo[]>([]);
  moderationPhotos$ = this.moderationPhotosSource.asObservable();

  constructor(private http: HttpClient) { 

  }

  createModerationHub(user: User) {
    if (this.moderationHub) {
      return;
    }

    this.moderationHub = new HubConnectionBuilder()
      .withUrl(`${this.hubBaseUrl}moderation`, {
        accessTokenFactory: () => user.token
      })
      .withAutomaticReconnect()
      .build();

    this.moderationHub.on('ReceiveModerationPool', (photos: Photo[]) => {
      this.moderationPhotosSource.next(photos);
    });

    this.moderationHub.on('PhotoAdded', (photo: Photo) => {
      if (photo === null) return;
      var curArray = this.moderationPhotosSource.getValue();
      curArray.push(photo);
      this.moderationPhotosSource.next(curArray);
    });

    this.moderationHub.on('PhotoModerated', (photoId: number, replacement: Photo) => {
      let curArray = this.moderationPhotosSource.getValue();
      const index = curArray.findIndex(p => p.id === photoId);

      if (index === -1) {
        console.error(`Photo ${photoId} was not found in the moderation pool`);
        return;
      }

      if (replacement) {
        curArray[index] = replacement;
      } else {
        curArray.splice(index, 1);
      }      

      this.moderationPhotosSource.next(curArray);
    });

    this.moderationHub.start().catch(error => console.error(error));
  }

  stopModerationHub() {
    if (!this.moderationHub) {
      return;
    }

    this.moderationHub.stop().catch(error => console.error(error));
    this.moderationHub = undefined;
  }

  getUsersWithRoles() {
    return this.http.get<User[]>(`${this.baseUrl}admin/users-with-roles`)
  }

  updateUserRoles(username: string, roles: string[]) {
    return this.http.post<string[]>(
      `${this.baseUrl}admin/edit-roles/${username}?roles=${roles}`, {});
  }

  moderatePhoto(photoId: number, approved: boolean) {
    if (!this.moderationHub) {
      console.error("Photo moderation hub connection has not been created");
      return;
    }

    return this.moderationHub.invoke('ModeratePhoto', photoId, approved);
  }
}
