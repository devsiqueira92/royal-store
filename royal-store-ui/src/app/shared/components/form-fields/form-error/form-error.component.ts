import { Component, input } from '@angular/core';
import { UntypedFormGroup } from '@angular/forms';
import { TranslatePipe } from '@ngx-translate/core';
import { Message } from 'primeng/message';

@Component({
  selector: 'app-form-error',
  imports: [Message, TranslatePipe],
  templateUrl: './form-error.component.html',
  styleUrl: './form-error.component.scss',
})
export class FormErrorComponent {
  parentForm = input.required<UntypedFormGroup>();
  controlName = input.required<string>();

  objectKeys = Object.keys;
}
