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
        //IdentityServer配置——用户
        public static IEnumerable<ApiResource> GetResources()
        {
            return new List<ApiResource> { new ApiResource("api", "glxapi", new List<string>() { JwtClaimTypes.Role }) };
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
                        IdentityServerConstants.StandardScopes.Profile
                    }
               },
               new Client()
               {
                   ClientId="glxApi002",
                   AllowedGrantTypes=GrantTypes.ResourceOwnerPassword,//密码模式
                   ClientSecrets={ new Secret("console.write".Sha256())},
                   RequireClientSecret=false,
                   AllowedScopes =
                   {    "api" ,IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile
                   }
               }
            };
        }

        //模拟用户
        public static List<TestUser> GetTsetUsers()
        {
            NpgsqlHelper data = new NpgsqlHelper(ConfigManager.DbType, ConfigManager.HOST,ConfigManager.DB,ConfigManager.UserName,ConfigManager.PWD);
            DataTable tbl = data.GetDataTbl(@"select a.id,name,pwd,role_name from public.user as a
                                                    left join public.role as b on a.role_id=b.id where a.is_del=false");
            List<TestUser> userList = data.DataTableToList<TestUser>(tbl, delegate (DataRow dr) {
                return new TestUser()
                {
                    SubjectId = dr["id"].ToString().Trim(),
                    Username = dr["name"].ToString().Trim(),
                    Password = dr["pwd"].ToString().Trim(),
                    Claims = new List<Claim>
                    {
                        new Claim(JwtClaimTypes.Role, dr["role_name"].ToString().Trim())
                    }
                };
            });
            return userList;
        }
    }
}
