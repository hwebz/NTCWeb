using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using EPiServer.Core;
using EPiServer.Filters;
using EPiServer.ServiceLocation;
using Lucene.Net.Search;
using Niteco.Common.Helpers;
using Niteco.Common.Search.Queries;
using Niteco.Common.Search.Queries.Lucene;
using Niteco.Search.Fields;
using Niteco.Search.Queries.Lucene;

namespace Niteco.Search
{
    /// <summary>
    /// Search helpers
    /// </summary>
    public class CmsSearchHandler
    {
        public static int Count<T>(string keywords, ContentReference root) where T : IContent
        {
            var groupQuery = new GroupQuery(LuceneOperator.AND);

            groupQuery.QueryExpressions.Add(new ContentQuery<T>());

            if (string.IsNullOrWhiteSpace(keywords))
            {
                var dateRangeQuery = new CreatedDateRangeQuery(DateTime.MinValue, DateTime.MaxValue, true);
                groupQuery.QueryExpressions.Add(dateRangeQuery);
            }
            else
            {
                var keywordGroupQuery = new GroupQuery(LuceneOperator.OR);
                keywordGroupQuery.QueryExpressions.Add(AddQueryExpression(keywords, Field.Title));
                keywordGroupQuery.QueryExpressions.Add(AddQueryExpression(keywords, Field.DisplayText));
                groupQuery.QueryExpressions.Add(keywordGroupQuery);
            }

            var searchHandler = ServiceLocator.Current.GetInstance<LuceneContentSearchHandler>();

            return searchHandler.GetSearchResults<T>(groupQuery, root, 1, int.MaxValue, null, true).TotalHits;
        }

        public static IList<T> SearchTags<T>(string tags, ContentReference root, int pageNumber, int pageSize, Collection<SortField> sortFields, out int totalItems) where T : IContent
        {
            var groupQuery = new GroupQuery(LuceneOperator.AND);

            groupQuery.QueryExpressions.Add(new ContentQuery<T>());

            var keywordGroupQuery = new GroupQuery(LuceneOperator.OR);
            keywordGroupQuery.QueryExpressions.Add(AddQueryExpression(tags, Field.DisplayText));
            groupQuery.QueryExpressions.Add(keywordGroupQuery);

            var searchHandler = ServiceLocator.Current.GetInstance<LuceneContentSearchHandler>();

            // Get search results
            var results = searchHandler.GetSearchResults<T>(groupQuery, root, pageNumber, pageSize, sortFields, true);

            totalItems = results.TotalHits;

            // Convert search results to pages
            var indexResponseItems = results.IndexResponseItems;

            return indexResponseItems.Select(v => searchHandler.GetContent<T>(v)).Where(v => v != null).ToList();
        }


        public static IList<T> Search<T>(string keywords, ContentReference root, int pageNumber, int pageSize, Collection<SortField> sortFields, out int totalItems) where T : IContent
        {
            var groupQuery = new GroupQuery(LuceneOperator.AND);

            groupQuery.QueryExpressions.Add(new ContentQuery<T>());

            if (string.IsNullOrWhiteSpace(keywords))
            {
                var dateRangeQuery = new CreatedDateRangeQuery(DateTime.MinValue, DateTime.MaxValue, true);
                groupQuery.QueryExpressions.Add(dateRangeQuery);
            }
            else
            {
                var keywordGroupQuery = new GroupQuery(LuceneOperator.OR);
                keywordGroupQuery.QueryExpressions.Add(AddQueryExpression(keywords, Field.Title));
                keywordGroupQuery.QueryExpressions.Add(AddQueryExpression(keywords, Field.DisplayText));
                keywordGroupQuery.QueryExpressions.Add(AddQueryExpression(keywords, Field.Default));
                groupQuery.QueryExpressions.Add(keywordGroupQuery);
            }

            var searchHandler = ServiceLocator.Current.GetInstance<LuceneContentSearchHandler>();

            // Get search results
            var results = searchHandler.GetSearchResults<T>(groupQuery, root, pageNumber, pageSize, sortFields, true);

            totalItems = results.TotalHits;

            // Convert search results to pages
            var indexResponseItems = results.IndexResponseItems;

            return indexResponseItems.Select(v => searchHandler.GetContent<T>(v)).Where(v => v != null).ToList();
        }

        public static IList<T> Search<T>(string keywords, IContent root, int pageNumber, int pageSize, Collection<SortField> sortFields, out int totalItems) where T : IContent
        {
            return Search<T>(keywords, root.ContentLink, pageNumber, pageSize, sortFields, out totalItems);
        }

