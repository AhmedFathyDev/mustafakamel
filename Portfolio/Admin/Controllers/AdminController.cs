﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Admin.Repositories;
using Portfolio.Admin.Utilities;
using Portfolio.Utilities;

namespace Portfolio.Admin.Controllers;

[Authorize(Roles = Roles.Admin)]
public class AdminController : ApiControllerBase
{
    private readonly IAdminRepository _adminRepository;

    public AdminController(IAdminRepository adminRepository)
    {
        _adminRepository = adminRepository;
    }

    [HttpGet]
    [ProducesResponseType(typeof(Response), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(IEnumerable<Error>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateAdminRole() =>
        ResponseToIActionResult(await _adminRepository.CreateRoleAsync(Roles.Admin));

    [HttpPost]
    [ProducesResponseType(typeof(Response), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(IEnumerable<Error>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(IEnumerable<Error>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddToRoleAdmin([FromQuery] string userEmail) =>
        ResponseToIActionResult(await _adminRepository.AddToRoleAsync(userEmail, Roles.Admin));
}