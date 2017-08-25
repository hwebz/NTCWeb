using System.Collections.Generic;
using EPiServer.Framework;
using EPiServer.Security;
using Niteco.Common.Search.Queries.Lucene;

namespace Niteco.Search.Queries.Lucene
{
    public static class AccessControlListQueryExtensions
    {
        public static void AddAclForUser(this AccessControlListQuery query, PrincipalInfo principal, object context)
        {
            query.AddAclForUser(VirtualRoleRepository<VirtualRoleProviderBase>.GetDefault(), principal, context);
        }
        public static void AddAclForUser(this AccessControlListQuery query, VirtualRoleRepository<VirtualRoleProviderBase> virtualRoleRepository, PrincipalInfo principal, object context)
        {
            if (principal == null)
            {
                return;
            }
            Validator.ThrowIfNull("virtualRoleRepository", virtualRoleRepository);
            if (!string.IsNullOrEmpty(principal.Name))
            {
                query.AddUser(principal.Name);
            }
            ICollection<string> roleList = principal.RoleList;
            if (roleList != null)
            {
                foreach (string current in roleList)
                {
                    query.AddRole(current);
                }
            }
            foreach (string current2 in virtualRoleRepository.GetAllRoles())
            {
                VirtualRoleProviderBase virtualRoleProviderBase;
                if (virtualRoleRepository.TryGetRole(current2, out virtualRoleProviderBase) && virtualRoleProviderBase.IsInVirtualRole(principal.Principal, context))
                {
                    query.AddRole(current2);
                }
            }
        }
    }
}