        public static IList<T> SearchByCreatedDateRange<T>(DateTime startDate, DateTime endDate, bool inclusive, ContentReference root, int pageNumber, int pageSize, Collection<SortField> sortFields, out int totalItems) where T : IContent
        {
            var groupQuery = new GroupQuery(LuceneOperator.AND);

            var dateRangeQuery = new CreatedDateRangeQuery(startDate, endDate, inclusive);
            groupQuery.QueryExpressions.Add(dateRangeQuery);

            var searchHandler = ServiceLocator.Current.GetInstance<LuceneContentSearchHandler>();

            var results = searchHandler.GetSearchResults<T>(groupQuery, root, pageNumber, pageSize, sortFields, true);

            totalItems = results.TotalHits;

            return results.IndexResponseItems.Select(v => searchHandler.GetContent<T>(v)).ToList();
        }

        //public static int CountEvent<T>(string tag, ContentReference root) where T : IContent
        //{
        //    var groupQuery = new GroupQuery(LuceneOperator.AND);

        //    groupQuery.QueryExpressions.Add(new ContentQuery<T>());
        //    groupQuery.QueryExpressions.Add(new EventTagQuery(tag));

        //    //if (string.IsNullOrWhiteSpace(keywords))
        //    //{
        //    //    var dateRangeQuery = new CreatedDateRangeQuery(DateTime.MinValue, DateTime.MaxValue, true);
        //    //    groupQuery.QueryExpressions.Add(dateRangeQuery);
        //    //}
        //    //else
        //    //{
        //    //    AddFieldToQuery(groupQuery, keywords, Field.Title);
        //    //    AddFieldToQuery(groupQuery, keywords, Field.DisplayText);
        //    //}

        //    var searchHandler = ServiceLocator.Current.GetInstance<LuceneContentSearchHandler>();

        //    // Get search results
        //    return searchHandler.GetSearchResults<T>(groupQuery, root, 1, int.MaxValue, null, true).TotalHits;
        //}

        //public static IList<T> SearchEvent<T>(IList<string> tags, ContentReference root, int pageNumber, int pageSize, out int totalItems) where T : IContent
        //{
        //    var groupQuery = new GroupQuery(LuceneOperator.AND);

        //    groupQuery.QueryExpressions.Add(new ContentQuery<T>());

        //    if (tags == null || tags.Count <= 0)
        //    {
        //        groupQuery.QueryExpressions.Add(new CreatedDateRangeQuery(DateTime.MinValue, DateTime.MaxValue, true));
        //    }
        //    else
        //    {
        //        groupQuery.QueryExpressions.Add(new EventTagQuery(tags, LuceneOperator.OR));
        //    }

        //    var searchHandler = ServiceLocator.Current.GetInstance<LuceneContentSearchHandler>();

        //    // Get search results
        //    var sortFields = new Collection<SortField>();
        //    sortFields.Add(new SortField(EventDateField.FieldName, SortField.LONG, true));
        //    var results = searchHandler.GetSearchResults<T>(groupQuery, root, pageNumber, pageSize, sortFields, true);

        //    totalItems = results.TotalHits;

        //    // Convert search results to pages
        //    var indexResponseItems = results.IndexResponseItems;

        //    return indexResponseItems.Select(v => searchHandler.GetContent<T>(v)).ToList();
        //}

        public static IList<T> Search<T>(string keywords, IContent root, int pageNumber, int pageSize, out int totalItems) where T : IContent
        {
            var sortFields = new Collection<SortField>();

            var page = root as PageData;
            if (page != null)
            {
                var filterSortOrder = page.GetPropertyValue<FilterSortOrder>(PropertyName.Internal.PageChildOrderRule);

                sortFields.Add(SortFieldFactory.CreateSortField(filterSortOrder));

                if (filterSortOrder == FilterSortOrder.Index)
                {
                    // Add another sort field
                    sortFields.Add(SortFieldFactory.CreateSortField(FilterSortOrder.CreatedAscending));
                }
            }

            return Search<T>(keywords, root.ContentLink, pageNumber, pageSize, sortFields, out totalItems);
        }

        //public static IList<T> SearchCaseByAdvertiserAndIndustry<T>(int adId, int indId, IContent root, int pageNumber, int pageSize, out int totalItems) where T : IContent
        //{
        //    var groupQuery = new GroupQuery(LuceneOperator.AND);

        //    groupQuery.QueryExpressions.Add(new ContentQuery<T>());

        //    if (adId < 1 && indId < 1)
        //    {
        //        groupQuery.QueryExpressions.Add(new CreatedDateRangeQuery(DateTime.MinValue, DateTime.MaxValue, true));
        //    }
        //    else
        //    {
        //        if (adId > 0)
        //        {
        //            groupQuery.QueryExpressions.Add(new CaseAdvertiserQuery(adId, LuceneOperator.AND));
        //        }
        //        if (indId > 0)
        //        {
        //            groupQuery.QueryExpressions.Add(new CaseIndustryQuery(indId, LuceneOperator.AND));
        //        }
        //    }

