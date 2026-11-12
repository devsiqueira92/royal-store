import { ChangeDetectionStrategy, Component, inject, Input, output } from '@angular/core';
import { ReactiveFormsModule, UntypedFormGroup, Validators } from '@angular/forms';
import { TranslatePipe } from '@ngx-translate/core';
import { FloatLabelModule } from 'primeng/floatlabel';
import { SelectChangeEvent, SelectModule } from 'primeng/select';
import { FormErrorComponent } from '../form-error/form-error.component';
import { InputGroup } from 'primeng/inputgroup';
import { InputGroupAddon } from 'primeng/inputgroupaddon';
import { TooltipModule } from 'primeng/tooltip';
import { OriginOptionFacade } from '@app/core/origin-option/facade/origin-option.facade';
import { toSignal } from '@angular/core/rxjs-interop';
import { map } from 'rxjs';
import { AsyncPipe } from '@angular/common';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-origin-select-field',
  imports: [
    ReactiveFormsModule,
    SelectModule,
    FloatLabelModule,
    TranslatePipe,
    FormErrorComponent,
    InputGroup,
    InputGroupAddon,
    TooltipModule,
    AsyncPipe,
  ],
  templateUrl: './origin-select-field.component.html',
  styleUrl: './origin-select-field.component.scss',
})
export class OriginSelectFieldComponent {
  @Input({ required: true }) parentForm!: UntypedFormGroup;
  @Input({ required: true }) controlName!: string;
  @Input() label?: string;
  @Input() placeholder?: string;
  @Input() useHint?: boolean = true;

  onSelect = output<SelectChangeEvent>();

  private readonly originOptionFacade = inject(OriginOptionFacade);
  protected readonly Validators = Validators;

  protected readonly loading$ = this.originOptionFacade.isLoading$;
  protected hint: string = '';
  protected readonly originOptions = toSignal(
    this.originOptionFacade.originOptions$.pipe(
      map(options =>
        options.map(option => ({
          code: String(option.code),
          description: option.name,
          hint: option.description,
        })),
      ),
    ),
    { initialValue: [] },
  );

  ngOnInit() {
    this.getHintValue(this.parentForm.controls[this.controlName].value);
  }

  selectOption(event: SelectChangeEvent) {
    this.getHintValue(event.value);
    this.onSelect.emit(event);
  }

  private getHintValue(value: string) {
    const selectedOption = this.originOptions().find(opt => opt.code === value);
    this.hint = selectedOption!.hint!;
  }
}
