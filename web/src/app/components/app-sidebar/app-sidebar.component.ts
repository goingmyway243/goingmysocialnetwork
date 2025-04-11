import { Component } from '@angular/core';
import { CreatePostComponent } from "../create-post/create-post.component";
import { UserAvatarComponent } from "../user-avatar/user-avatar.component";
import { NavigationEnd, Router } from '@angular/router';
import { AppCommonComponent } from '../app-common/app-common.component';
import { AuthService } from '../../common/services/auth.service';
import { filter } from 'rxjs';
import { MatDialog } from '@angular/material/dialog';
import { CustomizeThemeDialogComponent } from '../../dialogs/customize-theme-dialog/customize-theme-dialog.component';

@Component({
    selector: 'app-sidebar',
    templateUrl: './app-sidebar.component.html',
    styleUrl: './app-sidebar.component.scss',
    standalone: true,
    imports: [CreatePostComponent, UserAvatarComponent]
})
export class SidebarComponent extends AppCommonComponent {
    activeMenu: string = 'home'; // Default active menu
    isSmallScreen: boolean = false;

    constructor(private router: Router, private dialog: MatDialog, authSvc: AuthService) {
        super(authSvc);
        this.isSmallScreen = window.innerWidth < 768;
        window.addEventListener('resize', () => {
            this.isSmallScreen = window.innerWidth < 768;
        });
    }

    override onInit(): void {
        this.activeMenu = location.pathname.split('/')[1] || 'home';
        this.router.events
            .pipe(filter(event => event instanceof NavigationEnd))
            .subscribe((event: NavigationEnd) => {
                this.activeMenu = event.url.split('/')[1] || 'home';
            });
    }

    setActiveMenu(menu: string): void {
        this.activeMenu = menu;
        this.navigateTo(menu);
    }

    navigateTo(path: string): void {
        this.router.navigate([path]);
    }

    openCustomizeThemeDialog(): void {
      const previousMenu = this.activeMenu;
      this.activeMenu = 'customize-theme';
      
      this.dialog.open(CustomizeThemeDialogComponent, {
        minWidth: '50vw',
        panelClass: ['custom-panel-dialog', '--with-shadow'],
      }).afterClosed().subscribe(() =>{
        this.activeMenu = previousMenu;
      });
    }
}
