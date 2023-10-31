import { Component, OnDestroy, OnInit } from '@angular/core';
import { AdminService } from '../../_services/admin.service';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-photo-management',
  templateUrl: './photo-management.component.html',
  styleUrls: ['./photo-management.component.css']
})
export class PhotoManagementComponent implements OnDestroy {
    subscription: Subscription | undefined;

  constructor(public adminService: AdminService) {
    
  }

  ngOnDestroy(): void {
    this.subscription?.unsubscribe();
  }

  moderatePhoto(photoId: number, approved: boolean) {

    this.adminService.moderatePhoto(photoId, approved)?.catch((error) => {
      console.error(error);
    });
  }
}
