import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AppTabstripComponent } from './app-tabstrip.component';

describe('AppTabstripComponent', () => {
  let component: AppTabstripComponent;
  let fixture: ComponentFixture<AppTabstripComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AppTabstripComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AppTabstripComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
