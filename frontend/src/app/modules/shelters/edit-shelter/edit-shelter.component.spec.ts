import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EditShelterComponent } from './edit-shelter.component';

describe('EditShelterComponent', () => {
  let component: EditShelterComponent;
  let fixture: ComponentFixture<EditShelterComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EditShelterComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(EditShelterComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
