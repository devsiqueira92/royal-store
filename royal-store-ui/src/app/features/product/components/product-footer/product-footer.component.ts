import { Component, inject, input } from '@angular/core';
import { FormArray, FormGroup } from '@angular/forms';
import {
  Product,
  ProductFormControls,
  ProductVariantFormControls,
} from '../../interfaces/product.interface';
import { ButtonComponent } from '@app/shared/components/button/button.component';
import {
  CreateProductRequest,
  CreateProductVariantRequest,
} from '../../interfaces/create-product-request.interface';
import {
  UpdateProductRequest,
  UpdateProductVariantRequest,
} from '../../interfaces/update-product-request.interface';
import {
  CreateServiceRequest,
  CreateServiceVariantRequest,
} from '../../interfaces/create-service-request.interface';
import {
  UpdateServiceRequest,
  UpdateServiceVariantRequest,
} from '../../interfaces/update-service-request.interface';
import { ProductFacade } from '../../facade/product.facade';
import { ServiceFacade } from '../../facade/service.facade';
import { toSignal } from '@angular/core/rxjs-interop';
import { DetailsSidebarService } from '@app/shared/services/details-sidebar.service';
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
  private readonly serviceFacade = inject(ServiceFacade);
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
      (total, row) => total + (Number(row.stockQuantity) || 0),
      0,
    );
    const prices = rows
      .map(row => row.price)
      .filter((price): price is number => price != null);

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

    const isService = this.itemType() === 1;
    const product = this.selectedProduct();

    if (isService) {
      this.submitService(product);
    } else {
      this.submitProduct(product);
    }

    this.productFacade.clearSelectedProduct();
    this.detailsSidebarService.closeDrawer();
  }

  private submitProduct(product: Product | null): void {
    const raw = this.formGroup().getRawValue();

    const baseRequest = {
      categoryId: raw.categoryId ?? '',
      name: (raw.name ?? '').trim(),
      sku: (raw.sku ?? '').trim(),
      stockTrack: !!raw.stockTrack,
      basePrice: raw.basePrice ?? 0,
      baseCostPrice: raw.baseCostPrice ?? 0,
      description: raw.description?.trim() || undefined,
      unitOfMeasure: raw.unitOfMeasure?.trim() || 'UN',
      ncm: raw.ncm?.trim() || undefined,
      cest: raw.cest?.trim() || undefined,
      origin: Number(raw.origin ?? 0),
      measure: {
        netWeight: raw.netWeight ?? undefined,
        grossWeight: raw.grossWeight ?? undefined,
        width: raw.width ?? undefined,
        height: raw.height ?? undefined,
        length: raw.length ?? undefined,
      },
      taxProfileTemplateId: raw.taxProfileTemplateId ?? '',
      overrideTaxProfile: raw.overrideTaxProfile,
      defaultCfop: raw.defaultCfop?.trim() || undefined,
      icmsCst: raw.icmsCst?.trim() || undefined,
      pisCofinsCst: raw.pisCofinsCst?.trim() || undefined,
      ivaCst: raw.ivaCst?.trim() || undefined,
      isSelectiveTax: raw.isSelectiveTax,
    };

    if (product) {
      const { variantsToAdd, variantsToUpdate } =
        this.buildProductVariantChanges(product);

      const request: UpdateProductRequest = {
        ...baseRequest,
        version: product.version,
        variantsToAdd,
        variantsToUpdate,
      };
      this.productFacade.saveProductEdit(product.id, request);
    } else {
      const variants = this.variants().controls.map(group =>
        this.toProductVariantRequest(group),
      );

      const request: CreateProductRequest = { ...baseRequest, variants };
      this.productFacade.createProduct(request);
    }
  }

  private submitService(product: Product | null): void {
    const raw = this.formGroup().getRawValue();

    const baseRequest = {
      categoryId: raw.categoryId ?? '',
      name: (raw.name ?? '').trim(),
      sku: (raw.sku ?? '').trim(),
      basePrice: raw.basePrice ?? 0,
      baseCostPrice: raw.baseCostPrice ?? 0,
      description: raw.description?.trim() || undefined,
      unitOfMeasure: raw.unitOfMeasure?.trim() || 'UN',
      nbs: raw.nbs?.trim() || '',
      issCode: raw.issCode?.trim() || '',
      taxProfileTemplateId: raw.taxProfileTemplateId ?? '',
      overrideTaxProfile: raw.overrideTaxProfile,
      pisCofinsCst: raw.pisCofinsCst?.trim() || undefined,
      ivaCst: raw.ivaCst?.trim() || undefined,
      isSelectiveTax: raw.isSelectiveTax,
    };

    if (product) {
      const { variantsToAdd, variantsToUpdate } =
        this.buildServiceVariantChanges(product);

      const request: UpdateServiceRequest = {
        ...baseRequest,
        version: product.version,
        variantsToAdd,
        variantsToUpdate,
      };
      this.serviceFacade.saveServiceEdit(product.id, request);
    } else {
      const variants = this.variants().controls.map(group =>
        this.toServiceVariantRequest(group),
      );

      const request: CreateServiceRequest = { ...baseRequest, variants };
      this.serviceFacade.createService(request);
    }
  }

  private buildProductVariantChanges(product: Product): {
    variantsToAdd: CreateProductVariantRequest[];
    variantsToUpdate: UpdateProductVariantRequest[];
  } {
    const originalById = new Map(product.variants.map(v => [v.id, v]));

    const variantsToUpdate: UpdateProductVariantRequest[] = [];
    const variantsToAdd: CreateProductVariantRequest[] = [];

    for (const group of this.variants().controls) {
      const variantId = group.getRawValue().id;
      const original = variantId ? originalById.get(variantId) : undefined;
      const request = this.toProductVariantRequest(group);

      if (original) {
        variantsToUpdate.push({ variantId: original.id, ...request });
      } else {
        variantsToAdd.push(request);
      }
    }

    return { variantsToAdd, variantsToUpdate };
  }

  private buildServiceVariantChanges(product: Product): {
    variantsToAdd: CreateServiceVariantRequest[];
    variantsToUpdate: UpdateServiceVariantRequest[];
  } {
    const originalById = new Map(product.variants.map(v => [v.id, v]));

    const variantsToUpdate: UpdateServiceVariantRequest[] = [];
    const variantsToAdd: CreateServiceVariantRequest[] = [];

    for (const group of this.variants().controls) {
      const variantId = group.getRawValue().id;
      const original = variantId ? originalById.get(variantId) : undefined;
      const request = this.toServiceVariantRequest(group);

      if (original) {
        variantsToUpdate.push({ variantId: original.id, ...request });
      } else {
        variantsToAdd.push(request);
      }
    }

    return { variantsToAdd, variantsToUpdate };
  }

  private toProductVariantRequest(
    group: FormGroup<ProductVariantFormControls>,
  ): CreateProductVariantRequest {
    const variant = group.getRawValue();

    return {
      size: variant.size?.trim() || undefined,
      color: variant.color?.trim() || undefined,
      sku: variant.sku?.trim() || undefined,
      barcode: variant.barcode?.trim() || undefined,
      price: variant.price ?? undefined,
      costPrice: variant.costPrice ?? undefined,
      stockQuantity: variant.stockQuantity ?? undefined,
      minStockQuantity: variant.minStockQuantity ?? undefined,
    };
  }

  private toServiceVariantRequest(
    group: FormGroup<ProductVariantFormControls>,
  ): CreateServiceVariantRequest {
    const variant = group.getRawValue();

    return {
      optionName: (variant.optionName ?? '').trim(),
      price: variant.price ?? undefined,
      costPrice: variant.costPrice ?? undefined,
    };
  }
}
