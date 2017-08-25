using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using EPiServer.Core;
using Niteco.Common.Extensions;

namespace Niteco.Common.Helpers
{
    /// <summary>
    /// Provides constants and methods for working with EpiServer property names.
    /// </summary>
    public static class PropertyName
    {
        /// <summary>
        /// Contains constants that correspond to internal name of standard EpiServer
        /// properties.
        /// </summary>        
        public static class Internal
        {
            /// <summary> A name of the PageLink property, equals to "PageLink". </summary>
            public const string PageLink = "PageLink";

            /// <summary> A name of the PageTypeID property, equals to "PageTypeID". </summary>
            public const string PageTypeID = "PageTypeID";

            /// <summary>A name of the PageParentLink property, equals to "PageParentLink".</summary>
            public const string PageParentLink = "PageParentLink";

            /// <summary> A name of the PagePendingPublish property, equals to "PagePendingPublish". </summary>
            public const string PagePendingPublish = "PagePendingPublish";

            /// <summary> A name of the PageArchiveLink property, equals to "PageArchiveLink". </summary>
            public const string PageArchiveLink = "PageArchiveLink";

            /// <summary> A name of the PageTargetFrame property, equals to "PageTargetFrame". </summary>
            public const string PageTargetFrame = "PageTargetFrame";

            /// <summary> A name of the PageExternalURL property, equals to "PageExternalURL". </summary>
            public const string PageExternalURL = "PageExternalURL";

            /// <summary> A name of the PagePeerOrder property, equals to "PagePeerOrder". </summary>
            public const string PagePeerOrder = "PagePeerOrder";

            /// <summary> A name of the PageTypeName property, equals to "PageTypeName". </summary>
            public const string PageTypeName = "PageTypeName";

            /// <summary> A name of the PageFolderID property, equals to "PageFolderID". </summary>
            public const string PageFolderID = "PageFolderID";

            /// <summary> A name of the LinkURL property, equals to "PageLinkURL". </summary>
            public const string LinkURL = "PageLinkURL";

            /// <summary> A name of the PageStartPublish property, equals to "PageStartPublish". </summary>
            public const string PageStartPublish = "PageStartPublish";

            /// <summary> A name of the PageStopPublish property, equals to "PageStopPublish". </summary>
            public const string PageStopPublish = "PageStopPublish";

            /// <summary>A name of the PageVisibleInMenu property, equals to "PageVisibleInMenu".</summary>
            public const string PageVisibleInMenu = "PageVisibleInMenu";

            /// <summary>A name of the PageLanguageBranch property, equals to "PageLanguageBranch".</summary>
            public const string PageLanguageBranch = "PageLanguageBranch";

            /// <summary>A name of the PageGuid property, equals to "PageGUID".</summary>
            public const string PageGuid = "PageGUID";

            /// <summary>A name of the PageDeleted property, equals to "PageDeleted".</summary>
            public const string PageDeleted = "PageDeleted";

            /// <summary>A name of the PageCreated property, equals to "PageCreated".</summary>
            public const string PageCreated = "PageCreated";

            /// <summary>A name of the PageChanged property, equals to "PageChanged".</summary>
            public const string PageChanged = "PageChanged";

            /// <summary>A name of the PageSaved property, equals to "PageSaved".</summary>
            public const string PageSaved = "PageSaved";

            /// <summary>A name of the PageCreatedBy property, equals to "PageCreatedBy".</summary>
            public const string PageCreatedBy = "PageCreatedBy";

            /// <summary>A name of the PageChangedBy property, equals to "PageChangedBy".</summary>
            public const string PageChangedBy = "PageChangedBy";

            /// <summary>A name of the PageChangedOnPublish property, equals to "PageChangedOnPublish".</summary>
            public const string PageChangedOnPublish = "PageChangedOnPublish";

            /// <summary>A name of the PageWorkStatus property, equals to "PageWorkStatus".</summary>
            public const string PageWorkStatus = "PageWorkStatus";

            /// <summary>A name of the PageMasterLanguageBranch property, equals to "PageMasterLanguageBranch".</summary>
            public const string PageMasterLanguageBranch = "PageMasterLanguageBranch";

            /// <summary>A name of the PageName property, equals to "PageName".</summary>
            public const string PageName = "PageName";

            /// <summary>A name of the PageUrlSegment property, equals to "PageUrlSegment".</summary>
            public const string PageUrlSegment = "PageURLSegment";

            /// <summary>A name of the PageChildOrderRule property, equals to "PageChildOrderRule".</summary>
            public const string PageChildOrderRule = "PageChildOrderRule";

            /// <summary>A name of the PageShortcutLink propertym equals to "PageShortcutLink".</summary>            
            public const string PageShortcutLink = "PageShortcutLink";

            /// <summary>A name of the PageDelayedPublish propertym equals to "PageDelayedPublish".</summary>            
            public const string PageDelayedPublish = "PageDelayedPublish";

            /// <summary>A name of the PageShortcutType property equals to "PageShortcutType".</summary>            
            public const string PageShortcutType = "PageShortcutType";

            /// <summary>A name of the PageCategory property equals to "PageCategory".</summary>            
            public const string PageCategory = "PageCategory";
        }

