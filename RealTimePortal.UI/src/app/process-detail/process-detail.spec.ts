import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ProcessDetail } from './process-detail';

describe('ProcessDetail', () => {
  let component: ProcessDetail;
  let fixture: ComponentFixture<ProcessDetail>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ProcessDetail]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ProcessDetail);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
