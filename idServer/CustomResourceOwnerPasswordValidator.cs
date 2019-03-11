using IdentityModel;
using IdentityServer4.Models;
using IdentityServer4.Test;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GalaxyAPI.V01
{
    public class CustomResourceOwnerPasswordValidator : IResourceOwnerPasswordValidator
    {
        private readonly TestUserStore _users;
        private readonly ISystemClock _clock;

        public CustomResourceOwnerPasswordValidator(TestUserStore users, ISystemClock clock)
        {
            _users = users;
            _clock = clock;
        }
        public Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {
            //此处使用context.UserName, context.Password 用户名和密码来与数据库的数据做校验
            if (_users.ValidateCredentials(context.UserName, context.Password))
            {
                var user = _users.FindByUsername(context.UserName);

                //验证通过返回结果 
                //subjectId 为用户唯一标识 一般为用户id
                //authenticationMethod 描述自定义授权类型的认证方法 
                //authTime 授权时间
                //claims 需要返回的用户身份信息单元 此处应该根据我们从数据库读取到的用户信息 添加Claims 如果是从数据库中读取角色信息，那么我们应该在此处添加 此处只返回必要的Claim
                context.Result = new GrantValidationResult(
                    user.SubjectId ?? throw new ArgumentException("Subject ID not set", nameof(user.SubjectId)),
                    OidcConstants.AuthenticationMethods.Password, _clock.UtcNow.UtcDateTime,
                    user.Claims);
            }
            else
            {
                //验证失败
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, "invalid custom credential");
            }
            return Task.CompletedTask;
        }
    }
}
