import { ChangeDetectionStrategy, Component, DestroyRef, inject, input } from '@angular/core';
import { FormGroup } from '@angular/forms';
import { ProductFormControls } from '@app/features/product/interfaces/product.interface';
import { combineLatest, filter, map, tap } from 'rxjs';
import { SelectFieldComponent } from '../select-field/select-field.component';
import { AsyncPipe } from '@angular/common';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ProductFacade } from '@app/features/product/facade/product.facade';
import { TaxProfileTemplateFacade } from '@app/core/tax-profile-template/facade/tax-profile-template.facade';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-tax-template-select-field',
  imports: [SelectFieldComponent, AsyncPipe],
  templateUrl: './tax-template-select-field.component.html',
  styleUrl: './tax-template-select-field.component.scss',
})
export class TaxTemplateSelectFieldComponent {
  parentForm = input.required<FormGroup<ProductFormControls>>();
  controlName = input.required<string>();

  private readonly destroyRef = inject(DestroyRef);
  private readonly taxProfileTemplateFacade = inject(TaxProfileTemplateFacade);
  private readonly productFacade = inject(ProductFacade);

  protected readonly templates$ = this.taxProfileTemplateFacade.templates$.pipe(
    map(template =>
      template.map(t => ({
        code: t.id,
        description: t.name,
      })),
    ),
  );
  protected readonly defaultTemplate$ = this.taxProfileTemplateFacade.templates$.pipe(
    map(template => template.find(t => t.isDefault)),
  );

  protected readonly selectedProduct$ = this.productFacade.selectedProduct$;

  ngOnInit() {
    const form = this.parentForm();
    combineLatest([this.selectedProduct$, this.defaultTemplate$])
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        filter(([product, template]) => !!template),
        tap(([product, template]) => {
          if (product === null) {
            form?.patchValue({
              taxProfileTemplateId: template?.id,
            });
          }
          if (product?.overrideTaxProfile === false) {
            form?.patchValue({
              taxProfileTemplateId: product?.taxProfileTemplateId,
            });
          }
        }),
      )
      .subscribe();
  }

  applyTemplate(event: any) {
    console.log('applyTemplate', event);
  }
}
