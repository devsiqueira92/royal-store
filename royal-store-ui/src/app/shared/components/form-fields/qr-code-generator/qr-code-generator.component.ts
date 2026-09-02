import { Component, computed, input } from '@angular/core';
import { UntypedFormGroup } from '@angular/forms';
import qrcode from 'qrcode-generator';

@Component({
  selector: 'app-qr-code-generator',
  imports: [],
  templateUrl: './qr-code-generator.component.html',
  styleUrl: './qr-code-generator.component.scss',
})
export class QrCodeGeneratorComponent {
  parentForm = input.required<UntypedFormGroup>();
  controlName = input.required<string>();

  protected readonly qrDataUrl = computed<string | null>(() => {
    const barcode = this.parentForm().get(this.controlName())?.value;
    if (!barcode) {
      return null;
    }

    const qr = qrcode(0, 'M');
    qr.addData(barcode);
    qr.make();
    return qr.createDataURL(3, 0);
  });
}
