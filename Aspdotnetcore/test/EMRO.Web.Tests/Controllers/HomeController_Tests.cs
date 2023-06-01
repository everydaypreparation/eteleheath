using System.Threading.Tasks;
using EMRO.Models.TokenAuth;
using EMRO.Web.Controllers;
using Shouldly;
using Xunit;

namespace EMRO.Web.Tests.Controllers
{
    public class HomeController_Tests: EMROWebTestBase
    {
        [Fact]
        public async Task Index_Test()
        {
            await AuthenticateAsync(null, new AuthenticateModel
            {
                UserNameOrEmailAddress = "admin",
                Password = "123qwe"
            });

            //Act
            var response = await GetResponseAsStringAsync(
                GetUrl<HomeController>(nameof(HomeController.Index))
            );

            //Assert
            response.ShouldNotBeNullOrEmpty();
        }
    }
}