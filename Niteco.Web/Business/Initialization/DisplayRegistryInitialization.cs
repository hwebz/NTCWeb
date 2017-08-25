using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.ServiceLocation;
using EPiServer.Web;
using System.Collections.Generic;
using System.Web.Mvc;
using Niteco.Common.Consts;

namespace Niteco.Web.Business.Initialization
{
    [InitializableModule]
    [ModuleDependency(typeof(EPiServer.Web.InitializationModule))]
    public class DisplayRegistryInitialization : IInitializableModule
    {
        public void Initialize(InitializationEngine context)
        {
            if (context.HostType == HostType.WebApplication)
            {
                // Register Display Options
                var options = ServiceLocator.Current.GetInstance<DisplayOptions>();
                options
                    .Add("full", "/displayoptions/full", ContentAreaTags.FullWidth, "", "epi-icon__layout--full");
                AreaRegistration.RegisterAllAreas();

            }
        }

        public void Preload(string[] parameters){}

        public void Uninitialize(InitializationEngine context){}
    }
}
