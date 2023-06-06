using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
namespace Test2
{
    
    public class AuthToken
    {
        public String Token { get; set; }
        public DateTime ExpiresAt { get; set; }
    }

    public interface IAuthenticationService
    {
        AuthToken Authenticate(String username, String password);
    }

    public interface IUser
    {
        String AuthToken { get; }
    }

    class User : IUser
    {
        private readonly IAuthenticationService _authService;
        private AuthToken _authToken;

        public User(IAuthenticationService authService)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        }

        public string AuthToken => _authToken?.Token;

        public void Authenticate(string username, string password)
        {
            _authToken = _authService.Authenticate(username, password);
        }
    }
    [TestFixture]
    public class UserTests
    {
        [Test]
        public void Authenticate_WithValidCredentials_SetsAuthToken()
        {
            // Arrange
            var authServiceMock = new Mock<IAuthenticationService>();
            authServiceMock.Setup(a => a.Authenticate("username", "password"))
                .Returns(new AuthToken { Token = "token" });
            var user = new User(authServiceMock.Object);

            // Act
            user.Authenticate("username", "password");

            // Assert
            Assert.AreEqual("token", user.AuthToken);
        }
    }

}

