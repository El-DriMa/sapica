import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AdoptionPostAdminComponent } from './adoption-post-admin.component';

describe('AdoptionPostAdminComponent', () => {
  let component: AdoptionPostAdminComponent;
  let fixture: ComponentFixture<AdoptionPostAdminComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AdoptionPostAdminComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(AdoptionPostAdminComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
