using ChatTest.DataBusiness.Models;
using ChatTest.DataModel;
using ChatTest.DataModel.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatTest.DataBusiness.Services
{
    public class AccountService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        public AccountService(UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }
        public async Task<LoginResponseModel> LoginAsync(LoginModel model)
        {
            var user = new User {
                UserName = model.Login,
                Email="test@mail.ru"
            };

            try
            {
                var result = await _signInManager.PasswordSignInAsync(model.Login, model.Password, false, false);

                if (!result.Succeeded)
                    return new LoginResponseModel();

                var userSaved = await _userManager.FindByNameAsync(model.Login);

                return new LoginResponseModel
                {
                    Succeeded = result.Succeeded,
                    Name = userSaved.Name
                };
            }catch(Exception e)
            {

            }
            return null;
        }

        public LoginResponseModel CheckPassword(LoginModel model)
        {
            var user = _userManager.Users.SingleOrDefault(p => p.UserName == model.Login);
            if (user == null)
                return new LoginResponseModel();
            var result = _userManager.PasswordHasher.VerifyHashedPassword(user, user.PasswordHash, model.Password);

            if (result != PasswordVerificationResult.Success)
                return new LoginResponseModel();

            return new LoginResponseModel
            {
                Name = user.Name,
                Succeeded = true
            };
        }

        public async Task<IdentityResult> RegisterAsync(RegisterModel model)
        {
            var user = new User
            {
                UserName = model.Login,
                Name = model.Name
            };

            return await _userManager.CreateAsync(user, model.Password);
            
        }

        public async Task LogoutAsync()
        {
            // delete cookie
            await _signInManager.SignOutAsync();
        }
    }
}
