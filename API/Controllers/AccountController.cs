using API.Data;
using API.DTOs;
using API.Entities;
using API.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace API.Controllers
{
    
    public class AccountController : BaseController
    {
        private readonly DataContext _context;
        private readonly ITokenService _tokenService;

        public AccountController(DataContext context, ITokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto request)
        {
            if (await UserExist(request.UserName)) return BadRequest("User exists");

            using var hmac = new HMACSHA512();

            var user = new AppUser
            {
                UserName = request.UserName.ToLower(),
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(request.Password)),
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

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto request)
        {
            var user = await _context.Users.SingleAsync(x => x.UserName == request.UserName);

            if (user == null) return Unauthorized("User name is invalid");

            var hmac = new HMACSHA512(user.PasswordSalt);

            var hashedPassword = hmac.ComputeHash(Encoding.UTF8.GetBytes(request.Password));

            for(int i=0; i<hashedPassword.Length; i++)
            {
                if (hashedPassword[i] != user.PasswordHash[i]) return Unauthorized("password is invalid");
            }

            return new UserDto
            {
                Username = user.UserName,
                Token = _tokenService.CreateToken(user)
            };

        }

        private async Task<bool> UserExist(string username)
        {
            return await _context.Users.AnyAsync(x=> x.UserName == username.ToLower());
        }
    }
}
