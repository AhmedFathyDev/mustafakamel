﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Admin.Utilities;
using Portfolio.CurrentUser.Repositories;
using Portfolio.User.DTOs;
using Portfolio.User.Repositories;
using Portfolio.Utilities;

namespace Portfolio.User.Controllers;

public class UserController : ApiControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserRepository _currentUserRepository;

    public UserController(IUserRepository userRepository, ICurrentUserRepository currentUserRepository)
    {
        _userRepository = userRepository;
        _currentUserRepository = currentUserRepository;
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpPost]
    [ProducesResponseType(typeof(Response), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(IEnumerable<Error>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] UserDto userDto) =>
        ResponseToIActionResult(await _userRepository.CreateAsync(userDto));

    [HttpPost]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(IEnumerable<Error>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(IEnumerable<Error>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SignIn([FromBody] UserDto userDto) =>
        ResponseToIActionResult(await _userRepository.SignInAsync(userDto));

    [HttpGet]
    [ProducesResponseType(typeof(Response), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(IEnumerable<Error>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(IEnumerable<Error>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConfirmEmail([FromQuery] string userEmail, [FromQuery] string token) =>
        ResponseToIActionResult(await _userRepository.ConfirmEmailAsync(userEmail, token));

    [HttpGet]
    [ProducesResponseType(typeof(Response), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(IEnumerable<Error>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(IEnumerable<Error>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResendConfirmationEmail([FromQuery] string userEmail) =>
        ResponseToIActionResult(await _userRepository.SendConfirmationEmailAsync(userEmail));

    [Authorize]
    [HttpDelete]
    [ProducesResponseType(typeof(Response), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(IEnumerable<Error>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(IEnumerable<Error>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Delete([FromBody] string password) =>
        ResponseToIActionResult(await _userRepository.DeleteAsync(
            new UserDto { Email = _currentUserRepository.UserEmail, Password = password }));
}