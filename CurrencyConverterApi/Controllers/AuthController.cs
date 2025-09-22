using CurrencyConverterApi.Data;
using CurrencyConverterApi.Models;
using CurrencyConverterApi.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyConverterApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IEmailService _emailService;
        private readonly IOtpService _otpService;
        private readonly IConfiguration _config;
        private readonly PasswordHasher<User> _pwHasher;

        public AuthController(
            AppDbContext db,
            IEmailService emailService,
            IOtpService otpService,
            IConfiguration config)
        {
            _db = db;
            _emailService = emailService;
            _otpService = otpService;
            _config = config;
            _pwHasher = new PasswordHasher<User>();
        }

        /// <summary>
        /// Register (or noop if exists), hashing the password and sending an OTP.
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Email) ||
                string.IsNullOrWhiteSpace(req.Password))
                return BadRequest("Email and password are required.");

            // check or create user
            var user = await _db.Users
                                .SingleOrDefaultAsync(u => u.Email == req.Email);
            if (user == null)
            {
                user = new User { Email = req.Email };
                user.PasswordHash = _pwHasher.HashPassword(user, req.Password);
                _db.Users.Add(user);
            }
            else
            {
                // if they already exist, optionally update their password:
                user.PasswordHash = _pwHasher.HashPassword(user, req.Password);
                _db.Users.Update(user);
            }
            await _db.SaveChangesAsync();

            // send OTP
            var code = await _otpService.GenerateAndStoreOtpAsync(req.Email);
            await _emailService.SendEmailAsync(
                req.Email,
                "Your Registration OTP",
                $"<p>Your code is <b>{code}</b>. Expires in 5 minutes.</p>");

            return Ok("Registered (or updated) – OTP sent.");
        }

        /// <summary>
        /// Request (or re-request) an OTP for an existing user.
        /// </summary>
        [HttpPost("request-otp")]
        public async Task<IActionResult> RequestOtp([FromBody] OtpRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Email))
                return BadRequest("Email is required.");

            var exists = await _db.Users.AnyAsync(u => u.Email == req.Email);
            if (!exists)
                return NotFound("User not found; please register first.");

            var code = await _otpService.GenerateAndStoreOtpAsync(req.Email);
            await _emailService.SendEmailAsync(
                req.Email,
                "Your OTP Code",
                $"<p>Your code is <b>{code}</b>. Expires in 5 minutes.</p>");

            return Ok("OTP sent.");
        }

        /// <summary>
        /// Verify an OTP (no JWT issued).
        /// </summary>
        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] OtpVerifyRequest req)
        {
            if (!await _otpService.ValidateOtpAsync(req.Email, req.Code))
                return Unauthorized("Invalid or expired OTP.");
            return Ok("OTP is valid.");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest req)
        {
            // 1) Basic input validation
            if (string.IsNullOrWhiteSpace(req.Email) ||
                string.IsNullOrWhiteSpace(req.Password))
            {
                return BadRequest("Email and password are required.");
            }

            // 2) Lookup user
            var user = await _db.Users
                                .SingleOrDefaultAsync(u => u.Email == req.Email);
            if (user == null)
                return Unauthorized("Invalid credentials.");

            // 3) Verify password
            var pwResult = _pwHasher.VerifyHashedPassword(user, user.PasswordHash, req.Password);
            if (pwResult == PasswordVerificationResult.Failed)
                return Unauthorized("Invalid credentials.");

            // 4) Issue JWT
            var jwtCfg = _config.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtCfg["Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddMinutes(double.Parse(jwtCfg["ExpireMinutes"]));

            var token = new JwtSecurityToken(
                issuer: jwtCfg["Issuer"],
                audience: jwtCfg["Audience"],
                claims: new[] { new Claim(ClaimTypes.Name, req.Email) },
                expires: expires,
                signingCredentials: creds
            );

            return Ok(new AuthResponse
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                ExpiresAt = expires
            });
        }

    }
}
