using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Data.Migration;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Users.Indexes;
using OrchardCore.Users.Models;
using YesSql;
using YesSql.Sql;

namespace OrchardCore.Users
{
    public class Migrations : DataMigration
    {
        private readonly ISession _session;
        private readonly string _userCollection;

        public Migrations(ISession session,IOptions<UserOptions> userOptions)
        {
            _session = session;
            _userCollection = userOptions.Value.UserCollection;
        }

        // This is a sequenced migration. On a new schemas this is complete after UpdateFrom2.
        public int Create()
        {
            SchemaBuilder.CreateMapIndexTable<UserIndex>(table => table
                .Column<string>("NormalizedUserName") // TODO These should have defaults. on SQL Server they will fall at 255. Exceptions are currently thrown if you go over that.
                .Column<string>("NormalizedEmail")
                .Column<bool>("IsEnabled", c => c.NotNull().WithDefault(true))
                .Column<bool>("IsLockoutEnabled", c => c.NotNull().WithDefault(false))
                .Column<DateTime?>("LockoutEndUtc", c => c.Nullable())
                .Column<int>("AccessFailedCount", c => c.NotNull().WithDefault(0))
                .Column<string>("UserId"),_userCollection
            );

            SchemaBuilder.AlterIndexTable<UserIndex>(table => table
                .CreateIndex("IDX_UserIndex_DocumentId",
                    "DocumentId",
                    "UserId",
                    "NormalizedUserName",
                    "NormalizedEmail",
                    "IsEnabled"
                    ),_userCollection
            );

            SchemaBuilder.AlterIndexTable<UserIndex>(table => table
                .CreateIndex("IDX_UserIndex_Lockout",
                    "DocumentId",
                    "IsLockoutEnabled",
                    "LockoutEndUtc",
                    "AccessFailedCount"),_userCollection
            );

            SchemaBuilder.CreateReduceIndexTable<UserByRoleNameIndex>(table => table
               .Column<string>("RoleName")
               .Column<int>("Count"),_userCollection
            );

            SchemaBuilder.AlterIndexTable<UserByRoleNameIndex>(table => table
                .CreateIndex("IDX_UserByRoleNameIndex_RoleName",
                    "RoleName"),_userCollection
            );

            SchemaBuilder.CreateMapIndexTable<UserByLoginInfoIndex>(table => table
                .Column<string>("LoginProvider")
                .Column<string>("ProviderKey"),_userCollection
                );

            SchemaBuilder.AlterIndexTable<UserByLoginInfoIndex>(table => table
                .CreateIndex("IDX_UserByLoginInfoIndex_DocumentId",
                    "DocumentId",
                    "LoginProvider",
                    "ProviderKey"),_userCollection
            );

            SchemaBuilder.CreateMapIndexTable<UserByClaimIndex>(table => table
               .Column<string>(nameof(UserByClaimIndex.ClaimType))
               .Column<string>(nameof(UserByClaimIndex.ClaimValue)),
                _userCollection);

            SchemaBuilder.AlterIndexTable<UserByClaimIndex>(table => table
                .CreateIndex("IDX_UserByClaimIndex_DocumentId",
                    "DocumentId",
                    nameof(UserByClaimIndex.ClaimType),
                    nameof(UserByClaimIndex.ClaimValue)),_userCollection
            );

            // Shortcut other migration steps on new content definition schemas.
            return 12;
        }

        // This code can be removed in a later version.
        public int UpdateFrom1()
        {
            SchemaBuilder.CreateMapIndexTable<UserByLoginInfoIndex>(table => table
                .Column<string>("LoginProvider")
                .Column<string>("ProviderKey"),_userCollection
                );

            return 2;
        }

        // This code can be removed in a later version.
        public int UpdateFrom2()
        {
            SchemaBuilder.CreateMapIndexTable<UserByClaimIndex>(table => table
               .Column<string>(nameof(UserByClaimIndex.ClaimType))
               .Column<string>(nameof(UserByClaimIndex.ClaimValue)),
                _userCollection);

            return 3;
        }

        // This code can be removed in a later version.
        public int UpdateFrom3()
        {
            SchemaBuilder.AlterIndexTable<UserIndex>(table => table
                .AddColumn<bool>(nameof(UserIndex.IsEnabled), c => c.NotNull().WithDefault(true)),_userCollection
                );

            return 4;
        }

        // UserId database migration.
        // This code can be removed in a later version.
        public int UpdateFrom4()
        {
            SchemaBuilder.AlterIndexTable<UserIndex>(table => table
                .AddColumn<string>("UserId"),_userCollection
                );

            return 5;
        }

