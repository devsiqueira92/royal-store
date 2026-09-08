import {
  ChangeDetectionStrategy,
  Component,
  input,
  output,
} from '@angular/core';
import { PercentPipe } from '@angular/common';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { TranslatePipe } from '@ngx-translate/core';
import { IconFieldModule } from 'primeng/iconfield';
import { InputIconModule } from 'primeng/inputicon';
import { InputTextModule } from 'primeng/inputtext';
import { Customer } from '../../interfaces/customer.interface';

@Component({
  selector: 'app-customer-step',
  imports: [
    PercentPipe,
    ReactiveFormsModule,
    TranslatePipe,
    IconFieldModule,
    InputIconModule,
    InputTextModule,
  ],
  templateUrl: './customer-step.component.html',
  styleUrl: './customer-step.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CustomerStepComponent {
  customers = input.required<Customer[]>();
  search = input.required<FormControl<string>>();
  selectedId = input<string | null>(null);

  customerSelected = output<string | null>();
}
