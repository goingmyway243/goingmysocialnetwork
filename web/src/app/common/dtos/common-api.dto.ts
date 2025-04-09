export interface IPagedRequest {
  pageIndex?: number,
  pageSize: number,
  cursorTimestamp?: Date
}

export interface IPagedResponse<T> {
  items: T[],
  pageIndex: number,
  totalCount: number,
}