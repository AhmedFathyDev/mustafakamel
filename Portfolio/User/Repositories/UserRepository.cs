﻿using System.Net.Mail;
using System.Web;
using Microsoft.AspNetCore.Identity;
using Portfolio.Configurations;
using Portfolio.Email.Repositories;
using Portfolio.JWTGenerator.Repositories;
using Portfolio.User.DTOs;
using Portfolio.Utilities;

namespace Portfolio.User.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IEmailRepository _emailRepository;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly IJwtGeneratorRepository _jwtGeneratorRepository;

    public UserRepository(IEmailRepository emailRepository, UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager, IJwtGeneratorRepository jwtGeneratorRepository)
    {
        _emailRepository = emailRepository;
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtGeneratorRepository = jwtGeneratorRepository;
    }

    public async Task<Response> CreateAsync(UserDto userDto)
    {
        var createResult = await _userManager.CreateAsync(
            new IdentityUser(userDto.Email) { Email = userDto.Email }, userDto.Password);
        return createResult.Succeeded
            ? await SendConfirmationEmailAsync(userDto.Email)
            : createResult.ToApplicationResponse();
    }

    public async Task<Response<string>> SignInAsync(UserDto userDto)
    {
        var user = await _userManager.FindByEmailAsync(userDto.Email);
        if (user is null) return ResponseFactory.Fail<string>(ErrorsList.UserEmailNotFound(userDto.Email));

        var passwordSignInResult = await PasswordSignInAsync(user, userDto.Password);
        return !passwordSignInResult.Succeeded
            ? ResponseFactory.Fail<string>(passwordSignInResult.Errors)
            : ResponseFactory.Ok(await _jwtGeneratorRepository.GenerateJwtSecurityToken(user));
    }

    private async Task<Response> PasswordSignInAsync(IdentityUser user, string password)
    {
        var passwordSignInAsyncResult = await _signInManager.PasswordSignInAsync(user, password, true, false);

        if (passwordSignInAsyncResult.IsNotAllowed)
            return ResponseFactory.Fail(ErrorsList.SignInNotAllowed());

        return passwordSignInAsyncResult.Succeeded == false
            ? ResponseFactory.Fail(ErrorsList.SignInFailed())
            : ResponseFactory.Ok();
    }

    public async Task<Response> SendConfirmationEmailAsync(string userEmail)
    {
        var user = await _userManager.FindByEmailAsync(userEmail);
        if (user is null) return ResponseFactory.Fail(ErrorsList.UserEmailNotFound(userEmail));

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var confirmEmailUri =
            $"{ConfigProvider.MailMessageConfirmEmailEndpoint}?userEmail={userEmail}&token={HttpUtility.UrlEncode(token)}";
        var body = "<h1>Welcome To Mustafa Kamel Portfolio</h1><br>" +
                   "<p> Thanks for registering please click " +
                   $"<strong><a href=\"{confirmEmailUri}\" target=\"_blank\">here</a></strong>" +
                   " to confirm your email</p>";
        var mailMessage = new MailMessage(ConfigProvider.SmtpClientGmailCredentialsUserName, userEmail,
            ConfigProvider.MailMessageSubject, body)
        {
            IsBodyHtml = true
        };

        return await _emailRepository.SendAsync(mailMessage);
    }

    public async Task<Response> ConfirmEmailAsync(string userEmail, string token)
    {
        var user = await _userManager.FindByEmailAsync(userEmail);
        return user is null
            ? ResponseFactory.Fail(ErrorsList.UserEmailNotFound(userEmail))
            : (await _userManager.ConfirmEmailAsync(user, token)).ToApplicationResponse();
    }

    public async Task<Response> DeleteAsync(UserDto userDto)
    {
        var user = await _userManager.FindByEmailAsync(userDto.Email);
        if (user is null) return ResponseFactory.Fail(ErrorsList.UserEmailNotFound(userDto.Email));

        var passwordSignInResult = await PasswordSignInAsync(user, userDto.Password);
        return !passwordSignInResult.Succeeded
            ? passwordSignInResult
            : (await _userManager.DeleteAsync(user)).ToApplicationResponse();
    }
}