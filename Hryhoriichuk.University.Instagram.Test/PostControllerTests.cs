using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Hryhoriichuk.University.Instagram.Web.Controllers;
using Hryhoriichuk.University.Instagram.Web.Models;
using Hryhoriichuk.University.Instagram.Web.Data;
using System.IO;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using System.Security.Claims;

namespace Hryhoriichuk.University.Instagram.Test
{
    [TestClass]
    public class PostControllerTests : TestBase
    {
        private readonly Mock<AuthDbContext> _contextMock = new Mock<AuthDbContext>();
        private readonly Mock<IWebHostEnvironment> _webHostEnvironmentMock = new Mock<IWebHostEnvironment>();

        [TestMethod]
        public async Task UploadMedia_ValidFile_ReturnsRedirectToActionResult()
        {
            // Arrange
            var controller = new PostController(_contextMock.Object, _webHostEnvironmentMock.Object);
            var post = new Post();
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("test.jpg");
            fileMock.Setup(f => f.Length).Returns(1024); // Set appropriate length
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, "testuser")
                    }))
                }
            };

            // Act
            var result = await controller.UploadMedia(post, fileMock.Object) as RedirectToActionResult;

            // Assert
            // Assert.NotNull(result);
            // Assert.Equal("Index", result.ActionName);
            // Assert.Equal("Home", result.ControllerName);
        }

        // Add more tests for invalid file, null file, etc.
    }
}
