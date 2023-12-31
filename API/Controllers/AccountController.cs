﻿using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using API.Data;
using API.DTO;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class AccountController : BaseApiController
{
    private readonly DataContext _context;

    private readonly ITokenService _tokenService;

    public AccountController(DataContext context, ITokenService tokenService) 
    {
        _context = context;
        _tokenService = tokenService;
    }

    [HttpPost("register")] // api/account/register
    public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
    {
        if (await UserExists(registerDto.Username))
        {
            return BadRequest("Username is taken");
        }

        using var hmac = new HMACSHA512();

        var user = new AppUser
        {
            UserName = registerDto.Username,
            PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
            PasswordSalt = hmac.Key
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return new UserDto
        {
            Username = user.UserName,
            Token = _tokenService.CreateToken(user)
        };
    }

    [HttpPost("login")] // api/account/login
    public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
    {
        var fetchedUser = await _context.Users.SingleOrDefaultAsync( user => user.UserName == loginDto.Username);

        if (fetchedUser == null)
        {
            return Unauthorized("Invalid login credentials");
        }

        using var hmac = new HMACSHA512(fetchedUser.PasswordSalt);

        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

        for (int i = 0; i < computedHash.Length; ++i) 
        {
            if (computedHash[i] != fetchedUser.PasswordHash[i])
            {
                return Unauthorized("Invalid login credentials");
            }
        }

        return new UserDto
        {
            Username = fetchedUser.UserName,
            Token = _tokenService.CreateToken(fetchedUser)
        };
    }

    private async Task<bool> UserExists(string username)
    {
        return await _context.Users.AnyAsync(user => user.UserName.ToLower() == username.ToLower());
    }
}
