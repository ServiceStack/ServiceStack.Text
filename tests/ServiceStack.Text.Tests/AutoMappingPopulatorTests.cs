using NUnit.Framework;

namespace ServiceStack.Text.Tests
{
    public class AutoMappingPopulatorTests
    {
        private static User CreateUser() =>
            new User {
                FirstName = "John",
                LastName = "Doe",
                Car = new Car {Name = "BMW X6", Age = 3}
            };

        private static UsersData CreateUserData()
        {
            var user = CreateUser();
            return new UsersData {
                Id = 1,
                User = user,
                UsersList = {user},
                UsersMap = {{1, user}}
            };
        }

        [Test]
        public void Does_call_populator_for_PopulateWith()
        {
            AutoMapping.RegisterPopulator((UserDto target, User source) => 
                target.LastName += "?!");

            var user = CreateUser();
            var dtoUser = new UserDto().PopulateWith(user);
            
            Assert.That(dtoUser.FirstName, Is.EqualTo(user.FirstName));
            Assert.That(dtoUser.LastName, Is.EqualTo(user.LastName + "?!"));
        }

        [Test]
        public void Does_call_populator_for_PopulateWithNonDefaultValues()
        {
            AutoMapping.RegisterPopulator((UserDto target, User source) => 
                target.LastName += "?!");

            var user = CreateUser();
            var dtoUser = new UserDto().PopulateWithNonDefaultValues(user);
            
            Assert.That(dtoUser.FirstName, Is.EqualTo(user.FirstName));
            Assert.That(dtoUser.LastName, Is.EqualTo(user.LastName + "?!"));
        }

        [Test]
        public void Does_call_populator_for_PopulateFromPropertiesWithoutAttribute()
        {
            AutoMapping.RegisterPopulator((UserDto target, User source) => 
                target.LastName += "?!");

            var user = CreateUser();
            var dtoUser = new UserDto().PopulateFromPropertiesWithoutAttribute(user, typeof(IgnoreAttribute));
            
            Assert.That(dtoUser.FirstName, Is.EqualTo(user.FirstName));
            Assert.That(dtoUser.LastName, Is.EqualTo(user.LastName + "?!"));
        }

        [Test]
        public void Does_call_populator_for_ConvertTo()
        {
            AutoMapping.RegisterPopulator((UserDto target, User source) => 
                target.LastName += "?!");

            var user = CreateUser();
            var dtoUser = user.ConvertTo<UserDto>();
            
            Assert.That(dtoUser.FirstName, Is.EqualTo(user.FirstName));
            Assert.That(dtoUser.LastName, Is.EqualTo(user.LastName + "?!"));
        }
    }
}