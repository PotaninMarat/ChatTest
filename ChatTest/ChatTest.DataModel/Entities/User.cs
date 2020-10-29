using Microsoft.AspNetCore.Identity;

namespace ChatTest.DataModel.Entities
{
    public class User : IdentityUser
    {
        public string Name { get; set; }
    }
}
