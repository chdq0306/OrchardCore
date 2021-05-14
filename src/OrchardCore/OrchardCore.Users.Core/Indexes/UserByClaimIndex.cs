using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Users.Models;
using YesSql.Indexes;

namespace OrchardCore.Users.Indexes
{
    public class UserByClaimIndex : MapIndex
    {
        public string ClaimType { get; set; }

        public string ClaimValue { get; set; }
    }

    public class UserByClaimIndexProvider : IndexProvider<User>
    {
        public UserByClaimIndexProvider(IOptions<UserOptions> userOptions)
        {
            CollectionName = userOptions.Value.UserCollection;
        }
        public override void Describe(DescribeContext<User> context)
        {
            context.For<UserByClaimIndex>()
                .Map(user => user.UserClaims.Select(x => new UserByClaimIndex
                {
                    ClaimType = x.ClaimType,
                    ClaimValue = x.ClaimValue,
                }));
        }
    }
}
