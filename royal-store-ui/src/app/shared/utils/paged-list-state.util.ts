import { CursorPage } from '../interfaces/cursor-page.interface';

/**
 * Store slice of an infinite-scroll list (067), reusable by every feature: the first page replaces
 * the items, next pages append, and a restart (new filters) drops the cursor so no page of the
 * previous filters can be appended.
 */
export interface PagedListState<T, P> {
  items: T[];
  /** Filters of the list being scrolled; next pages reuse them. */
  params: P | undefined;
  nextCursor: string | null;
  hasMore: boolean;
  /** First page in flight (list (re)started). */
  isLoading: boolean;
  /** Next page in flight. */
  isLoadingMore: boolean;
  /** The last next-page request failed; cleared by a retry or a restart. */
  loadMoreFailed: boolean;
}

export function initialPagedListState<T, P>(): PagedListState<T, P> {
  return {
    items: [],
    params: undefined,
    nextCursor: null,
    hasMore: false,
    isLoading: false,
    isLoadingMore: false,
    loadMoreFailed: false,
  };
}

/** Whether a next page may be requested now: there is one, and nothing else is loading. */
export function canLoadNextPage<T, P>(state: PagedListState<T, P>): boolean {
  return state.nextCursor !== null && !state.isLoading && !state.isLoadingMore;
}

/** Pure transitions for the reducer's `on(...)` handlers. */
export const pagedList = {
  firstPageRequested<T, P>(state: PagedListState<T, P>, params: P | undefined): PagedListState<T, P> {
    return {
      ...state,
      params,
      nextCursor: null,
      hasMore: false,
      isLoading: true,
      isLoadingMore: false,
      loadMoreFailed: false,
    };
  },

  nextPageRequested<T, P>(state: PagedListState<T, P>): PagedListState<T, P> {
    return canLoadNextPage(state)
      ? { ...state, isLoadingMore: true, loadMoreFailed: false }
      : state;
  },

  pageLoaded<T, P>(
    state: PagedListState<T, P>,
    page: CursorPage<T>,
    append: boolean,
  ): PagedListState<T, P> {
    return {
      ...state,
      items: append ? [...state.items, ...page.items] : page.items,
      nextCursor: page.nextCursor,
      hasMore: page.hasMore,
      isLoading: false,
      isLoadingMore: false,
      loadMoreFailed: false,
    };
  },

  /** A failed next page keeps what is shown (067 FR-017); a failed first page empties the list. */
  pageFailed<T, P>(state: PagedListState<T, P>, append: boolean): PagedListState<T, P> {
    return append
      ? { ...state, isLoadingMore: false, loadMoreFailed: true }
      : { ...state, items: [], nextCursor: null, hasMore: false, isLoading: false };
  },
};
