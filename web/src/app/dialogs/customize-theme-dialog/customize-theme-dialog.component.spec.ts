import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CustomizeThemeDialogComponent } from './customize-theme-dialog.component';

describe('CustomizeThemeDialogComponent', () => {
  let component: CustomizeThemeDialogComponent;
  let fixture: ComponentFixture<CustomizeThemeDialogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CustomizeThemeDialogComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CustomizeThemeDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
