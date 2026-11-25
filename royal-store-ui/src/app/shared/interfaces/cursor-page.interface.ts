/** Default page size of every cursor-paged list (067); matches the API default. */
export const DEFAULT_PAGE_SIZE = 30;

/** Paging part of a list request: `cursor` is the previous page's `nextCursor` (absent → first page). */
export interface CursorPageParams {
  cursor?: string;
  pageSize?: number;
}

/** One page of a cursor-paged list, as every paged API endpoint returns it (067). */
export interface CursorPage<T> {
  items: T[];
  /** Opaque: only echoed back as `cursor`. `null` exactly when `hasMore` is false. */
  nextCursor: string | null;
  hasMore: boolean;
}