        /// <summary>
        /// Contains constants that correspond to internal name of standard EpiServer
        /// properties.
        /// </summary>        
        public static class Public
        {
            /// <summary> A name of the PageLink property, equals to "PageLink". </summary>
            public static readonly string PageLink = GetPublic(x => x.PageLink);

            /// <summary> A name of the PageTypeID property, equals to "PageTypeID". </summary>
            public static readonly string PageTypeID = GetPublic(x => x.PageTypeID);

            /// <summary> A name of the ParentLink property, equals to "ParentLink".</summary>
            public static readonly string ParentLink = GetPublic(x => x.ParentLink);

            /// <summary> A name of the PendingPublish property, equals to "PendingPublish". </summary>
            public static readonly string PendingPublish = GetPublic(x => x.PendingPublish);

            /// <summary> A name of the ArchiveLink property, equals to "ArchiveLink". </summary>
            public static readonly string ArchiveLink = GetPublic(x => x.ArchiveLink);

            /// <summary> A name of the PageTypeName property, equals to "PageTypeName". </summary>
            public static readonly string PageTypeName = GetPublic(x => x.PageTypeName);

            /// <summary> A name of the LinkURL property, equals to "LinkURL". </summary>
            public static readonly string LinkURL = GetPublic(x => x.LinkURL);

            /// <summary> A name of the StartPublish property, equals to "StartPublish". </summary>
            public static readonly string StartPublish = GetPublic(x => x.StartPublish);

            /// <summary> A name of the StopPublish property, equals to "StopPublish". </summary>
            public static readonly string StopPublish = GetPublic(x => x.StopPublish);

            /// <summary>A name of the VisibleInMenu property, equals to "VisibleInMenu".</summary>
            public static readonly string VisibleInMenu = GetPublic(x => x.VisibleInMenu);

            /// <summary>A name of the LanguageBranch property, equals to "LanguageBranch".</summary>
            public static readonly string LanguageBranch = GetPublic(x => x.LanguageBranch);

            /// <summary>A name of the PageGuid property, equals to "PageGuid".</summary>
            public static readonly string PageGuid = GetPublic(x => x.PageGuid);

            /// <summary>A name of the IsDeleted property, equals to "IsDeleted".</summary>
            public static readonly string IsDeleted = GetPublic(x => x.IsDeleted);

            /// <summary>A name of the Created property, equals to "Created".</summary>
            public static readonly string Created = GetPublic(x => x.Created);

            /// <summary>A name of the Changed property, equals to "Changed".</summary>
            public static readonly string Changed = GetPublic(x => x.Changed);

            /// <summary>A name of the Saved property, equals to "Saved".</summary>
            public static readonly string Saved = GetPublic(x => x.Saved);

            /// <summary>A name of the CreatedBy property, equals to "CreatedBy".</summary>
            public static readonly string CreatedBy = GetPublic(x => x.CreatedBy);

            /// <summary>A name of the ChangedBy property, equals to "ChangedBy".</summary>
            public static readonly string ChangedBy = GetPublic(x => x.ChangedBy);

            /// <summary>A name of the Status property, equals to "Status".</summary>
            public static readonly string Status = GetPublic(x => x.Status);

            /// <summary>A name of the MasterLanguageBranch property, equals to "MasterLanguageBranch".</summary>
            public static readonly string MasterLanguageBranch = GetPublic(x => x.MasterLanguageBranch);

            /// <summary>A name of the PageName property, equals to "PageName".</summary>
            public static readonly string PageName = GetPublic(x => x.PageName);

            /// <summary>A name of the URLSegment property, equals to "URLSegment".</summary>
            public static readonly string URLSegment = GetPublic(x => x.URLSegment);

            /// <summary>A name of the LinkType property equals to "LinkType".</summary>
            public static readonly string LinkType = GetPublic(x => x.LinkType);

            /// <summary>A name of the Category property equals to "Category".</summary>
            public static readonly string Category = GetPublic(x => x.Category);

            /// <summary>A name of the WorkPageID property equals to "WorkPageID".</summary>
            public static readonly string WorkPageID = GetPublic(x => x.WorkPageID);
        }

        /// <summary>
        /// Contains constants that correspond to internal name of EPiServer PageReference properties.
        /// </summary>
        public static class OfPageReference
        {
            /// <summary> A name of the ID property, equals to "ID".</summary>
            public static readonly string ID = GetPublic<PageReference>(p => p.ID);

            /// <summary> A name of the ID property, equals to "RemoteSite".</summary>
            public static readonly string RemoteSite = GetPublic<PageReference>(p => p.RemoteSite);
        }

