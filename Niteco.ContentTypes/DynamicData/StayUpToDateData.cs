using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EPiServer.Data;
using EPiServer.Data.Dynamic;

namespace Niteco.ContentTypes.DynamicData
{
    [EPiServerDataStore(AutomaticallyRemapStore = true, AutomaticallyCreateStore = true, StoreName = "StayUpToDateData")]
    public class StayUpToDateData : IDynamicData
    {

        public Identity Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public DateTime SignUpDateTime { get; set; }

        public StayUpToDateData()
        {
            Id = Identity.NewIdentity(Guid.NewGuid());
            SignUpDateTime = DateTime.UtcNow;
        }
        private static DynamicDataStore GetStore()
        {
            return DynamicDataStoreFactory.Instance.GetStore(typeof(StayUpToDateData));
        }
        public static StayUpToDateData GetByEmail(string email)
        {
            if (string.IsNullOrEmpty(email)) return null;
            DynamicDataStore store = GetStore();
            return store.Items<StayUpToDateData>().FirstOrDefault(x => x.Email == email);
        }

        public static List<StayUpToDateData> GetAll()
        {
            DynamicDataStore store = GetStore();
            return store.Items<StayUpToDateData>().ToList();
        }
        public void Save()
        {
            DynamicDataStore store = GetStore();
            store.Save(this);
        }
        public static void DeleteAll()
        {
            DynamicDataStore store = GetStore();
            store.DeleteAll();
        }
    }
}
