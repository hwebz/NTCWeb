using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAbstraction.RuntimeModel;
using EPiServer.PlugIn;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using Niteco.Common.Extensions;


namespace Niteco.Web.App.Plugins.CleanUpPage
{
    [GuiPlugIn(
     DisplayName = "Clean up data",
     Description = "This is used for cleaning up unused pages and unused properties",
     Area = PlugInArea.AdminMenu,
     Url = "~/Plugins/CleanUpPage/CleanUpPagePlugin.aspx")]
    public partial class CleanUpPagePlugin : Page
    {
        #region Handlers
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                ddlChooseActionType.Items.Add(new ListItem("Remove properties no exist on model","1"));
                ddlChooseActionType.Items.Add(new ListItem("Update tab", "2"));

                ddlChooseCleanUpActionType.Items.Add(new ListItem("Clean up page types", "1"));
                ddlChooseCleanUpActionType.Items.Add(new ListItem("Clean up properties", "2"));

                BindData();
            }
        }
        protected void ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            var item = e.Item;
            var subRepeater = e.Item.FindControl("repeaterUsagePages") as Repeater;
            if (subRepeater != null)
            {
                subRepeater.DataSource = ((PageTypeModel)item.DataItem).UsagePages;
                subRepeater.DataBind();
            }
        }

        protected void btnCleanUpPageData_Click(object sender, EventArgs e)
        {
            var contentTypeRes = ServiceLocator.Current.GetInstance<IContentTypeRepository>();
            foreach (RepeaterItem item in repeaterUnusedPageTypes.Items)
            {
                var checkBox = item.FindControl("chkPage") as CheckBox;
                var hiddenPageId = item.FindControl("hiddenPageId") as HiddenField;
                if (checkBox != null && checkBox.Checked && hiddenPageId != null)
                {
                    var contentTypeId = -1;
                    if (int.TryParse(hiddenPageId.Value, out contentTypeId))
                    {
                        var selectedContentType = contentTypeRes.Load(contentTypeId);
                        if (selectedContentType != null)
                        {
                            RemovePageType(new PageTypeModel(selectedContentType));
                        }

                    }
                }
            }
            BindData();
        }

        protected void ddlChooseActionType_SelectedIndexChanged(object sender, EventArgs e)
        {
            BindData();
        }

        protected void repeaterUnusedProperties_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            var item = e.Item;
            var subRepeater = e.Item.FindControl("repeaterUsagePages") as Repeater;
            if (subRepeater != null)
            {
                subRepeater.DataSource = ((ProperyModel)item.DataItem).UsageContents;
                subRepeater.DataBind();
            }
        }

        protected void btnCleanUpProperty_Click(object sender, EventArgs e)
        {
            var propertyDefinitionRes = ServiceLocator.Current.GetInstance<IPropertyDefinitionRepository>();
            var propDefinitionModelRes = ServiceLocator.Current.GetInstance<ContentTypeModelRepository>();
            var tabRes = ServiceLocator.Current.GetInstance<ITabDefinitionRepository>();
            foreach (RepeaterItem item in repeaterUnusedProperties.Items)
            {
                var checkBox = item.FindControl("chkProperties") as CheckBox;
                var hiddenPropertyId = item.FindControl("hiddenPropertyId") as HiddenField;
                if (checkBox != null && checkBox.Checked && hiddenPropertyId != null)
                {
                    var propertyId = -1;
                    if (int.TryParse(hiddenPropertyId.Value, out propertyId))
                    {
                        var selectedPropertyDefinition = propertyDefinitionRes.Load(propertyId);
                        if (selectedPropertyDefinition != null)
                        {
                            var selectedPropertyModel =
                                propDefinitionModelRes.GetPropertyModel(selectedPropertyDefinition.ContentTypeID,
                                                                        selectedPropertyDefinition);

                            if(ddlChooseActionType.SelectedValue == "1")
                            {
                                propertyDefinitionRes.Delete(selectedPropertyDefinition);
                            }
                            else if (selectedPropertyModel != null && !string.IsNullOrEmpty(selectedPropertyModel.TabName))
                            {
                                var cloneProperty = selectedPropertyDefinition.CreateWritableClone();

                                var newTab = tabRes.Load(selectedPropertyModel.TabName);
                                if(newTab == null)
                                {
                                    newTab = new TabDefinition();
                                    newTab.Name = selectedPropertyModel.TabName;
                                    tabRes.Save(newTab);
                                }
                                newTab = tabRes.Load(selectedPropertyModel.TabName);
                                cloneProperty.Tab = newTab;
                                propertyDefinitionRes.Save(cloneProperty);
                            }
                        }

                    }
                }
            }
            BindData();
        }
        #endregion
        #region Private methods
        private void BindData()
        {
            var contentTypeRes = ServiceLocator.Current.GetInstance<IContentTypeRepository>();
            var propDefinitionModelRes = ServiceLocator.Current.GetInstance<ContentTypeModelRepository>();
            var contentTypes = contentTypeRes.List();

            var unusedContentTypes = new List<PageTypeModel>();
            var unusedProperties = new List<ProperyModel>();
            foreach (var contentType in contentTypes)
            {
                if(contentType.ModelType == null && contentType.IsAvailable)
                {
                    unusedContentTypes.Add(new PageTypeModel(contentType));
                }
                else if(contentType.IsAvailable)
                {
                    foreach (var prop in contentType.PropertyDefinitions)
                    {
                        var propModel = propDefinitionModelRes.GetPropertyModel(contentType.ID, prop);

                        if (ddlChooseActionType.SelectedValue == "1")
                        {
                            if (!prop.ExistsOnModel || propModel == null ||((propModel.State == SynchronizationStatus.Deleted)))
                            {
                                unusedProperties.Add(new ProperyModel(prop, propModel));
                            }
                        }
                        else
                        {
                            if ((propModel != null && (propModel.State == SynchronizationStatus.Conflict) && (prop.Tab!=null && prop.Tab.Name != propModel.TabName)))
                            {
                                unusedProperties.Add(new ProperyModel(prop, propModel));
                            }
                        }
                    }
                }
            }
            if (unusedContentTypes.Count > 0)
            {
                repeaterUnusedPageTypes.DataSource = unusedContentTypes;
                repeaterUnusedPageTypes.DataBind();

                repeaterUnusedPageTypes.Visible = true;
                plhCleanupPageEmpty.Visible = false;
            }
            else
            {
                repeaterUnusedPageTypes.Visible = false;
                plhCleanupPageEmpty.Visible = true;
            }
            if (unusedProperties.Count > 0)
            {
                repeaterUnusedProperties.DataSource = unusedProperties;
                repeaterUnusedProperties.DataBind();
                repeaterUnusedProperties.Visible = true;
                plhNoPropertiesFound.Visible = false;
            }
            else
            {
                repeaterUnusedProperties.Visible = false;
                plhNoPropertiesFound.Visible = true;
            }

            if(ddlChooseCleanUpActionType.SelectedValue == "1")
            {
                plhCleanupPage.Visible = true;
                plhCleanupProperties.Visible = false;
            }
            else
            {
                plhCleanupPage.Visible = false;
                plhCleanupProperties.Visible = true;
            }
        }
        private void RemovePageType(PageTypeModel pageType)
        {
            var contentTypeRes = ServiceLocator.Current.GetInstance<IContentTypeRepository>();
            var contentRes = ServiceLocator.Current.GetInstance<IContentRepository>();
            if(pageType.UsagePages != null && pageType.UsagePages.Any())
            {
                foreach (var page in pageType.UsagePages)
                {
                    contentRes.Delete(page.ContentLink,true,AccessLevel.NoAccess);
                }
            }
            contentTypeRes.Delete(pageType.Id);
        }
        #endregion

        protected void ddlChooseCleanUpActionType_SelectedIndexChanged(object sender, EventArgs e)
        {
            BindData();
        }
    }

    public class PageTypeModel
    {
        public int Id { get; set; }
        public Guid Guid { get; set; }
        public string Name { get; set; }
        public string ModelType { get; set; }
        public IEnumerable<IContent> UsagePages { get; set; }

        public PageTypeModel(ContentType contentType)
        {
            Id = contentType.ID;
            Guid = contentType.GUID;
            Name = contentType.Name;
            ModelType = contentType.ModelTypeString;

            var contentLoader = ServiceLocator.Current.GetInstance<IContentLoader>();
            UsagePages =
                contentLoader.GetDescendents(ContentReference.RootPage).Select(ct => ct.GetContent()).Where(p => p.ContentTypeID == contentType.ID);

            //var pageQuery = ServiceLocator.Current.GetInstance<IPageCriteriaQueryService>();

            //var criteriaCollection = new PropertyCriteriaCollection();
            //var criteria = new PropertyCriteria();
            //criteria.Name = "PageTypeID";
            //criteria.Type = PropertyDataType.PageType;
            //criteria.Condition = CompareCondition.Equal;
            //criteria.Value = contentType.ID.ToString(CultureInfo.InvariantCulture);
            //criteriaCollection.Add(criteria);


            //UsagePages = pageQuery.FindPagesWithCriteria(ContentReference.RootPage, criteriaCollection);
        }
    }

    public class ProperyModel
    {
        public int Id { get; set; }
        public int ContentTypeId { get; set; }
        public string ContentTypeName { get; set; }
        public string Name { get; set; }
        public string EditCaption { get; set; }
        public string ModelEditCaption { get; set; }
        public string Description { get; set; }
        public string ModelDescription { get; set; }
        public string TabName { get; set; }
        public string ModelTabName { get; set; }
        public string TypeName { get; set; }
        public string ModelTypeName { get; set; }
        public string ModelState { get; set; }
        public int SortOrder { get; set; }
        public int? ModelSortOrder { get; set; }
        public IEnumerable<ContentUsage> UsageContents { get; set; }

        public ProperyModel(PropertyDefinition propertyDefinition, PropertyDefinitionModel propertyDefinitionModel)
        {
            var contentTypeRes = ServiceLocator.Current.GetInstance<IContentTypeRepository>();
            var propertyDefinitionRes = ServiceLocator.Current.GetInstance<IPropertyDefinitionRepository>();
            var propertyDefinitionSync = new PropertyDefinitionSynchronizer();

            Id = propertyDefinition.ID;
            ContentTypeId = propertyDefinition.ContentTypeID;
            ContentTypeName = contentTypeRes.Load(propertyDefinition.ContentTypeID).Name;
            Name = propertyDefinition.Name;
            EditCaption = propertyDefinition.EditCaption;
            Description = propertyDefinition.HelpText;
            TabName = propertyDefinition.Tab.Name;
            TypeName = propertyDefinition.Type.Name;
            SortOrder = propertyDefinition.FieldOrder;
            if (propertyDefinitionModel != null)
            {
                ModelDescription = propertyDefinitionModel.Description;
                ModelEditCaption = propertyDefinitionModel.DisplayName;
                ModelTabName = propertyDefinitionModel.TabName;
                ModelState = Enum.GetName(typeof (SynchronizationStatus), propertyDefinitionModel.State);
                ModelSortOrder = propertyDefinitionModel.Order;
                try
                {
                    ModelTypeName = propertyDefinitionSync.ResolveType(propertyDefinitionModel).Name;
                }
                catch
                {
                    ModelTypeName = propertyDefinitionModel.Type.Name;
                }
            }
            UsageContents = propertyDefinitionRes.GetUsage(propertyDefinition.ID, false, true, propertyDefinition.IsDynamicProperty);
        }
    }
}