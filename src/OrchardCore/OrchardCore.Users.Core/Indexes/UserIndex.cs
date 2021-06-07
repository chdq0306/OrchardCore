using OrchardCore.Users.Models;
using YesSql.Indexes;
using System;
using Microsoft.Extensions.Options;

namespace OrchardCore.Users.Indexes
{
    public class UserIndex : MapIndex
    {
        public int DocumentId { get; set; }
        public string UserId { get; set; }
        public string NormalizedUserName { get; set; }
        public string NormalizedEmail { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsLockoutEnabled { get; set; }
        public DateTime? LockoutEndUtc { get; set; }
        public int AccessFailedCount { get; set; }
    }

    public class UserIndexProvider : IndexProvider<User>
    {
        public UserIndexProvider(IOptions<UserOptions> userOptions)
        {
            CollectionName = userOptions.Value.UserCollection;
        }
        public override void Describe(DescribeContext<User> context)
        {
            context.For<UserIndex>()
                .Map(user =>
                {
                    return new UserIndex
                    {
                        UserId = user.UserId,
                        NormalizedUserName = user.NormalizedUserName,
                        NormalizedEmail = user.NormalizedEmail,
                        IsEnabled = user.IsEnabled,
                        IsLockoutEnabled = user.IsLockoutEnabled,
                        LockoutEndUtc = user.LockoutEndUtc,
                        AccessFailedCount = user.AccessFailedCount
                    };
                });
        }
    }
}
