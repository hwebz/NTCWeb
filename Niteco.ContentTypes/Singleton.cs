using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using EPiServer.Logging.Compatibility;

namespace Niteco.ContentTypes
{
    public static class Singleton<TClass> where TClass : class
    {
        // ReSharper disable StaticFieldInGenericType
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        //private static readonly object SyncObject = new object();

        private static volatile TClass _instance;
        // ReSharper restore StaticFieldInGenericType
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1409:RemoveUnnecessaryCode",
            Justification = "Explicit static constructor to tell C# compiler not to mark type as beforefieldinit")]
        // ReSharper disable EmptyConstructor
        static Singleton()
        {
            // ReSharper restore EmptyConstructor
            // Explicit static constructor to tell C# compiler not to mark type as beforefieldinit
        }

        public static TClass Instance
        {
            get
            {
                if (_instance == null)
                {
                    //lock (SyncObject)
                    //{
                    if (_instance == null)
                    {
                        Log.Debug("Creating singleton instanse for: " + typeof(TClass).FullName);
                        _instance = (TClass)Activator.CreateInstance(typeof(TClass), true);
                    }
                    //}
                }

                return _instance;
            }

            set
            {
                //lock (SyncObject)
                //{
                _instance = value;
                //}
            }
        }
    }
}