        /// <summary>
        /// Maps public properties of <see cref="PageData"/> to inner properties.
        /// </summary>
        public static readonly IDictionary<string, string> PublicToInnerMap = new Dictionary<string, string>
        {            
            { Public.PageLink, Internal.PageLink },
            { Public.PageTypeID, Internal.PageTypeID },
            { Public.ParentLink, Internal.PageParentLink },
            { Public.PendingPublish, Internal.PagePendingPublish },
            { Public.ArchiveLink, Internal.PageArchiveLink },
            { Public.PageTypeName, Internal.PageTypeName },
            { Public.LinkURL, Internal.LinkURL },
            { Public.StartPublish, Internal.PageStartPublish },
            { Public.StopPublish, Internal.PageStopPublish },
            { Public.VisibleInMenu, Internal.PageVisibleInMenu },
            { Public.LanguageBranch, Internal.PageLanguageBranch },
            { Public.PageGuid, Internal.PageGuid },
            { Public.IsDeleted, Internal.PageDeleted },
            { Public.Created, Internal.PageCreated },
            { Public.Changed, Internal.PageChanged },
            { Public.Saved, Internal.PageSaved },
            { Public.CreatedBy, Internal.PageCreatedBy },                        
            { Public.ChangedBy, Internal.PageChangedBy },
            { Public.Status, Internal.PageWorkStatus },
            { Public.MasterLanguageBranch, Internal.PageMasterLanguageBranch },
            { Public.PageName, Internal.PageName },            
            { Public.URLSegment, Internal.PageUrlSegment },            
            { Public.LinkType, Internal.PageShortcutType },            
            { Public.Category, Internal.PageCategory },            
        };

        /// <summary>
        /// Maps inner properties of <see cref="PageData"/> to public properties.
        /// </summary>
        public static readonly IDictionary<string, string> InnerToPublicMap = PublicToInnerMap.ToDictionary(i => i.Value, i => i.Key);

        /// <summary>
        /// Gets the internal property key (within PropertyCollection)
        /// by the name of the property on the class.
        /// </summary>
        /// <param name="publicName">Name of the property as visible on the class.</param>
        /// <returns>
        /// Internal property name if known, otherwise <paramref name="publicName"/>.
        /// </returns>
        public static string GetInternal(string publicName)
        {
            if (string.IsNullOrEmpty(publicName))
            {
                throw new ArgumentNullException("publicName");
            }

            return PublicToInnerMap.GetValueOrDefault(publicName) ?? publicName;
        }

        private static string GetPublic(Expression<Func<PageData, object>> reference)
        {
            return GetPublic<PageData>(reference);
        }

        /// <summary>
        /// Gets the public property name (on the class)
        /// by the internal property key (within PropertyCollection).
        /// </summary>
        /// <param name="internalName">Name of the internal property key.</param>
        /// <returns>
        /// Public property name if known, otherwise <paramref name="internalName" />.
        /// </returns>
        public static string GetPublic(string internalName)
        {
            if (string.IsNullOrEmpty(internalName))
            {
                throw new ArgumentNullException("internalName");
            }

            return InnerToPublicMap.GetValueOrDefault(internalName) ?? internalName;
        }

        /// <summary>
        /// Gets the public name of the property (as visible on the class).
        /// </summary>
        /// <typeparam name="TPageData">The type of the page data.</typeparam>
        /// <typeparam name="TValue">The value of the property.</typeparam>
        /// <param name="expression">The property reference.</param>
        /// <returns>
        /// A name of the property referenced by <paramref name="expression" />.
        /// </returns>
        public static string GetPublic<TPageData, TValue>(Expression<Func<TPageData, TValue>> expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }

            var target = expression.Body;

            // Uncast is required for cases like 'Func<object> f = () => value.ID;'
            // If Id is int, compiler will put automatic cast here, changing code to
            // 'Func<object> f = () => (object)value.ID;'
            var cast = target as UnaryExpression;
            if (cast != null)
            {
                target = cast.Operand;
            }

            var propertyExpression = target as MemberExpression;
            if (propertyExpression == null)
            {
                throw new ArgumentException(target + " is not a reference to a property.", "expression");
            }

            var property = propertyExpression.Member as PropertyInfo;
            if (property == null)
            {
                throw new ArgumentException(propertyExpression.Member + " is not a property.", "expression");
            }

            return property.Name;
        }

        public static string GetPublic<TPageData>(Expression<Func<TPageData, object>> expression)
        {
            return GetPublic<TPageData, object>(expression);
        }

        /// <summary>
        /// Gets the internal name of the property (as visible on the class).
        /// </summary>
        /// <typeparam name="TPageData">The type of the page data.</typeparam>
        /// <typeparam name="TValue">The value of the property.</typeparam>
        /// <param name="reference">The property reference.</param>
        /// <returns>
        /// A name of the property referenced by <paramref name="reference" />.
        /// </returns>
        public static string GetInternal<TPageData, TValue>(Expression<Func<TPageData, TValue>> reference)
        {
            if (reference == null)
            {
                throw new ArgumentNullException("reference");
            }

            var publicName = GetPublic(reference);

            return GetInternal(publicName);
        }

        /// <summary>
        /// Gets the internal name of the property (as visible on the class).
        /// </summary>
        /// <param name="propertyInfo">The property info.</param>
        /// <returns>
        /// A name of the property described by <paramref name="propertyInfo" />.
        /// </returns>
        public static string GetInternal(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null)
            {
                throw new ArgumentNullException("propertyInfo");
            }

            var publicName = propertyInfo.Name;

            return GetInternal(publicName);
        }
    }
}
