using FBC.DBRepository;
using FBC.Devices.API.Data.Repositories;
using FBC.Mediator;

namespace FBC.Devices.API.Features.User;

public sealed class UserGetAll
{
    public record Query(int PageNumber = 0, int ItemsPerPage = 25) : IRequest<PaginateResponseModel<UserDto>>;

    public record UserDto(int Id, string UserName, string Name, bool IsSysAdmin, string[] Roles);

    internal sealed class Handler(UserRepository repo)
        : IRequestHandler<Query, PaginateResponseModel<UserDto>>
    {
        public async Task<PaginateResponseModel<UserDto>> Handle(Query request, CancellationToken token = default)
        {
            var result = await repo.GetListAsync(
                orderBy: q => q.OrderBy(x => x.UserName),
                pageNumber: request.PageNumber,
                itemsPerPage: request.ItemsPerPage,
                enableTracking: false,
                cancellationToken: token);

            return new PaginateResponseModel<UserDto>
            {
                Items = result.Items.Select(u => new UserDto(u.Id, u.UserName, u.Name, u.IsSysAdmin, u.GetRoles())).ToList(),
                TotalFilteredCount = result.TotalFilteredCount,
                TotalPages = result.TotalPages,
                PageIndex = result.PageIndex,
                ItemsPerPage = result.ItemsPerPage
            };
        }
    }
}
