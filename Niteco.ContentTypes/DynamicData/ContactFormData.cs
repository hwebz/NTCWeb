using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EPiServer.Data;
using EPiServer.Data.Dynamic;

namespace Niteco.ContentTypes.DynamicData
{
    [EPiServerDataStore(AutomaticallyRemapStore = true, AutomaticallyCreateStore = true, StoreName = "ContactFormData")]
    public class ContactFormData
    {

        public Identity Id { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Message { get; set; }
        public string Country { get; set; }

        public ContactFormData()
        {
            Id = Identity.NewIdentity(Guid.NewGuid());
        }
        private static DynamicDataStore GetStore()
        {
            return DynamicDataStoreFactory.Instance.GetStore(typeof(ContactFormData));
        }
        public static ContactFormData GetByEmail(string email)
        {
            DynamicDataStore store = GetStore();
            return store.Items<ContactFormData>().FirstOrDefault(x => x.Email == email);
        }

        public static List<ContactFormData> GetAll()
        {
            DynamicDataStore store = GetStore();
            return store.Items<ContactFormData>().ToList();
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
