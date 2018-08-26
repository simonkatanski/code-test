using code_test.Extensions;
using NSubstitute;
using RingbaLibs;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Tests
{
    public class LogServiceExtensionsTests
    {
        [Theory]
        [InlineData("testLogMessage", "Error occurred", "System.InvalidOperationException: Error occurred")]
        [InlineData("", "Error occurred", "System.InvalidOperationException: Error occurred")]
        [InlineData(null, "Error occurred", "System.InvalidOperationException: Error occurred")]
        [InlineData(null, "", "System.InvalidOperationException")]
        public async Task GivenLoggingException_ThenLogAccordinglyBasedOnParameters(string testLogMessage, string testExceptionMessage, string expectedExceptionMessage)
        {
            //arrange
            var logService = Substitute.For<ILogService>();
            logService.LogAsync(Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<LOG_LEVEL>(),
                Arg.Any<object[]>())
            .Returns(Task.CompletedTask);

            var testException = new InvalidOperationException(testExceptionMessage);

            //act
            await LogServiceExtensions.LogExceptionAsync(logService, "testLogId", testException, testLogMessage);

            //assert            
            await logService.Received().LogAsync(
                    Arg.Any<string>(),
                    Arg.Is<string>(exceptionMessage => exceptionMessage == $"{testLogMessage}{Environment.NewLine}{expectedExceptionMessage}"),
                    Arg.Is<LOG_LEVEL>(logLevel => logLevel == LOG_LEVEL.EXCEPTION),
                    Arg.Any<object[]>());
        }
    }
}