        //    var searchHandler = ServiceLocator.Current.GetInstance<LuceneContentSearchHandler>();

        //    // Get search results
        //    var sortFields = new Collection<SortField>();
        //    //sortFields.Add(new SortField(CaseAdvertiserField.FieldName, SortField.LONG, true));
        //    //sortFields.Add(new SortField(CaseIndustryField.FieldName, SortField.LONG, true));


        //    var page = root as PageData;
        //    if (page != null)
        //    {
        //        var filterSortOrder = page.GetPropertyValue<FilterSortOrder>(PropertyName.Internal.PageChildOrderRule);

        //        sortFields.Add(SortFieldFactory.CreateSortField(filterSortOrder));

        //        if (filterSortOrder == FilterSortOrder.Index)
        //        {
        //            // Add another sort field
        //            sortFields.Add(SortFieldFactory.CreateSortField(FilterSortOrder.CreatedAscending));
        //        }
        //    }

        //    var results = searchHandler.GetSearchResults<T>(groupQuery, root.ContentLink, pageNumber, pageSize, sortFields, true);

        //    totalItems = results.TotalHits;

        //    // Convert search results to pages
        //    var indexResponseItems = results.IndexResponseItems;

        //    return indexResponseItems.Select(v => searchHandler.GetContent<T>(v)).ToList();
        //}

        public static IList<T> SearchUpcomingEvents<T>(IList<string> tags, ContentReference root, bool inclusive, int numberOfEvents, bool orderByDateDescending, out int totalItems) where T : IContent
        {
            var groupQuery = new GroupQuery(LuceneOperator.AND);

            groupQuery.QueryExpressions.Add(new ContentQuery<T>());
            groupQuery.QueryExpressions.Add(new UpcomingEventQuery(inclusive));

            if (tags != null && tags.Count > 0)
            {
                var tagGroupQuery = new GroupQuery(LuceneOperator.AND);
                tagGroupQuery.QueryExpressions.Add(new EventTagQuery(tags, LuceneOperator.OR));
                groupQuery.QueryExpressions.Add(tagGroupQuery);
            }

            var searchHandler = ServiceLocator.Current.GetInstance<LuceneContentSearchHandler>();

            // Get search results
            var sortFields = new Collection<SortField>();
            sortFields.Add(new SortField(EventDateField.FieldName, SortField.LONG, orderByDateDescending));

            var results = searchHandler.GetSearchResults<T>(groupQuery, root, 1, numberOfEvents, sortFields, true);

            totalItems = results.TotalHits;

            // Convert search results to pages
            var indexResponseItems = results.IndexResponseItems;

            return indexResponseItems.Select(v => searchHandler.GetContent<T>(v, true)).ToList();
        }

        public static int CountPastEvents<T>(IList<string> tags, ContentReference root, int pageNumber, int pageSize) where T : IContent
        {
            var groupQuery = new GroupQuery(LuceneOperator.AND);

            groupQuery.QueryExpressions.Add(new ContentQuery<T>());
            groupQuery.QueryExpressions.Add(new OlderEventQuery());

            if (tags != null && tags.Count > 0)
            {
                var tagGroupQuery = new GroupQuery(LuceneOperator.AND);
                tagGroupQuery.QueryExpressions.Add(new EventTagQuery(tags, LuceneOperator.OR));
                groupQuery.QueryExpressions.Add(tagGroupQuery);
            }

            var searchHandler = ServiceLocator.Current.GetInstance<LuceneContentSearchHandler>();

            // Get search results
            var sortFields = new Collection<SortField>();
            sortFields.Add(new SortField(EventDateField.FieldName, SortField.LONG, true));
            var results = searchHandler.GetSearchResults<T>(groupQuery, root, 1, pageSize, sortFields, true);
            return results.TotalHits;
        }

