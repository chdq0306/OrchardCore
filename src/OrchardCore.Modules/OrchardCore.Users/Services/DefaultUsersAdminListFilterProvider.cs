using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Users.Models;
using YesSql.Filters.Query;
using YesSql;
using YesSql.Services;
using OrchardCore.Users.ViewModels;
using OrchardCore.Users.Indexes;
using Microsoft.AspNetCore.Identity;

namespace OrchardCore.Users.Services
{
    public class DefaultUsersAdminListFilterProvider : IUsersAdminListFilterProvider
    {
        public void Build(QueryEngineBuilder<User> builder)
        {
            builder
                .WithNamedTerm("filter", builder => builder
                    .OneCondition<User>((val, query) =>
                    {
                        if (Enum.TryParse<UsersFilter>(val, true, out var usersStatus))
                        {
                            switch (usersStatus)
                            {
                                case UsersFilter.Enabled:
                                    query.With<UserIndex>(u => u.IsEnabled);
                                    break;
                                case UsersFilter.Disabled:
                                    query.With<UserIndex>(u => !u.IsEnabled);
                                    break;
                            }
                        }

                        return query;
                    })
                    .MapTo<UserIndexOptions>((val, model) =>
                    {
                        if (Enum.TryParse<UsersFilter>(val, true, out var usersFilter))
                        {
                            model.Filter = usersFilter;
                        }
                    })
                    .MapFrom<UserIndexOptions>((model) =>
                    {
                        switch (model.Filter)
                        {
                            case UsersFilter.Enabled:
                                return (true, model.Filter.ToString());
                            case UsersFilter.Disabled:
                                return (true, model.Filter.ToString());
                        }

                        return (false, String.Empty);
                    })
                )
                .WithNamedTerm("sort", builder => builder
                    .OneCondition<User>((val, query) =>
                    {
                        if (Enum.TryParse<UsersOrder>(val, true, out var usersOrder))
                        {
                            switch (usersOrder)
                            {
                                // Name is provided as a default sort.
                                case UsersOrder.Email:
                                    query.With<UserIndex>().OrderBy(u => u.NormalizedEmail);
                                    break;
                            };
                        }

                        return query;
                    })
                    .MapTo<UserIndexOptions>((val, model) =>
                    {
                        if (Enum.TryParse<UsersOrder>(val, true, out var order))
                        {
                            model.Order = order;
                        }
                    })
                    .MapFrom<UserIndexOptions>((model) =>
                    {
                        if (model.Order != UsersOrder.Name)
                        {
                            return (true, model.Order.ToString());
                        }

                        return (false, String.Empty);
                    })
                )
                .WithNamedTerm("role", builder => builder
                    .OneCondition<User>((val, query, ctx) =>
                    {
                        var context = (UserQueryContext)ctx;
                        var userManager = context.ServiceProvider.GetRequiredService<UserManager<IUser>>();
                        var normalizedRoleName = userManager.NormalizeName(val);
                        query.With<UserByRoleNameIndex>(x => x.RoleName == normalizedRoleName);

                        return new ValueTask<IQuery<User>>(query);
                    })
                    .MapTo<UserIndexOptions>((val, model) => model.SelectedRole = val)
                    .MapFrom<UserIndexOptions>((model) => (!String.IsNullOrEmpty(model.SelectedRole), model.SelectedRole))
                )
                .WithDefaultTerm("name", builder => builder
                    .ManyCondition<User>(
                        ((val, query, ctx) =>
                        {
                            var context = (UserQueryContext)ctx;
                            var userManager = context.ServiceProvider.GetRequiredService<UserManager<IUser>>();
                            query.With<UserIndex>(x => x.NormalizedUserName.Contains(val));

                            return new ValueTask<IQuery<User>>(query);
                        }),
                        ((val, query, ctx) =>
                        {
                            var context = (UserQueryContext)ctx;
                            var userManager = context.ServiceProvider.GetRequiredService<UserManager<IUser>>();
                            query.With<UserIndex>(x => x.NormalizedUserName.IsNotIn<UserIndex>(s => s.NormalizedUserName, w => w.NormalizedUserName.Contains(val)));

                            return new ValueTask<IQuery<User>>(query);
                        })
                    )
                )
                .WithNamedTerm("email", builder => builder
                    .ManyCondition<User>(
                        ((val, query, ctx) =>
                        {
                            var context = (UserQueryContext)ctx;
                            var userManager = context.ServiceProvider.GetRequiredService<UserManager<IUser>>();
                            query.With<UserIndex>(x => x.NormalizedEmail.Contains(val));

                            return new ValueTask<IQuery<User>>(query);
                        }),
                        ((val, query, ctx) =>
                        {
                            var context = (UserQueryContext)ctx;
                            var userManager = context.ServiceProvider.GetRequiredService<UserManager<IUser>>();
                            query.With<UserIndex>(x => x.NormalizedEmail.IsNotIn<UserIndex>(s => s.NormalizedEmail, w => w.NormalizedEmail.Contains(val)));

                            return new ValueTask<IQuery<User>>(query);
                        })
                    )
                );

        }
    }
}