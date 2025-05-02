import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AdoptionPostComponent } from './adoption-post.component';

describe('AdoptionPostComponent', () => {
  let component: AdoptionPostComponent;
  let fixture: ComponentFixture<AdoptionPostComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AdoptionPostComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(AdoptionPostComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