        public static IList<T> SearchPastEvents<T>(IList<string> tags, ContentReference root, int pageNumber, int pageSize, out int totalItems) where T : IContent
        {
            var groupQuery = new GroupQuery(LuceneOperator.AND);

            groupQuery.QueryExpressions.Add(new ContentQuery<T>());
            groupQuery.QueryExpressions.Add(new OlderEventQuery());

            if (tags != null && tags.Count > 0)
            {
                var tagGroupQuery = new GroupQuery(LuceneOperator.AND);
                tagGroupQuery.QueryExpressions.Add(new EventTagQuery(tags, LuceneOperator.OR));
                groupQuery.QueryExpressions.Add(tagGroupQuery);
            }

            var searchHandler = ServiceLocator.Current.GetInstance<LuceneContentSearchHandler>();

            // Get search results
            var sortFields = new Collection<SortField>();
            sortFields.Add(new SortField(EventDateField.FieldName, SortField.LONG, true));
            var results = searchHandler.GetSearchResults<T>(groupQuery, root, pageNumber, pageSize, sortFields, true);
            totalItems = results.TotalHits;

            // Convert search results to pages
            var indexResponseItems = results.IndexResponseItems;

            return indexResponseItems.Select(v => searchHandler.GetContent<T>(v)).ToList();
        }

        public static Dictionary<string, string> GetTags<T>(ContentReference root, bool includeOlder) where T : IContent
        {
            var dictionary = new Dictionary<string, string>();
            var groupQuery = new GroupQuery(LuceneOperator.AND);

            groupQuery.QueryExpressions.Add(new ContentQuery<T>());
            if (includeOlder)
            {
                var dateRangeQuery = new CreatedDateRangeQuery(DateTime.MinValue, DateTime.MaxValue, true);
                groupQuery.QueryExpressions.Add(dateRangeQuery);
            }
            else
            {
                groupQuery.QueryExpressions.Add(new UpcomingEventQuery(true));
            }

            var searchHandler = ServiceLocator.Current.GetInstance<LuceneContentSearchHandler>();

            var eventTagField = new EventTagField();
            var sortFields = new Collection<SortField>();
            sortFields.Add(new SortField(eventTagField.Name, SortField.STRING));
            // Get search results
            var results = searchHandler.GetSearchResults<T>(groupQuery, root, 1, int.MaxValue, sortFields, true);

            foreach (var item in results.IndexResponseItems)
            {
                if (!string.IsNullOrWhiteSpace(item.DisplayText))
                {
                    var displayText = item.DisplayText.Trim();

                    var tags = displayText.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var tag in tags)
                    {
                        var key = tag.ToLowerInvariant();
                        if (!dictionary.ContainsKey(key))
                        {
                            dictionary.Add(key, tag);
                        }
                    }
                }
            }

            return dictionary;
        }

        private static void AddFieldToQuery(GroupQuery groupQuery, string keyword, Field field, bool wildcard = true)
        {
            if (wildcard)
            {
                if (keyword.StartsWith("\"") && keyword.EndsWith("\""))
                {
                    var fieldQuery = new FieldQuery(keyword.Trim(), field);
                    groupQuery.QueryExpressions.Add(fieldQuery);
                    return;
                }

                if (keyword.Trim().Contains(" "))
                {
                    var words = keyword.Trim().Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                    var query = new GroupQuery(LuceneOperator.OR);
                    foreach (string w in words)
                    {
                        if (w.Equals("and") || w.Equals("+")) continue;
                        var s = w.Replace("\"", string.Empty).Trim();
                        var f = new FieldQuery(string.Format("{0}*", LuceneHelpers.Escape(s)), field);
                        query.QueryExpressions.Add(f);
                    }
                    groupQuery.QueryExpressions.Add(query);
                }
                else
                {
                    var fieldQuery = new FieldQuery(string.Format("{0}*", LuceneHelpers.Escape(keyword)), field);
                    groupQuery.QueryExpressions.Add(fieldQuery);
                }
            }
            else
            {
                var fieldQuery = new FieldQuery(string.Format("{0}", LuceneHelpers.Escape(keyword.Trim())), field);
                groupQuery.QueryExpressions.Add(fieldQuery);
            }
        }

        private static IQueryExpression AddQueryExpression(string keyword, Field field, bool wildcard = true)
        {
            keyword = keyword.Trim();
            if (wildcard)
            {
                if (keyword.StartsWith("\"") && keyword.EndsWith("\""))
                {
                    return new FieldQuery(keyword, field);
                }

                if (keyword.Contains(" "))
                {
                    var words = keyword.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                    var query = new GroupQuery(LuceneOperator.OR);
                    foreach (string w in words)
                    {
                        if (w.Equals("and") || w.Equals("+")) continue;
                        var s = w.Replace("\"", string.Empty);
                        var f = new FieldQuery(string.Format("{0}*", LuceneHelpers.Escape(s)), field);
                        query.QueryExpressions.Add(f);
                    }
                    return query;
                }
                else
                {
                    return new FieldQuery(string.Format("{0}*", LuceneHelpers.Escape(keyword)), field);
                }
            }
            else
            {
                return new FieldQuery(string.Format("{0}", LuceneHelpers.Escape(keyword)), field);
            }
        }

    }
}
