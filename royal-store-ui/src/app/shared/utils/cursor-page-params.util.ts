import { HttpParams } from '@angular/common/http';
import {
  CursorPageParams,
  DEFAULT_PAGE_SIZE,
} from '../interfaces/cursor-page.interface';

/** Adds the API's paging query parameters (`cursor`, `page-size`) to a list request. */
export function withCursorPageParams(
  params: HttpParams,
  page?: CursorPageParams,
): HttpParams {
  let result = params.set('page-size', page?.pageSize ?? DEFAULT_PAGE_SIZE);
  if (page?.cursor) {
    result = result.set('cursor', page.cursor);
  }
  return result;
}
