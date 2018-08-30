using code_test;
using code_test.ExisitingInterfaces.Models;
using NSubstitute;
using RingbaLibs;
using RingbaLibs.Models;
using System;
using System.Threading.Tasks;
using Xunit;

namespace tests
{
    public class UOWStatusServiceTests
    {
        private ILogService _logService;

        public UOWStatusServiceTests()
        {
            _logService = Substitute.For<ILogService>();
            _logService.LogAsync(Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<LOG_LEVEL>(),
                Arg.Any<object[]>())
            .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task GivenUowStatusInKV_WhenInProcessingReset_ThenNewEntryAddedOldEntryDeletedInKV()
        {
            //arrange            
            const string testUowId = "testId";
            var subKVRepository = Substitute.For<IKVRepository>();

            var testResult = new Result<UowStatus>
            {
                IsSuccessfull = true,
                Item = new UowStatus(3, true)
            };
            
            subKVRepository.GetAsync<UowStatus>(Arg.Is<string>(uowId => uowId == testUowId))
                .Returns(Task.FromResult(testResult));
            subKVRepository.DeleteAsync(Arg.Is<string>(uowId => uowId == testUowId))
                .Returns(new ActionResult());
            subKVRepository.CreateAsync(Arg.Is<CreateKVRequest<UowStatus>>(createKVRequest =>
                createKVRequest.Item.IsInProcessing == false &&
                createKVRequest.Item.CurrentRetrialCount == 3 &&
                createKVRequest.Key == testUowId))
            .Returns(new ActionResult());

            //act
            var uowStatusService = new UOWStatusService(subKVRepository, _logService);
            await uowStatusService.ResetInProcessing(testUowId);

            //assert
            await subKVRepository.Received().DeleteAsync(Arg.Is<string>(uowId => uowId == testUowId));            

            await subKVRepository.Received().CreateAsync(Arg.Is<CreateKVRequest<UowStatus>>(createKVRequest =>
                createKVRequest.Item.IsInProcessing == false &&
                createKVRequest.Item.CurrentRetrialCount == 3 &&
                createKVRequest.Key == testUowId));                       
        }

        [Fact]
        public async Task GivenUowStatusNotInKV_WhenInProcessingReset_ThenExpectException()
        {
            //arrange            
            const string testUowId = "testId";
            var subKVRepository = Substitute.For<IKVRepository>();

            var testResult = new Result<UowStatus>
            {
                IsSuccessfull = false,
                ErrorCode = 1,
                ErrorMessage = "Error message"                
            };

            subKVRepository.GetAsync<UowStatus>(Arg.Is<string>(uowId => uowId == testUowId))
                .Returns(Task.FromResult(testResult));
            
            //act and assert
            var uowStatusService = new UOWStatusService(subKVRepository, _logService);
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => uowStatusService.ResetInProcessing(testUowId));
            Assert.True(exception.Message.Contains(testUowId));
        }

        [Theory]
        [InlineData(true, true, true)]
        [InlineData(true, false, false)]
        [InlineData(false, true, false)]
        [InlineData(false, false, false)]        
        public async Task GivenStatusInKV_WhenInProcessingCheck_ThenExpectAppropriateResult(bool isSuccessful, bool isInProcessing, bool expectedResult)
        {
            //arrange
            const string testUowId = "testId";
            var subKVRepository = Substitute.For<IKVRepository>();

            var testResult = new Result<UowStatus>
            {
                IsSuccessfull = isSuccessful,
                Item = new UowStatus(3, isInProcessing)
            };

            subKVRepository.GetAsync<UowStatus>(Arg.Is<string>(uowId => uowId == testUowId))
                .Returns(Task.FromResult(testResult));

            //act
            var uowStatusService = new UOWStatusService(subKVRepository, _logService);
            var isInProcessingResult = await uowStatusService.IsInProcessing(testUowId);

            //assert
            Assert.True(isInProcessingResult == expectedResult);
        }
        
        [Fact]
        public async Task GivenStatusInKV_AndInfiniteRetries_ThenExpectHasExpiredRetriesFalse()
        {
            //act
            var uowStatusService = new UOWStatusService(null, null);
            var hasExpiredResult = await uowStatusService.HasExpiredAge("testId", creationEPOCH: 1, maxAgeInSeconds: -1);

            //assert
            Assert.True(hasExpiredResult == false);
        }
    }
}
