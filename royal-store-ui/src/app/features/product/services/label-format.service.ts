import { Injectable } from '@angular/core';
import { combineLatest, map, Observable, of } from 'rxjs';
import {
  LabelFormat,
  LabelTemplate,
  LabelTemplateConfiguration,
  LabelType,
} from '../interfaces/label-format.interface';
import {
  MOCK_LABEL_FORMATS,
  MOCK_LABEL_TEMPLATES,
} from './label-format.mock';

const EMPTY_CONFIGURATION: LabelTemplateConfiguration = {
  version: 1,
  elements: [],
};

const parseConfiguration = (raw: string): LabelTemplateConfiguration => {
  try {
    const parsed = JSON.parse(raw) as Partial<LabelTemplateConfiguration>;
    return {
      version: parsed.version ?? 1,
      elements: Array.isArray(parsed.elements) ? parsed.elements : [],
    };
  } catch {
    return EMPTY_CONFIGURATION;
  }
};

@Injectable({ providedIn: 'root' })
export class LabelFormatService {
  // TODO: replace with HttpClient calls once the label format/template endpoints exist.
  getFormats(): Observable<LabelFormat[]> {
    return of(MOCK_LABEL_FORMATS);
  }

  getTemplates(): Observable<LabelTemplate[]> {
    return of(MOCK_LABEL_TEMPLATES);
  }

  /** Active templates joined with their format; templates whose format is missing are dropped. */
  getLabelTypes(): Observable<LabelType[]> {
    return combineLatest([this.getTemplates(), this.getFormats()]).pipe(
      map(([templates, formats]) =>
        templates
          .filter(t => t.isActive)
          .flatMap(t => {
            const format = formats.find(f => f.id === t.labelFormatId);
            return format
              ? [
                  {
                    id: t.id,
                    name: t.name,
                    isDefault: t.isDefault,
                    format,
                    configuration: parseConfiguration(t.configuration),
                  },
                ]
              : [];
          }),
      ),
    );
  }
}
