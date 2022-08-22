export interface PaginationResult<T> {
  page: number;
  pageSize: number;
  totalCount: number;
  data: T[];
}
