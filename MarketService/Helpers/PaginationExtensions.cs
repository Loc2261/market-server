using Microsoft.EntityFrameworkCore;
using MarketService.DTOs;

namespace MarketService.Helpers
{
    public static class PaginationExtensions
    {
        public static async Task<PagedResult<TResponse>> ToPagedResultAsync<TEntity, TResponse>(
            this IQueryable<TEntity> query,
            int page,
            int pageSize,
            Func<TEntity, TResponse> mapFn)
        {
            var totalItems = await query.CountAsync();
            
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<TResponse>
            {
                Items = items.Select(mapFn).ToList(),
                TotalItems = totalItems,
                Page = page,
                PageSize = pageSize
            };
        }
    }
}
