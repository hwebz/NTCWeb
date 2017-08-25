using EPiServer.Filters;
using Lucene.Net.Search;
using Niteco.Common.Search;
using Niteco.Common.Search.Queries.Lucene;
using Niteco.Search.Fields;

namespace Niteco.Search
{
    public class SortFieldFactory
    {
        public static SortField CreateSortField(Field field, bool reverse)
        {
            switch (field)
            {
                case Field.Authors:
                    return new SortField(SearchSettings.Config.IndexingServiceFieldNameAuthors, SortField.STRING, reverse);
                case Field.Created:
                    return new SortField(SearchSettings.Config.IndexingServiceFieldNameCreated, SortField.LONG, reverse);
                case Field.Culture:
                    return new SortField(SearchSettings.Config.IndexingServiceFieldNameCulture, SortField.STRING, reverse);
                case Field.Default:
                    return new SortField(SearchSettings.Config.IndexingServiceFieldNameDefault, SortField.STRING, reverse);
                case Field.DisplayText:
                    return new SortField(SearchSettings.Config.IndexingServiceFieldNameDisplayText, SortField.STRING, reverse);
                case Field.Id:
                    return new SortField(SearchSettings.Config.IndexingServiceFieldNameId, SortField.STRING, reverse);
                case Field.ItemStatus:
                    return new SortField(SearchSettings.Config.IndexingServiceFieldNameItemStatus, SortField.SHORT, reverse);
                case Field.ItemType:
                    return new SortField(SearchSettings.Config.IndexingServiceFieldNameType, SortField.STRING, reverse);
                case Field.Modified:
                    return new SortField(SearchSettings.Config.IndexingServiceFieldNameModified, SortField.LONG, reverse);
                case Field.Title:
                    return new SortField(SearchSettings.Config.IndexingServiceFieldNameTitle, SortField.STRING, reverse);
            }

            return new SortField(SearchSettings.Config.IndexingServiceFieldNameDefault, SortField.STRING, reverse);
        }

        public static SortField CreateSortField(FilterSortOrder filterSortOrder)
        {
            switch (filterSortOrder)
            {
                case FilterSortOrder.Alphabetical:
                    return new SortField(SearchSettings.Config.IndexingServiceFieldNameTitle, SortField.STRING, false);
                case FilterSortOrder.Index:
                    return new SortField(SortIndexField.FieldName, SortField.INT, false);
                case FilterSortOrder.CreatedAscending:
                    return new SortField(SearchSettings.Config.IndexingServiceFieldNameCreated, SortField.LONG, false);
                case FilterSortOrder.CreatedDescending:
                    return new SortField(SearchSettings.Config.IndexingServiceFieldNameCreated, SortField.LONG, true);
                case FilterSortOrder.ChangedDescending:
                    return new SortField(SearchSettings.Config.IndexingServiceFieldNameModified, SortField.LONG, true);
                case FilterSortOrder.PublishedAscending:
                    return new SortField(StartPublishedField.FieldName, SortField.LONG, false);
                case FilterSortOrder.PublishedDescending:
                    return new SortField(StartPublishedField.FieldName, SortField.LONG, true);
            }

            return new SortField(SearchSettings.Config.IndexingServiceFieldNameDefault, SortField.STRING, false);
        }
    }
}
