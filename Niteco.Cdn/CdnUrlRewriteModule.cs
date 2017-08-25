using System;
using System.Web;

namespace Niteco.Cdn
{
    public class CdnUrlRewriteModule : IHttpModule
    {
        public void Init(HttpApplication context)
        {
            context.BeginRequest += OnBeginRequest;
        }

        private void OnBeginRequest(object sender, EventArgs e)
        {
            var url = HttpContext.Current.Request.Url.AbsoluteUri;
            var cdnConfiguration = CdnConfigurationSection.GetConfiguration();
            if (string.IsNullOrEmpty(cdnConfiguration.Url) || !url.StartsWith(cdnConfiguration.Url))
            {
                return;
            }
            HttpContext.Current.Items[CdnConfigurationSection.GetConfiguration().ItemKey] = true;
        }

        public void Dispose()
        {
        }
    }
}