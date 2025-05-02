import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AddAdoptionRequestComponent } from './add-adoption-request.component';

describe('AddAdoptionRequestComponent', () => {
  let component: AddAdoptionRequestComponent;
  let fixture: ComponentFixture<AddAdoptionRequestComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AddAdoptionRequestComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(AddAdoptionRequestComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
