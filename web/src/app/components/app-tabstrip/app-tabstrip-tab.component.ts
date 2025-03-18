import { CommonModule, NgTemplateOutlet } from '@angular/common';
import { Component, Input, TemplateRef } from '@angular/core';

@Component({
  selector: 'app-tabstrip-tab',
  standalone: true,
  imports: [CommonModule, NgTemplateOutlet],
  template: `
  <div *ngIf="canLoaded" 
        class="opal-tab-content"
        [hidden]="shouldHide" 
        [ngClass]="cssClass">
    <ng-container *ngTemplateOutlet="tabContent"></ng-container>
  </div>
`
})
export class AppTabstripTabComponent {
  @Input() tabTitle: TemplateRef<any> | null = null;
  @Input() tabContent: TemplateRef<any> | null = null;
  @Input() tabName: string = '';
  @Input() selected: boolean = false;
  @Input() cssClass: { key: string, condition: boolean } | string | undefined;
  @Input() cssHeader: { key: string, condition: boolean } | string | undefined;
  @Input() disabled: boolean = false;

  public index: number = -1;

  public keepTabContent: boolean = false;

  public get canLoaded(): boolean {
    return this.keepTabContent || this.selected;
  }

  public get shouldHide(): boolean {
    return this.keepTabContent && !this.selected;
  }

  public onTabSelected(): void {
    this.selected = true;
  }
}
