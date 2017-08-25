using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EPiServer.DataAbstraction;
using EPiServer.ServiceLocation;
using Niteco.Web.Data.Pages;

namespace Niteco.Web.Data
{
    public class ContentTypes
    {
        private static int? cmsSettingsPageTypeId;

        private static int? homePageTypeId;

        private static int? eventPageTypeId;

        public static int CmsSettingsPageTypeId
        {
            get
            {
                if (cmsSettingsPageTypeId == null)
                {
                    var contentTypeRepository = ServiceLocator.Current.GetInstance<IContentTypeRepository>();
                    // Site Settings Page
                    var siteSettingsPageType = contentTypeRepository.Load<CmsSettingsPageData>();
                    cmsSettingsPageTypeId = siteSettingsPageType.ID;
                }

                return cmsSettingsPageTypeId.Value;
            }
        }

        //public static int HomePageTypeId
        //{
        //    get
        //    {
        //        if (homePageTypeId == null)
        //        {
        //            var contentTypeRepository = ServiceLocator.Current.GetInstance<IContentTypeRepository>();
        //            // Home Page
        //            var homePageType = contentTypeRepository.Load<HomePageData>();
        //            homePageTypeId = homePageType.ID;
        //        }

        //        return homePageTypeId.Value;
        //    }
        //}

        public static int EventPageTypeId
        {
            get
            {
                //if (eventPageTypeId == null)
                //{
                //    var contentTypeRepository = ServiceLocator.Current.GetInstance<IContentTypeRepository>();
                //    var eventPageDataPageType = contentTypeRepository.Load<EventPageData>();
                //    eventPageTypeId = eventPageDataPageType.ID;
                //}

                //return eventPageTypeId.Value;

                return 0;
            }
        }
    }
}