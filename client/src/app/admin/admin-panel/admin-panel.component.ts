import { Component, OnDestroy, OnInit } from '@angular/core';
import { take } from 'rxjs';
import { User } from '../../_models/user';
import { AccountService } from '../../_services/account.service';
import { AdminService } from '../../_services/admin.service';

@Component({
  selector: 'app-admin-panel',
  templateUrl: './admin-panel.component.html',
  styleUrls: ['./admin-panel.component.css']
})
export class AdminPanelComponent implements OnInit, OnDestroy {
  user: User | undefined;

  constructor(
    private adminService: AdminService,
    accountService: AccountService) {
    accountService.currentUser$.pipe(take(1)).subscribe({
      next: user => {
        if (user) this.user = user;
      }
    });
  }

  ngOnInit(): void {
    if (!this.user) {
      console.error('the user is not initialized');
      return;
    }

    this.adminService.createModerationHub(this.user);
  }

  ngOnDestroy(): void {
    this.adminService.stopModerationHub();
  }
}
