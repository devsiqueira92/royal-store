import { ChangeDetectionStrategy, Component, Input, inject } from '@angular/core';
import {
  ReactiveFormsModule,
  UntypedFormGroup,
  Validators,
} from '@angular/forms';
import { TranslatePipe } from '@ngx-translate/core';
import { FloatLabelModule } from 'primeng/floatlabel';
import { MessageModule } from 'primeng/message';
import { SelectModule } from 'primeng/select';
import { ColorPaletteService } from '@app/shared/services/color-palette.service';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-color-select-field',
  imports: [
    ReactiveFormsModule,
    SelectModule,
    MessageModule,
    FloatLabelModule,
    TranslatePipe,
  ],
  templateUrl: './color-select-field.component.html',
  styleUrl: './color-select-field.component.scss',
})
export class ColorSelectFieldComponent {
  @Input({ required: true }) parentForm!: UntypedFormGroup;
  @Input({ required: true }) controlName!: string;
  @Input() label?: string;
  @Input() placeholder?: string;
  @Input() id?: string;
  @Input() showClear: boolean = true;

  private readonly colorPalette = inject(ColorPaletteService);
  protected readonly groups = this.colorPalette.families;

  objectKeys = Object.keys;
  Validators = Validators;
}
