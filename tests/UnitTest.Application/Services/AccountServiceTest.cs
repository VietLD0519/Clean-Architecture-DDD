﻿using AutoMapper;
using Core.Application.Contracts.AutomapperProfiles;
using Core.Application.Contracts.DataTransferObjects;
using Core.Application.Contracts.Interfaces;
using Core.Application.Contracts.Permissions;
using Core.Application.Services;
using Core.Domain.Identity.Constants;
using Core.Domain.Identity.Contracts;
using Core.Domain.Identity.Entities;
using Core.Domain.Identity.Response;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace UnitTest.Application.Services
{
    [TestFixture]
    public class AccountServiceTest
    {
        private IAccountService _accountService;

        [OneTimeSetUp]
        public void SetUp()
        {
            var mappingConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new AccountServiceProfile());
            });
            var mapper = mappingConfig.CreateMapper();

            var applicationUserManager = new Mock<IApplicationUserManager>(MockBehavior.Strict);
            applicationUserManager.Setup(x => x.RegisterUserAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(IdentityResponse.Success("Succeeded"));
            applicationUserManager.Setup(x => x.GetUserByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(DefaultApplicationUsers.GetSuperUser());
            applicationUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(DefaultApplicationUsers.GetSuperUser);
            applicationUserManager.Setup(x => x.GetClaimsAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(new List<Claim>() {new (CustomClaimTypes.Permission, Permissions.Posts.View)});
            applicationUserManager.Setup(x => x.GetRolesAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(DefaultApplicationRoles.GetDefaultRoles().Select(x => x.Name).ToList());

            var applicationSignInManager = new Mock<IApplicationSignInManager>(MockBehavior.Strict);
            applicationSignInManager.Setup(x =>
                    x.PasswordSignInAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(IdentityResponse.Success("Succeeded"));
            applicationSignInManager.Setup(x => x.SignOutAsync());

            var applicationRoleManager = new Mock<IApplicationRoleManager>(MockBehavior.Strict);
            applicationRoleManager.Setup(x => x.GetClaimsAsync(It.IsAny<List<string>>()))
                .ReturnsAsync(PermissionHelper.GetPermissionClaims());

            _accountService = new AccountService(applicationUserManager.Object, applicationSignInManager.Object,
                applicationRoleManager.Object, mapper);
        }

        [Test]
        public async Task RegisterUserAsyncTest()
        {
            var registerUserDto = new RegisterUserDto
            {
                FirstName = DefaultApplicationUsers.GetSuperUser().FirstName,
                LastName = DefaultApplicationUsers.GetSuperUser().LastName,
                Email = DefaultApplicationUsers.GetSuperUser().Email,
                Password = "SuperAdmin",
                ConfirmPassword = "SuperAdmin"
            };
            var rs = await _accountService.RegisterUserAsync(registerUserDto);
            Assert.AreEqual(true, rs.Succeeded);
        }

        [Test]
        public async Task CookieSignInAsyncTest()
        {
            var loginUserDto = new LoginUserDto
            {
                UserName = DefaultApplicationUsers.GetSuperUser().UserName,
                Password = "SuperAdmin",
                RememberMe = true
            };
            var rs = await _accountService.CookieSignInAsync(loginUserDto);
            Assert.AreEqual(true, rs.Succeeded);
        }

        [Test]
        public async Task GetAllClaimsTest()
        {
            var claimPrincipal =
                new ClaimsPrincipal(new ClaimsIdentity(PermissionHelper.GetPermissionClaims(),
                    "AuthScheme"));
            var rs = await _accountService.GetAllClaims(claimPrincipal);
            Assert.AreEqual(true, rs.Succeeded);
            Assert.GreaterOrEqual(rs.Data.Count, Decimal.ToInt32(1));
        }

        [Test]
        public async Task GetRolesAsyncAsync()
        {
            var claimPrincipal =
                new ClaimsPrincipal(new ClaimsIdentity(PermissionHelper.GetPermissionClaims(),
                    "AuthScheme"));
            var rs = await _accountService.GetRolesAsync(claimPrincipal);
            Assert.AreEqual(true, rs.Succeeded);
            Assert.AreEqual(4, rs.Data.Count);
        }
    }
}
