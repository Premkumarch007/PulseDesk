import { ComponentFixture, TestBed } from '@angular/core/testing';

import { UpdateRoledialogComponent } from './update-roledialog.component';

describe('UpdateRoledialogComponent', () => {
  let component: UpdateRoledialogComponent;
  let fixture: ComponentFixture<UpdateRoledialogComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [UpdateRoledialogComponent]
    });
    fixture = TestBed.createComponent(UpdateRoledialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
