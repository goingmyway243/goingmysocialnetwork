import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RequestItemComponent } from './request-item.component';

describe('RequestItemComponent', () => {
  let component: RequestItemComponent;
  let fixture: ComponentFixture<RequestItemComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RequestItemComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(RequestItemComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
