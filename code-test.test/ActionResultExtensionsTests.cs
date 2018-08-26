using code_test.Extensions;
using RingbaLibs.Models;
using Xunit;

namespace Tests
{
    public class ActionResultExtensionsTests
    {
        [Fact]
        public void GivenSuccessfullActionResult_ThenExpectSuccessfulShortStatusInfoMessage()
        {
            //arrange
            string expectedMessageStatus = "IsSuccessful: True";
            var testActionResult = new ActionResult { IsSuccessfull = true };

            //act
            var resultMessageStatus = testActionResult.GetStatusInfo();

            //assert            
            Assert.Equal(expectedMessageStatus, resultMessageStatus);
        }

        [Fact]
        public void GivenUnsuccessfullActionResult_ThenExpectUnsuccessfulMessageToContainErrorCodeAndMessage()
        {
            //arrange
            string expectedMessageStatus = "IsSuccessful: False, Error code: 1, Error message: TestErrorMessage";
            var testActionResult = new ActionResult { IsSuccessfull = false, ErrorCode = 1, ErrorMessage = "TestErrorMessage" };

            //act
            var resultMessageStatus = testActionResult.GetStatusInfo();

            //assert            
            Assert.Equal(expectedMessageStatus, resultMessageStatus);
        }
    }
}
