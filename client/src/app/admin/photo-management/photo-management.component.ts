import { Component, OnInit } from '@angular/core';
import { Pagination } from '../../_models/pagination';
import { Photo } from '../../_models/photo';
import { AdminService } from '../../_services/admin.service';

@Component({
  selector: 'app-photo-management',
  templateUrl: './photo-management.component.html',
  styleUrls: ['./photo-management.component.css']
})
export class PhotoManagementComponent implements OnInit {
  moderationPhotos: Photo[] = [];
  pagination: Pagination = {
    currentPage: 1,
    itemsPerPage: 12,
    totalItems: 0,
    totalPages: 0,
   };

  constructor(private adminService: AdminService) { }

  ngOnInit(): void {
    this.loadModerationPhotos();
  }

  loadModerationPhotos() {
    this.adminService.getModerationPhotos(
      this.pagination.currentPage, this.pagination.itemsPerPage).subscribe({
        next: paginatedResult => {
          if (paginatedResult.result) {
            this.moderationPhotos = paginatedResult.result;
          }

          if (paginatedResult.pagination) {
            this.pagination = paginatedResult.pagination;
          }
        },
        error: err => console.error(err)
      });
  }

  moderatePhoto(photoId: number, approved: boolean) {
    this.adminService.moderatePhoto(photoId, approved).subscribe({
      next: () => {
        this.moderationPhotos.splice(
          this.moderationPhotos.findIndex(p => p.id === photoId), 1);
      },
      error: err => console.error(err)
    });
  }

  pageChanged(event: any) {
    if (this.pagination.currentPage === event.page) return;

    this.pagination.currentPage = event.page;
    this.loadModerationPhotos();
  }
}
