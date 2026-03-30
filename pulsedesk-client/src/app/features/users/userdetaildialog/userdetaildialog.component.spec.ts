import { ComponentFixture, TestBed } from '@angular/core/testing';

import { UserdetaildialogComponent } from './userdetaildialog.component';

describe('UserdetaildialogComponent', () => {
  let component: UserdetaildialogComponent;
  let fixture: ComponentFixture<UserdetaildialogComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [UserdetaildialogComponent]
    });
    fixture = TestBed.createComponent(UserdetaildialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
