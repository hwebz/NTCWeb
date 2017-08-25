using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Niteco.Common.Data
{
    public class PageGroups
    {
        public static class Container
        {
            public const string Name = "Container";

            public const int Order = 100;
        }

        public static class Content
        {
            public const string Name = "Content";

            public const int Order = 200;
        }

        public static class Setting
        {
            public const string Name = "Setting";

            public const int Order = 2000;
        }

        //public const string Site = "Site";

        //public const string Setting = "Setting";

        //public const string System = "System";
    }
}
