import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ShelterProfileComponent } from './shelter-profile.component';

describe('ShelterProfileComponent', () => {
  let component: ShelterProfileComponent;
  let fixture: ComponentFixture<ShelterProfileComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ShelterProfileComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(ShelterProfileComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
