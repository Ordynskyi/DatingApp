import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { PaginatedResult } from '../_models/pagination';
import { Photo } from '../_models/photo';
import { User } from '../_models/user';
import { getPaginatedResult, getPaginationHeaders } from './paginationHelper';

@Injectable({
  providedIn: 'root'
})
export class AdminService {
  baseUrl = environment.apiUrl;

  constructor(private http: HttpClient) { 

  }

  getUsersWithRoles() {
    return this.http.get<User[]>(`${this.baseUrl}admin/users-with-roles`)
  }

  updateUserRoles(username: string, roles: string[]) {
    return this.http.post<string[]>(
      `${this.baseUrl}admin/edit-roles/${username}?roles=${roles}`, {});
  }

  getModerationPhotos(page: number, pageSize: number) {
    var headers = getPaginationHeaders(page, pageSize);
    return getPaginatedResult<Photo[]>(`${this.baseUrl}admin/moderation-photos`, headers, this.http);
  }

  moderatePhoto(photoId: number, approved: boolean) {
    return this.http.put(
      `${this.baseUrl}admin/moderate-photo?photoId=${photoId}&approved=${approved}`, {});
  }
}
