export interface IPagedRequest {
  pageIndex: number,
  pageSize: number
}

export interface IPagedResponse<T> {
  items: T[],
  pageIndex: number,
  totalCount: number,
}