import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { LabelPrintPreviewComponent } from '../../components/label-print-preview/label-print-preview.component';

@Component({
  imports: [TranslatePipe, RouterLink, LabelPrintPreviewComponent],
  templateUrl: './label-print.component.html',
  styleUrl: './label-print.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LabelPrintComponent {}
