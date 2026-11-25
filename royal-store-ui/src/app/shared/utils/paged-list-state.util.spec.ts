import { CursorPage } from '../interfaces/cursor-page.interface';
import {
  canLoadNextPage,
  initialPagedListState,
  pagedList,
  PagedListState,
} from './paged-list-state.util';

type Params = { term?: string };
const page = (items: number[], nextCursor: string | null): CursorPage<number> => ({
  items,
  nextCursor,
  hasMore: nextCursor !== null,
});

describe('pagedList', () => {
  let state: PagedListState<number, Params>;

  beforeEach(() => {
    state = initialPagedListState<number, Params>();
  });

  it('replaces items on the first page and appends next pages', () => {
    state = pagedList.firstPageRequested(state, { term: 'a' });
    state = pagedList.pageLoaded(state, page([1, 2], 'c1'), false);
    expect(state.items).toEqual([1, 2]);
    expect(canLoadNextPage(state)).toBe(true);

    state = pagedList.nextPageRequested(state);
    expect(state.isLoadingMore).toBe(true);
    state = pagedList.pageLoaded(state, page([3], null), true);
    expect(state.items).toEqual([1, 2, 3]);
    expect(state.hasMore).toBe(false);
    expect(canLoadNextPage(state)).toBe(false);
  });

  it('drops the cursor on restart so no page of the old filters can be requested', () => {
    state = pagedList.pageLoaded(state, page([1, 2], 'c1'), false);
    state = pagedList.firstPageRequested(state, { term: 'b' });
    expect(state.params).toEqual({ term: 'b' });
    expect(state.nextCursor).toBeNull();
    expect(pagedList.nextPageRequested(state)).toBe(state);
  });

  it('keeps shown items when a next page fails and allows a retry', () => {
    state = pagedList.pageLoaded(state, page([1, 2], 'c1'), false);
    state = pagedList.nextPageRequested(state);
    state = pagedList.pageFailed(state, true);
    expect(state.items).toEqual([1, 2]);
    expect(state.loadMoreFailed).toBe(true);

    state = pagedList.nextPageRequested(state);
    expect(state.isLoadingMore).toBe(true);
    expect(state.loadMoreFailed).toBe(false);
  });

  it('ignores a next page while the first page is loading', () => {
    state = pagedList.pageLoaded(state, page([1], 'c1'), false);
    state = { ...state, isLoading: true };
    expect(pagedList.nextPageRequested(state)).toBe(state);
  });
});
