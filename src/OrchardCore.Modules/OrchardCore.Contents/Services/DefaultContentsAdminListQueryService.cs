using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.Contents.ViewModels;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Modules;
using YesSql;

namespace OrchardCore.Contents.Services
{
    public class DefaultContentsAdminListQueryService : IContentsAdminListQueryService
    {
        private readonly ISession _session;
        private readonly IServiceProvider _serviceProvider;
        private readonly IEnumerable<IContentsAdminListFilter> _contentsAdminListFilters;
        private readonly ILogger _logger;

        public DefaultContentsAdminListQueryService(
            ISession session,
            IServiceProvider serviceProvider,
            IEnumerable<IContentsAdminListFilter> contentsAdminListFilters,
            ILogger<DefaultContentsAdminListQueryService> logger)
        {
            _session = session;
            _serviceProvider = serviceProvider;
            _contentsAdminListFilters = contentsAdminListFilters;
            _logger = logger;
        }

        public Task<IQuery<ContentItem>> QueryAsync(ContentOptionsViewModel model, IUpdateModel updater)
        {
            // Because admin filters can add a different index to the query this must be added as a Query<ContentItem>()
            var query = _session.Query<ContentItem>();

            var result = model.FilterResult.ExecuteAsync(query, _serviceProvider).AsTask();

            return result;


            // return Task.FromResult<IQuery<ContentItem>>(result);

            // await _contentsAdminListFilters.InvokeAsync((filter, model, query, updater) => filter.FilterAsync(model, query, updater), model, query, updater, _logger);

            // return Task.FromResult<IQuery<ContentItem>>(query);
        }
    }
}
