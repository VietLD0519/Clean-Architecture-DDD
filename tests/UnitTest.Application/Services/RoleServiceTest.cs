﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
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

namespace UnitTest.Application.Services
{
    [TestFixture]
    public class RoleServiceTest
    {
        private IRoleService _roleService;

        [OneTimeSetUp]
        public void Setup()
        {
            var mappingConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new RoleServiceProfile());
            });
            var mapper = mappingConfig.CreateMapper();

            var applicationRoleManager = new Mock<IApplicationRoleManager>(MockBehavior.Strict);
            applicationRoleManager.Setup(x => x.Roles())
                .Returns(DefaultApplicationRoles.GetDefaultRoles().AsQueryable());
            applicationRoleManager.Setup(x => x.GetRoleAsync(It.IsAny<string>()))
                .ReturnsAsync((ApplicationRole)null);
            applicationRoleManager.Setup(x => x.AddRoleAsync(It.IsAny<ApplicationRole>()))
                .ReturnsAsync(IdentityResponse.Success("Succeeded"));
            applicationRoleManager.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new ApplicationRole(DefaultApplicationRoles.SuperAdmin));
            applicationRoleManager.Setup(x => x.GetClaimsAsync(It.IsAny<List<string>>()))
                .ReturnsAsync(PermissionHelper.GetPermissionClaims());
            applicationRoleManager.Setup(x => x.GetClaimsAsync(It.IsAny<ApplicationRole>()))
                .ReturnsAsync(PermissionHelper.GetPermissionClaims());
            applicationRoleManager.Setup(x => x.RemoveClaimAsync(It.IsAny<ApplicationRole>(), It.IsAny<Claim>()))
                .ReturnsAsync(IdentityResponse.Success("Succeeded"));
            applicationRoleManager.Setup(x => x.AddClaimAsync(It.IsAny<ApplicationRole>(), It.IsAny<Claim>()))
                .ReturnsAsync(IdentityResponse.Success("Succeeded"));
            _roleService = new RoleService(applicationRoleManager.Object, mapper);
        }

        [Test]
        public async Task AddRoleAsyncTest()
        {
            var addRoleDto = new AddRoleDto
            {
                Name = DefaultApplicationRoles.Basic
            };
            var rs = await _roleService.AddRoleAsync(addRoleDto);
            Assert.AreEqual(true, rs.Succeeded);
        }

        [Test]
        public async Task ManagePermissionsAsyncTest()
        {
            var roleId = "roleId";
            var rs = await _roleService.ManagePermissionsAsync(roleId);
            Assert.AreEqual(true, rs.Succeeded);
            Assert.GreaterOrEqual(rs.Data.ManagePermissionsDto.Count, Decimal.ToUInt16(1));
        }
    }
}