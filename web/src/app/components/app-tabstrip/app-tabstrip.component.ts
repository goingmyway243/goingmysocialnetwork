import { CommonModule } from '@angular/common';
import { AfterContentInit, Component, ContentChildren, EventEmitter, Input, Output, QueryList } from '@angular/core';
import { AppTabstripTabComponent } from './app-tabstrip-tab.component';


@Component({
  selector: 'app-tabstrip',
  standalone: true,
  imports: [CommonModule],
  styleUrl: './app-tabstrip.component.scss',
  template: `
  <ul #headers class="opal-tabstrip-headers" [ngClass]="{ 'end': tabAlignment === 'end'}">
    <li *ngFor="let tab of tabs" 
        class="opal-tabstrip-title"
        (click)="selectTab(tab)" 
        [class.active]="tab.selected"
        [class.disabled]="tab.disabled"
        [ngClass]="tab.cssHeader">
      <ng-container *ngTemplateOutlet="tab.tabTitle"></ng-container>
    </li>
  </ul>
  <div class="opal-tabstrip-container">
    <ng-content></ng-content>
  </div>
  `
})
export class AppTabstripComponent implements AfterContentInit {
  @ContentChildren(AppTabstripTabComponent) tabs!: QueryList<AppTabstripTabComponent>;
  @Input() keepTabsContent: boolean = false;
  @Input() tabAlignment: 'start' | 'end' = 'start';
  @Output() tabSelect: EventEmitter<AppTabStripSelectEvent> = new EventEmitter();

  // contentChildren are set
  public ngAfterContentInit() {
    this.tabs.forEach((tab, index) => {
      tab.index = index;
      tab.keepTabContent = this.keepTabsContent;
    });

    // get all active tabs
    const selectedTabs = this.tabs.filter(tab => tab.selected);

    // if there is no active tab set, activate the first
    if (selectedTabs.length === 0) {
      this.selectTab(this.tabs.first);
    }
  }

  public selectTabByIndex(index: number) {
    const tab = this.tabs.find((tab, i) => i === index);
    if (tab) {
      this.selectTab(tab);
    }
  }

  public selectTab(tab: AppTabstripTabComponent) {
    if (tab.disabled) {
      return;
    }

    // deactivate all tabs
    this.tabs.toArray().forEach(tab => tab.selected = false);

    // activate the tab the user has clicked on.
    tab.onTabSelected();

    this.tabSelect.emit(new AppTabStripSelectEvent(tab.index === -1 ? 0 : tab.index, tab.tabName));
  }
}

export class AppTabStripSelectEvent {
  /**
   * Constructs the event arguments for the `select` event.
   * @param index - The index of the selected tab.
   * @param title - The name of the selected tab.
   */
  constructor(public index: number, public title: string) { }
}
