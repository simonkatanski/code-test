using Xunit;
using NSubstitute;
using RingbaLibs;
using System.Threading;
using System.Threading.Tasks;
using code_test;

namespace Tests
{
    public class ImplementMeServiceTest_DoWork
    {
        readonly ILogService _logService;

        public ImplementMeServiceTest_DoWork()
        {
            var logService = Substitute.For<RingbaLibs.ILogService>();

            logService.LogAsync(Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<LOG_LEVEL>(),
                Arg.Any<object[]>())
            .Returns(Task.CompletedTask);

            _logService = logService;
        }

        [Fact]
        public void SampleTestOk()
        {
            //this is a fake test to show some general usage
            var service = new ImplementMeService(Substitute.For<IKVRepository>(),
                _logService,
                Substitute.For<IMessageProcessService>(),
                Substitute.For<IMessageQueService>());
            service.DoWork();
            Assert.True(true);

        }

        [Fact]
        public void SampleTesFail()
        {
            //this is a fake test to show some general usage
            var service = new ImplementMeService(Substitute.For<IKVRepository>(),
                _logService,
                Substitute.For<IMessageProcessService>(),
                Substitute.For<IMessageQueService>());
            service.DoWork();
            Assert.True(false);

        }


    }
}