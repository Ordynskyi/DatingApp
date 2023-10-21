namespace API.Helpers;

public struct PaginationHeader
{
    public PaginationHeader(int currentPage, int itemsPerPage,
         int totalItems, int totalPages)
    {
        CurrentPage = currentPage;
        ItemsPerPage = itemsPerPage;
        TotalItems = totalItems;
        TotalPages = totalPages;
    }

    public int CurrentPage { get; }
    public int ItemsPerPage { get; }
    public int TotalItems { get; }
    public int TotalPages { get; }
}