        // UserId column is added. This initializes the UserId property to the UserName for existing users.
        // The UserName property rather than the NormalizedUserName is used as the ContentItem.Owner property matches the UserName.
        // New users will be created with a generated Id.
        // This code can be removed in a later version.
        public int UpdateFrom5()
        {
            // Defer this until after the subsequent migrations have succeded as the schema has changed.
            ShellScope.AddDeferredTask(async scope =>
            {
                var session = scope.ServiceProvider.GetRequiredService<ISession>();
                var users = await session.Query<User>(_userCollection).ListAsync();
                foreach (var user in users)
                {
                    user.UserId = user.UserName;
                    session.Save(user,_userCollection);
                }
            });

            return 6;
        }

        public async Task<int> UpdateFrom5Async()
        {
            var users = await _session.Query<User>(_userCollection).ListAsync();
            foreach (var user in users)
            {
                user.UserId = user.UserName;
                _session.Save(user, _userCollection);
            }

            return 6;
        }

        // This buggy migration has been removed.
        // This code can be removed in a later version.
        public int UpdateFrom6()
        {
            return 7;
        }

        // Migrate any user names replacing '@' with '+' as user names can no longer be an email address.
        // This code can be removed in a later version.
        public int UpdateFrom7()
        {
            // Defer this until after the subsequent migrations have succeded as the schema has changed.
            ShellScope.AddDeferredTask(async scope =>
            {
                var session = scope.ServiceProvider.GetRequiredService<ISession>();
                var users = await session.Query<User, UserIndex>(u => u.NormalizedUserName.Contains("@"),_userCollection).ListAsync();
                foreach (var user in users)
                {
                    user.UserName = user.UserName.Replace('@', '+');
                    user.NormalizedUserName = user.NormalizedUserName.Replace('@', '+');
                    session.Save(user,_userCollection);
                }
            });

            return 8;
        }
        public async Task<int> UpdateFrom7Async()
        {
            var users = await _session.Query<User, UserIndex>(u => u.NormalizedUserName.Contains("@"), _userCollection).ListAsync();
            foreach (var user in users)
            {
                user.UserName = user.UserName.Replace('@', '+');
                user.NormalizedUserName = user.NormalizedUserName.Replace('@', '+');
                _session.Save(user, _userCollection);
            }

            return 8;
        }

        // This code can be removed in a later version.
        public int UpdateFrom8()
        {
            SchemaBuilder.AlterIndexTable<UserIndex>(table => table
                .CreateIndex("IDX_UserIndex_DocumentId",
                    "DocumentId",
                    "UserId",
                    "NormalizedUserName",
                    "NormalizedEmail",
                    "IsEnabled",
                    "IsLockoutEnabled",
                    "LockoutEndUtc",
                    "AccessFailedCount"),_userCollection
            );

            SchemaBuilder.AlterIndexTable<UserByLoginInfoIndex>(table => table
                .CreateIndex("IDX_UserByLoginInfoIndex_DocumentId",
                    "DocumentId",
                    "LoginProvider",
                    "ProviderKey"),_userCollection
            );

            SchemaBuilder.AlterIndexTable<UserByClaimIndex>(table => table
                .CreateIndex("IDX_UserByClaimIndex_DocumentId",
                    "DocumentId",
                    nameof(UserByClaimIndex.ClaimType),
                    nameof(UserByClaimIndex.ClaimValue)),_userCollection
            );

            return 9;
        }

        // This code can be removed in a later version.
        public int UpdateFrom9()
        {
            SchemaBuilder.AlterIndexTable<UserByRoleNameIndex>(table => table
                .CreateIndex("IDX_UserByRoleNameIndex_RoleName",
                    "RoleName"),_userCollection
            );

            return 10;
        }

        public int UpdateFrom10()
        {
            SchemaBuilder.AlterIndexTable<UserIndex>(table => table
                .AddColumn<bool>(nameof(UserIndex.IsLockoutEnabled), c => c.NotNull().WithDefault(false)),_userCollection);

            SchemaBuilder.AlterIndexTable<UserIndex>(table => table
                .AddColumn<DateTime?>(nameof(UserIndex.LockoutEndUtc), c => c.Nullable()), _userCollection);

            SchemaBuilder.AlterIndexTable<UserIndex>(table => table
                .AddColumn<int>(nameof(UserIndex.AccessFailedCount), c => c.NotNull().WithDefault(0)), _userCollection);

            return 11;
        }

        public int UpdateFrom11()
        {
            SchemaBuilder.AlterIndexTable<UserIndex>(table => table
                .CreateIndex("IDX_UserIndex_Lockout",
                    "DocumentId",
                    "IsLockoutEnabled",
                    "LockoutEndUtc",
                    "AccessFailedCount"
                    ),_userCollection
            );

            return 12;
        }
    }
}
