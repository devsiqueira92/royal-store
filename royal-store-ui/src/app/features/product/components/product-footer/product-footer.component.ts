import { Component, inject, input } from '@angular/core';
import { FormArray, FormGroup } from '@angular/forms';
import {
  Product,
  ProductFormControls,
  ProductVariantFormControls,
} from '../../interfaces/product.interface';
import { ButtonComponent } from '@app/shared/components/button/button.component';
import {
  UpdateProductRequest,
  UpdateProductVariantRequest,
} from '../../interfaces/update-product-request.interface';
import { ProductFacade } from '../../facade/product.facade';
import { toSignal } from '@angular/core/rxjs-interop';
import { DetailsSidebarService } from '@app/shared/services/details-sidebar.service';
import {
  CreateProductRequest,
  CreateProductVariantRequest,
} from '../../interfaces/create-product-request.interface';
import { CurrencyPipe } from '@angular/common';
import { TranslatePipe } from '@ngx-translate/core';

@Component({
  selector: 'app-product-footer',
  imports: [ButtonComponent, CurrencyPipe, TranslatePipe],
  templateUrl: './product-footer.component.html',
  styleUrl: './product-footer.component.scss',
})
export class ProductFooterComponent {
  itemType = input.required<number>();
  formGroup = input.required<FormGroup<ProductFormControls>>();
  variants = input.required<FormArray<FormGroup<ProductVariantFormControls>>>();

  private readonly productFacade = inject(ProductFacade);
  private readonly detailsSidebarService = inject(DetailsSidebarService);

  protected readonly selectedProduct = toSignal(
    this.productFacade.selectedProduct$,
    { initialValue: null as Product | null },
  );

  protected getSummary(): {
    variantsCount: number;
    totalUnits: number;
    minPrice: number | null;
    maxPrice: number | null;
  } {
    const rows = this.variants().controls.map(group => group.getRawValue());
    const totalUnits = rows.reduce(
      (total, row) => total + (Number(row.stockQuantity) ?? 0),
      0,
    );
    const prices = rows.map(row => row.price ?? 0);

    return {
      variantsCount: rows.length,
      totalUnits,
      minPrice: prices.length ? Math.min(...prices) : null,
      maxPrice: prices.length ? Math.max(...prices) : null,
    };
  }

  protected onClose(): void {
    this.productFacade.clearSelectedProduct();
    this.detailsSidebarService.closeDrawer();
  }

  protected onSubmit(event: Event): void {
    event.preventDefault();

    if (this.formGroup().invalid || this.variants().length === 0) {
      this.formGroup().markAllAsTouched();
      return;
    }

    const raw = this.formGroup().getRawValue();
    const isService = raw.itemType === 1;

    const baseRequest = {
      itemType: raw.itemType,
      categoryId: raw.categoryId ?? '',
      name: (raw.name ?? '').trim(),
      referenceCode: (raw.sku ?? '').trim(),
      description: raw.description?.trim() || undefined,
      unitOfMeasure: raw.unitOfMeasure?.trim() || 'UN',
      ncm: isService ? undefined : raw.ncm?.trim() || undefined,
      cest: isService ? undefined : raw.cest?.trim() || undefined,
      origin: isService ? 0 : Number(raw.origin ?? 0),
      taxProfileTemplate: raw.taxProfileTemplate ?? undefined,
      nbs: isService ? raw.nbs?.trim() || undefined : undefined,
      issCode: isService ? raw.issCode?.trim() || undefined : undefined,
      overrideTaxProfile: raw.overrideTaxProfile,
      defaultCfop: raw.defaultCfop?.trim() || undefined,
      icmsCst: raw.icmsCst?.trim() || undefined,
      pisCofinsCst: raw.pisCofinsCst?.trim() || undefined,
      ivaCst: raw.ivaCst?.trim() || undefined,
      isSelectiveTax: raw.isSelectiveTax,
    };

    const product = this.selectedProduct();

    if (product) {
      const { variantsToAdd, variantsToUpdate } =
        this.buildVariantChanges(product);

      const request: UpdateProductRequest = {
        ...baseRequest,
        version: product.version,
        variantsToAdd,
        variantsToUpdate,
      };
      this.productFacade.saveProductEdit(product.id, request);
    } else {
      const variants = this.variants().controls.map(group =>
        this.toVariantRequest(group),
      );

      const request: CreateProductRequest = {
        ...baseRequest,
        variants,
      };
      this.productFacade.createProduct(request);
    }

    this.productFacade.clearSelectedProduct();
    this.detailsSidebarService.closeDrawer();
  }

  private buildVariantChanges(product: Product): {
    variantsToAdd: CreateProductVariantRequest[];
    variantsToUpdate: UpdateProductVariantRequest[];
  } {
    const originalById = new Map(product.variants.map(v => [v.id, v]));

    const variantsToUpdate: UpdateProductVariantRequest[] = [];
    const variantsToAdd: CreateProductVariantRequest[] = [];

    for (const group of this.variants().controls) {
      const variantId = group.getRawValue().id;
      const original = variantId ? originalById.get(variantId) : undefined;
      const request = this.toVariantRequest(group);

      if (original) {
        variantsToUpdate.push({ variantId: original.id, ...request });
      } else {
        variantsToAdd.push(request);
      }
    }

    return { variantsToAdd, variantsToUpdate };
  }

  private toVariantRequest(
    group: FormGroup<ProductVariantFormControls>,
  ): CreateProductVariantRequest {
    const variant = group.getRawValue();

    return {
      size: variant.size?.trim() || undefined,
      color: variant.color?.trim() || undefined,
      sku: variant.sku?.trim() || undefined,
      barcode: variant.barcode?.trim() || undefined,
      price: variant.price ?? 0,
      costPrice: variant.costPrice ?? undefined,
      stockQuantity: variant.stockQuantity ?? 0,
      minStockQuantity: variant.minStockQuantity ?? undefined,
    };
  }
}
