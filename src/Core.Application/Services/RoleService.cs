﻿using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Core.Application.Contracts.DataTransferObjects;
using Core.Application.Contracts.Interfaces;
using Core.Application.Contracts.Permissions;
using Core.Application.Contracts.Response;
using Core.Domain.Identity.Contracts;
using Core.Domain.Identity.Entities;
using Core.Domain.Identity.Response;
using Microsoft.EntityFrameworkCore;

namespace Core.Application.Services
{
    public class RoleService : IRoleService
    {
        private readonly IApplicationRoleManager _roleManager;
        private readonly IMapper _mapper;

        public RoleService(IApplicationRoleManager roleManager, IMapper mapper)
        {
            _roleManager = roleManager;
            _mapper = mapper;
        }

        public async Task<PaginatedList<RoleDto>> GetPaginatedRolesAsync(int? pageNumber, int? pageSize)
        {
            var configuration = new MapperConfiguration(cfg =>
                cfg.CreateMap<ApplicationRole, RoleDto>());
            var roles = _roleManager.Roles().ProjectTo<RoleDto>(configuration);
            return await PaginatedList<RoleDto>.CreateAsync(roles.AsNoTracking(),
                pageNumber ?? 1, pageSize ?? 12);
        }

        public async Task<Response<string>> AddRoleAsync(AddRoleDto addRoleDto)
        {
            if (await _roleManager.GetRoleAsync(addRoleDto.Name) != null)
                return Response<string>.Fail("The role already exists. Please try a different one!");
            var appRole = _mapper.Map<ApplicationRole>(addRoleDto);
            var rs = await _roleManager.AddRoleAsync(appRole);
            return rs.Succeeded
                ? Response<string>.Success(appRole.Id, "New role has been created")
                : Response<string>.Fail("Failed to create new role");
        }

        public async Task<Response<ManageRolePermissionsDto>> ManagePermissionsAsync(string roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null) return Response<ManageRolePermissionsDto>.Fail("No Role Exists");
            var roleClaims = await _roleManager.GetClaimsAsync(role);
            var allPermissions = PermissionHelper.GetAllPermissions();
            foreach (var permission in allPermissions)
            {
                if (roleClaims.Any(x => x.Value == permission.Value))
                {
                    permission.Checked = true;
                }
            }

            var manageRolePermissionsDto = new ManageRolePermissionsDto
            {
                RoleId = roleId,
                RoleName = role.Name,
                ManagePermissionsDto = allPermissions
            };
            return allPermissions.Count > 0
                ? Response<ManageRolePermissionsDto>.Success(manageRolePermissionsDto, "Successfully retrieved")
                : Response<ManageRolePermissionsDto>.Fail(
                    $"No Permissions exists! Something is Wrong with {typeof(Permissions).Namespace} file");
        }

        public async Task<Response<RoleIdentityDto>> ManagePermissionsAsync(ManageRolePermissionsDto manageRolePermissionsDto)
        {
            var role = await _roleManager.FindByIdAsync(manageRolePermissionsDto.RoleId);
            if (role == null)
                return Response<RoleIdentityDto>.Fail("No role exists by this id");

            var existingClaims = await _roleManager.GetClaimsAsync(role);
            var existingPermissions = existingClaims.Where(x => x.Type == CustomClaimTypes.Permission).ToList();
            foreach (var existingPermission in existingPermissions)
            {
                var removeResult = await _roleManager.RemoveClaimAsync(role, existingPermission);
                if (!removeResult.Succeeded)
                    return Response<RoleIdentityDto>.Fail("Failed to remove some Permissions");
            }

            var newClaims = manageRolePermissionsDto.ManagePermissionsDto.Where(x => x.Checked)
                .Select(x => new Claim(CustomClaimTypes.Permission, x.Value)).ToList();
            var identityResponse = new IdentityResponse();
            foreach (var newClaim in newClaims)
            {
                var rs = await _roleManager.AddClaimAsync(role, newClaim);
                if(!rs.Succeeded)
                    return Response<RoleIdentityDto>.Fail("Failed to add some permission to this role");
                identityResponse = rs;
            }

            return Response<RoleIdentityDto>.Success(new RoleIdentityDto { RoleId = manageRolePermissionsDto.RoleId},
                identityResponse.Message); ;
        }
    }
}