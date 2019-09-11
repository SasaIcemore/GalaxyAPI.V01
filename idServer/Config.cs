using IdentityModel;
using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Test;
using MyConfig;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Tools;
using Tools.pgsql;

namespace GalaxyAPI.V01
{
    public class Config
    {
        public static IEnumerable<IdentityResource> GetIdentityResourceResources()
        {
            var customProfile = new IdentityResource(
                name: "custom.profile",
                displayName: "Custom profile",
                claimTypes: new[] { "role" });

            return new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                customProfile
            };
        }

        public static IEnumerable<ApiResource> GetApiResources()
        {
            return new List<ApiResource>
            {
                new ApiResource("api", "My API",new List<string>(){JwtClaimTypes.Role})
            };
        }

        //IdentityServer客户端信息
        public static IEnumerable<Client> GetClients()
        {
            return new List<Client>
            {
                new Client()
               {
                   ClientId="glxApi001",
                   AllowedGrantTypes=GrantTypes.ClientCredentials,//客户端模式
                   ClientSecrets={ new Secret("console.write".Sha256())},
                   AllowedScopes =
                    {   "api" ,IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,"custom.profile"
                    }
               },
               new Client()
               {
                   ClientId="glxApi002",
                   AllowedGrantTypes=GrantTypes.ResourceOwnerPassword,//密码模式
                   ClientSecrets={ new Secret("console.write".Sha256())},
                   AllowedScopes =
                   {    "api" ,IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,"custom.profile"
                   }
               }
            };
        }

        //模拟用户
        public static List<TestUser> GetTsetUsers()
        {
            DataTable tbl = ConfigManager.dataHelper.GetDataTbl(@"select a.id,name,pwd,b.id as role_id from public.user as a
                                                    left join public.role as b on a.role_id=b.id where a.is_del=false");
            List<TestUser> userList = ConfigManager.dataHelper.DataTableToList<TestUser>(tbl, delegate (DataRow dr) {
                return new TestUser()
                {
                    SubjectId = dr["id"].ToString().Trim(),
                    Username = dr["name"].ToString().Trim(),
                    Password = dr["pwd"].ToString().Trim(),
                    Claims = new List<Claim>
                    {
                        new Claim(JwtClaimTypes.Role, dr["role_id"].ToString().Trim())
                    }
                };
            });
            return userList;
        }
    }
}
