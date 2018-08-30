using code_test;
using NSubstitute;
using RingbaLibs;
using RingbaLibs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

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

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task GivenOneMessageInQueue_WhenProcessed_ThenExpectOneMessageUpdated(bool isActionResultSuccessful)
        {
            //arrange
            var testBatch = new MessageBatchResult<RingbaUOW>
            {
                Messages = new List<MessageWrapper<RingbaUOW>> { new MessageWrapper<RingbaUOW> { Id = "1", Body = new RingbaUOW() } }
            };

            ImplementMeService service = null;

            var subMessageQueService = Substitute.For<IMessageQueService>();
            subMessageQueService.GetMessagesFromQueAsync<RingbaUOW>(
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<int>())
                .Returns(Task.FromResult(testBatch));
            subMessageQueService.UpdateMessagesAsync(Arg.Any<IEnumerable<UpdateBatchRequest>>())
                .Returns(Task.FromResult(new ActionResult()))
                .AndDoes(p => service.Stop());

            var subMessageProcessingService = Substitute.For<IMessageProcessService>();
            subMessageProcessingService.ProccessMessageAsync(Arg.Any<RingbaUOW>())
                .Returns(Task.FromResult(new ActionResult { IsSuccessfull = isActionResultSuccessful }));

            service = new ImplementMeService(
                Substitute.For<IUOWStatusService>(),
                _logService,
                subMessageProcessingService,
                subMessageQueService);

            //act
            await service.DoWork();

            //assert
            await subMessageQueService.Received().UpdateMessagesAsync(Arg.Is<IEnumerable<UpdateBatchRequest>>(p =>
            p.Count() == 1 &&
            p.First().Id == "1" &&
            p.First().MessageCompleted == isActionResultSuccessful));
        }

        [Fact]
        public async Task GivenOneMessageInQueue_WhenReceivedAndProcessingFails_ThenExpectOneMessageUpdated()
        {
            //arrange
            var updateFinishedEvent = new ManualResetEventSlim();

            var testBatch = new MessageBatchResult<RingbaUOW>
            {
                Messages = new List<MessageWrapper<RingbaUOW>> { new MessageWrapper<RingbaUOW> { Id = "1", Body = new RingbaUOW() } }
            };

            ImplementMeService service = null;

            var subMessageQueService = Substitute.For<IMessageQueService>();
            subMessageQueService.GetMessagesFromQueAsync<RingbaUOW>(
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<int>())
                .Returns(Task.FromResult(testBatch));
            subMessageQueService.UpdateMessagesAsync(Arg.Any<IEnumerable<UpdateBatchRequest>>())
                .Returns(Task.FromResult(new ActionResult()))
                .AndDoes(p => service.Stop());

            var subMessageProcessingService = Substitute.For<IMessageProcessService>();
            subMessageProcessingService.ProccessMessageAsync(Arg.Any<RingbaUOW>())
                .Returns<ActionResult>(x => throw new ArgumentNullException());

            //act
            service = new ImplementMeService(
                Substitute.For<IUOWStatusService>(),
                _logService,
                subMessageProcessingService,
                subMessageQueService);
            await service.DoWork();
            
            //assert
            await subMessageQueService.Received().UpdateMessagesAsync(Arg.Is<IEnumerable<UpdateBatchRequest>>(p =>
            p.Count() == 1 &&
            p.First().Id == "1" &&
            p.First().MessageCompleted == false));
        }
        
        [Fact]
        public async Task GivenOneMessageInQueue_WhenReceivedAndProcessingFails_ThenExpectExceptionLogged()
        {
            //arrange
            string expectedExceptionMessage = "exceptionMessage";
            var updateFinishedEvent = new ManualResetEventSlim();

            var testBatch = new MessageBatchResult<RingbaUOW>
            {
                Messages = new List<MessageWrapper<RingbaUOW>> { new MessageWrapper<RingbaUOW> { Id = "1", Body = new RingbaUOW() } }
            };

            ImplementMeService service = null;

            var subMessageQueService = Substitute.For<IMessageQueService>();
            subMessageQueService.GetMessagesFromQueAsync<RingbaUOW>(
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<int>())
                .Returns(Task.FromResult(testBatch));
            subMessageQueService.UpdateMessagesAsync(Arg.Any<IEnumerable<UpdateBatchRequest>>())
                .Returns(Task.FromResult(new ActionResult()))
                .AndDoes(p => service.Stop());

            var subMessageProcessingService = Substitute.For<IMessageProcessService>();
            subMessageProcessingService.ProccessMessageAsync(Arg.Any<RingbaUOW>())
                .Returns<ActionResult>(x => throw new ArgumentNullException("someParam", expectedExceptionMessage));
                        
            service = new ImplementMeService(
                Substitute.For<IUOWStatusService>(),
                _logService,
                subMessageProcessingService,
                subMessageQueService);

            //act
            await service.DoWork();

            //assert                        
            await _logService.Received().LogAsync(
                Arg.Any<string>(),
                Arg.Is<string>(exceptionMessage => exceptionMessage.Contains(expectedExceptionMessage)),
                Arg.Is<LOG_LEVEL>(logLevel => logLevel == LOG_LEVEL.EXCEPTION),
                Arg.Any<object[]>());
        }
                
        [Fact]
        public async Task GivenOneMessageInQueue_WhenReceivedAndProcessingStopped_ThenExpectExceptionLogged()
        {
            //arrange
            string testMessageId = "1";
            string expectedExceptionMessage = $"Processing of message by id: { testMessageId } has been cancelled.";            
            
            var testBatch = new MessageBatchResult<RingbaUOW>
            {
                Messages = new List<MessageWrapper<RingbaUOW>> { new MessageWrapper<RingbaUOW> { Id = testMessageId, Body = new RingbaUOW() } }
            };

            ImplementMeService service = null;

            var logService = Substitute.For<ILogService>();
            logService.LogAsync(Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<LOG_LEVEL>(),
                Arg.Any<object[]>())
            .Returns(Task.CompletedTask);

            var subMessageQueService = Substitute.For<IMessageQueService>();
            subMessageQueService.GetMessagesFromQueAsync<RingbaUOW>(
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<int>())
                .Returns(Task.FromResult(testBatch))
                .AndDoes(p => { service.Stop(); });

            subMessageQueService.UpdateMessagesAsync(Arg.Any<IEnumerable<UpdateBatchRequest>>())
                .Returns(Task.FromResult(new ActionResult()));                

            var subMessageProcessingService = Substitute.For<IMessageProcessService>();
            subMessageProcessingService.ProccessMessageAsync(Arg.Any<RingbaUOW>())
                .Returns(new ActionResult());

            service = new ImplementMeService(
                Substitute.For<IUOWStatusService>(),
                logService,
                subMessageProcessingService,
                subMessageQueService);

            //act
            await service.DoWork();                        
            
            //assert                        
            await logService.Received().LogAsync(
                Arg.Any<string>(),
                Arg.Is<string>(exceptionMessage => exceptionMessage.Contains(expectedExceptionMessage)),
                Arg.Is<LOG_LEVEL>(logLevel => logLevel == LOG_LEVEL.WARNING),
                Arg.Any<object[]>());
            await subMessageProcessingService.DidNotReceive().ProccessMessageAsync(Arg.Any<RingbaUOW>());
        }

        [Fact]
        public async Task GivenOneMessageInQueue_WhenReceivedAndProcessedAndCancellationRequested_ThenExpect7LoggedMessages()
        {
            //arrange
            var updateFinishedEvent = new ManualResetEventSlim();

            var testBatch = new MessageBatchResult<RingbaUOW>
            {
                Messages = new List<MessageWrapper<RingbaUOW>> { new MessageWrapper<RingbaUOW> { Id = "1", Body = new RingbaUOW() } }
            };

            ImplementMeService service = null;

            var logService = Substitute.For<ILogService>();
            logService.LogAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<LOG_LEVEL>(),
                Arg.Any<object[]>())
            .Returns(Task.CompletedTask);

            var subMessageQueService = Substitute.For<IMessageQueService>();
            subMessageQueService.GetMessagesFromQueAsync<RingbaUOW>(
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<int>())
                .Returns(Task.FromResult(testBatch));

            subMessageQueService.UpdateMessagesAsync(Arg.Any<IEnumerable<UpdateBatchRequest>>())
                .Returns(Task.FromResult(new ActionResult()))
                .AndDoes(p => { service.Stop(); });

            var subMessageProcessingService = Substitute.For<IMessageProcessService>();
            subMessageProcessingService.ProccessMessageAsync(Arg.Any<RingbaUOW>())
                .Returns(new ActionResult { IsSuccessfull = true });

            //act
            service = new ImplementMeService(
                Substitute.For<IUOWStatusService>(),
                logService,
                subMessageProcessingService,
                subMessageQueService);
            await service.DoWork();
                        
            //assert
            await logService.Received(10).LogAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<LOG_LEVEL>(),
                Arg.Any<object[]>());
        }

        [Fact]
        public async Task GivenOneMessageInQueue_WhenReceivedAndProcessingAndCancellationRequested_ThenExpect10LoggedMessages()
        {
            //arrange
            var updateFinishedEvent = new ManualResetEventSlim();

            var testBatch = new MessageBatchResult<RingbaUOW>
            {
                Messages = new List<MessageWrapper<RingbaUOW>> { new MessageWrapper<RingbaUOW> { Id = "1", Body = new RingbaUOW() } }
            };

            ImplementMeService service = null;

            var logService = Substitute.For<ILogService>();
            logService.LogAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<LOG_LEVEL>(),
                Arg.Any<object[]>())
            .Returns(Task.CompletedTask);

            var subMessageQueService = Substitute.For<IMessageQueService>();
            subMessageQueService.GetMessagesFromQueAsync<RingbaUOW>(
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<int>())
                .Returns(Task.FromResult(testBatch))
                .AndDoes(p => { service.Stop(); });

            subMessageQueService.UpdateMessagesAsync(Arg.Any<IEnumerable<UpdateBatchRequest>>())
                .Returns(Task.FromResult(new ActionResult()));                

            var subMessageProcessingService = Substitute.For<IMessageProcessService>();
            subMessageProcessingService.ProccessMessageAsync(Arg.Any<RingbaUOW>())
                .Returns(new ActionResult { IsSuccessfull = true });

            //act
            service = new ImplementMeService(
                Substitute.For<IUOWStatusService>(),
                logService,
                subMessageProcessingService,
                subMessageQueService);
            await service.DoWork();

            //assert
            await logService.Received(10).LogAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<LOG_LEVEL>(),
                Arg.Any<object[]>());
        }

        [Fact]
        public async Task GivenOneMessageInQueue_WhenOneRingbaUOWWithSameIdInProcessing_ThenExpectMessageNotProcessed()
        {
            //arrange
            var updateFinishedEvent = new ManualResetEventSlim();

            var testBatch = new MessageBatchResult<RingbaUOW>
            {
                Messages = new List<MessageWrapper<RingbaUOW>> { new MessageWrapper<RingbaUOW> { Id = "1", Body = new RingbaUOW() } }
            };

            ImplementMeService service = null;

            var subMessageQueService = Substitute.For<IMessageQueService>();
            subMessageQueService.GetMessagesFromQueAsync<RingbaUOW>(
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<int>())
                .Returns(Task.FromResult(testBatch));
            subMessageQueService.UpdateMessagesAsync(Arg.Is<IEnumerable<UpdateBatchRequest>>(p => p.Count() == 0))
                .Returns(Task.FromResult(new ActionResult()));

            var subUOWStatusService = Substitute.For<IUOWStatusService>();
            subUOWStatusService.IsInProcessing(Arg.Any<string>())
                .Returns(Task.FromResult(true))
                .AndDoes(p => service.Stop());

            var subMessageProcessingService = Substitute.For<IMessageProcessService>();
            subMessageProcessingService.ProccessMessageAsync(Arg.Any<RingbaUOW>())
                .Returns(Task.FromResult(new ActionResult { IsSuccessfull = true }));

            //act
            service = new ImplementMeService(
                subUOWStatusService,
                _logService,
                subMessageProcessingService,
                subMessageQueService);
            await service.DoWork();

            //assert            
            await subMessageQueService.Received().UpdateMessagesAsync(Arg.Is<IEnumerable<UpdateBatchRequest>>(p => p.Count() == 0));
        }


        [Theory]
        [InlineData(true, true, true)]
        [InlineData(true, false, true)]
        [InlineData(false, true, true)]
        [InlineData(false, false, true)]
        public async Task GivenOneMessageInQueue_WhenExpirySet_ThenSetMessageCompletedAccordingly(
            bool hasExpiredAge, 
            bool hasExpiredRetries, 
            bool expectedMessageCompleted)
        {
            //arrange
            var updateFinishedEvent = new ManualResetEventSlim();

            var testBatch = new MessageBatchResult<RingbaUOW>
            {
                Messages = new List<MessageWrapper<RingbaUOW>> { new MessageWrapper<RingbaUOW> { Id = "1", Body = new RingbaUOW() } }
            };

            ImplementMeService service = null;

            var subMessageQueService = Substitute.For<IMessageQueService>();
            subMessageQueService.GetMessagesFromQueAsync<RingbaUOW>(
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<int>())
                .Returns(Task.FromResult(testBatch));
            subMessageQueService.UpdateMessagesAsync(Arg.Is<IEnumerable<UpdateBatchRequest>>(p => p.First().MessageCompleted == true))
                .Returns(Task.FromResult(new ActionResult()));

            var subUOWStatusService = Substitute.For<IUOWStatusService>();
            subUOWStatusService.IsInProcessing(Arg.Any<string>())
                .Returns(Task.FromResult(false));

            subUOWStatusService.HasExpiredAge(
                Arg.Any<string>(),
                Arg.Any<long>(),
                Arg.Any<int>())
                .Returns(Task.FromResult(hasExpiredAge))
                .AndDoes(p => service.Stop());
            subUOWStatusService.HasExpiredRetries(
                Arg.Any<string>(),                
                Arg.Any<int>())
                .Returns(Task.FromResult(hasExpiredRetries))
                .AndDoes(p => service.Stop());

            var subMessageProcessingService = Substitute.For<IMessageProcessService>();
            subMessageProcessingService.ProccessMessageAsync(Arg.Any<RingbaUOW>())
                .Returns(Task.FromResult(new ActionResult { IsSuccessfull = true }));

            //act
            service = new ImplementMeService(
                subUOWStatusService,
                _logService,
                subMessageProcessingService,
                subMessageQueService);
            await service.DoWork();

            //assert
            await subMessageQueService.Received().UpdateMessagesAsync(Arg.Is<IEnumerable<UpdateBatchRequest>>(p => p.First().MessageCompleted == expectedMessageCompleted));
        }
    }
}
