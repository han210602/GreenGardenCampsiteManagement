﻿//using BusinessObject.Models;
using AutoMapper;
using BusinessObject.DTOs;
using BusinessObject.Models;
using DataAccess.DAO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Repositories.Accounts;
using System.Text.Json;

namespace GreenGardenCampsiteManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private IAccountRepository _repo;
        private IMapper _mapper;
        public AccountController(IAccountRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        [HttpGet("GetAllAccounts")]
        public IActionResult GetAllAccounts()
        {
            try
            {

                var user = _repo.GetAllAccount().ToList();
                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("Login")]
        public IActionResult Login([FromBody] AccountDTO loginRequest)
        {
            try
            {
                var loginResponse = _repo.Login(loginRequest);


                if (loginResponse == null)
                {
                    return Unauthorized("Invalid email or password.");
                }


                return Ok(loginResponse);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        [HttpPost("SendVerificationCode")]
        public async Task<IActionResult> SendVerificationCode([FromBody] string email)
        {
            try
            {
                var verificationCodeMessage = await _repo.SendVerificationCode(email);



                return Ok(verificationCodeMessage);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] Register registerRequest, string enteredCode)
        {
            try
            {

                var registerResponse = await _repo.Register(registerRequest, enteredCode);


                if (registerResponse == null)
                {
                    return BadRequest("Registration failed.");
                }


                return Ok(registerResponse);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }


        [HttpPost("SendResetPasswordEmail/{email}")]
        public async Task<IActionResult> SendResetPasswordEmail(string email)
        {
            try
            {
                var response = await _repo.SendResetPassword(email);
                return Content(response, "application/json");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, new { Message = ex.Message });
            }
        }

        [HttpPut("UpdateProfile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfile updateProfile)
        {
            if (updateProfile == null)
            {
                return BadRequest("Invalid data.");
            }

            if (updateProfile.UserId <= 0)
            {
                return BadRequest("Invalid user ID.");
            }

            try
            {
                var message = await _repo.UpdateProfile(updateProfile);

                if (message == "Profile updated successfully.")
                {
                    return Ok(message);
                }
                else
                {
                    return NotFound(message);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("ChangePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePassword changePasswordDto)
        {
            if (changePasswordDto == null)
            {
                return BadRequest("Invalid data.");
            }

            try
            {
                var message = await _repo.ChangePassword(changePasswordDto);

                if (message == "Password updated successfully.")
                {
                    return Ok(message);
                }
                else
                {
                    return BadRequest(message);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}