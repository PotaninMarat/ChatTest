using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChatTest.DataBusiness.Models;
using ChatTest.DataBusiness.Services;
using Microsoft.AspNetCore.Mvc;

namespace ChatTest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : Controller
    {
        private readonly AccountService _accountService;
        public AccountController(AccountService accountService)
        {
            _accountService = accountService;
        }

        [Route("Login")]
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var result = await _accountService.LoginAsync(model);
            return Ok(result);
        }

        [Route("Register")]
        [HttpPost]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            var result = await _accountService.RegisterAsync(model);
            return Ok(result);
        }

        [Route("Logout")]
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await _accountService.LogoutAsync();
            return Ok();
        }
    }
}
