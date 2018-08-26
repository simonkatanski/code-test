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
            var updateFinishedEvent = new ManualResetEventSlim();

            var testBatch = new MessageBatchResult<RingbaUOW>
            {
                Messages = new List<MessageWrapper<RingbaUOW>> { new MessageWrapper<RingbaUOW> { Id = "1" } }
            };

            var subMessageQueService = Substitute.For<IMessageQueService>();
            subMessageQueService.GetMessagesFromQueAsync<RingbaUOW>(
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<int>())
                .Returns(Task.FromResult(testBatch));
            subMessageQueService.UpdateMessagesAsync(Arg.Any<IEnumerable<UpdateBatchRequest>>())
                .Returns(Task.FromResult(new ActionResult()))
                .AndDoes(p => updateFinishedEvent.Set());

            var subMessageProcessingService = Substitute.For<IMessageProcessService>();
            subMessageProcessingService.ProccessMessageAsync(Arg.Any<RingbaUOW>())
                .Returns(Task.FromResult(new ActionResult { IsSuccessfull = isActionResultSuccessful }));

            var service = new ImplementMeService(
                Substitute.For<IKVRepository>(),
                _logService,
                subMessageProcessingService,
                subMessageQueService);

            //act
            Task.Run(() => service.DoWork());

            updateFinishedEvent.Wait();

            service.Stop();

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
                Messages = new List<MessageWrapper<RingbaUOW>> { new MessageWrapper<RingbaUOW> { Id = "1" } }
            };

            var subMessageQueService = Substitute.For<IMessageQueService>();
            subMessageQueService.GetMessagesFromQueAsync<RingbaUOW>(
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<int>())
                .Returns(Task.FromResult(testBatch));
            subMessageQueService.UpdateMessagesAsync(Arg.Any<IEnumerable<UpdateBatchRequest>>())
                .Returns(Task.FromResult(new ActionResult()))
                .AndDoes(p => updateFinishedEvent.Set());

            var subMessageProcessingService = Substitute.For<IMessageProcessService>();
            subMessageProcessingService.ProccessMessageAsync(Arg.Any<RingbaUOW>())
                .Returns<ActionResult>(x => throw new ArgumentNullException());

            var service = new ImplementMeService(
                Substitute.For<IKVRepository>(),
                _logService,
                subMessageProcessingService,
                subMessageQueService);

            //act
            Task.Run(() => service.DoWork());

            updateFinishedEvent.Wait();

            service.Stop();

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
                Messages = new List<MessageWrapper<RingbaUOW>> { new MessageWrapper<RingbaUOW> { Id = "1" } }
            };
            
            var subMessageQueService = Substitute.For<IMessageQueService>();
            subMessageQueService.GetMessagesFromQueAsync<RingbaUOW>(
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<int>())
                .Returns(Task.FromResult(testBatch));
            subMessageQueService.UpdateMessagesAsync(Arg.Any<IEnumerable<UpdateBatchRequest>>())
                .Returns(Task.FromResult(new ActionResult()))
                .AndDoes(p => updateFinishedEvent.Set());

            var subMessageProcessingService = Substitute.For<IMessageProcessService>();
            subMessageProcessingService.ProccessMessageAsync(Arg.Any<RingbaUOW>())
                .Returns<ActionResult>(x => throw new ArgumentNullException("someParam", expectedExceptionMessage));
                        
            var service = new ImplementMeService(
                Substitute.For<IKVRepository>(),
                _logService,
                subMessageProcessingService,
                subMessageQueService);

            //act
            Task.Run(() => service.DoWork());

            updateFinishedEvent.Wait();

            service.Stop();

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
                Messages = new List<MessageWrapper<RingbaUOW>> { new MessageWrapper<RingbaUOW> { Id = testMessageId } }
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
                .AndDoes(p =>
                {
                    service.Stop();                    
                });

            subMessageQueService.UpdateMessagesAsync(Arg.Any<IEnumerable<UpdateBatchRequest>>())
                .Returns(Task.FromResult(new ActionResult()));                

            var subMessageProcessingService = Substitute.For<IMessageProcessService>();
            subMessageProcessingService.ProccessMessageAsync(Arg.Any<RingbaUOW>())
                .Returns(new ActionResult());

            service = new ImplementMeService(
                Substitute.For<IKVRepository>(),
                logService,
                subMessageProcessingService,
                subMessageQueService);

            //act
            Task.Run(() => service.DoWork());                        
            
            //assert                        
            await logService.Received().LogAsync(
                Arg.Any<string>(),
                Arg.Is<string>(exceptionMessage => exceptionMessage.Contains(expectedExceptionMessage)),
                Arg.Is<LOG_LEVEL>(logLevel => logLevel == LOG_LEVEL.EXCEPTION),
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
                Messages = new List<MessageWrapper<RingbaUOW>> { new MessageWrapper<RingbaUOW> { Id = "1" } }
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
                .AndDoes(p =>
                 {
                     service.Stop();
                     updateFinishedEvent.Set();
                });

            var subMessageProcessingService = Substitute.For<IMessageProcessService>();
            subMessageProcessingService.ProccessMessageAsync(Arg.Any<RingbaUOW>())
                .Returns(new ActionResult { IsSuccessfull = true });

            service = new ImplementMeService(
                Substitute.For<IKVRepository>(),
                logService,
                subMessageProcessingService,
                subMessageQueService);

            //act
            Task.Run(() => service.DoWork());

            updateFinishedEvent.Wait();

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
                Messages = new List<MessageWrapper<RingbaUOW>> { new MessageWrapper<RingbaUOW> { Id = "1" } }
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
                .AndDoes(p =>
                {
                    service.Stop();
                    updateFinishedEvent.Set();
                });

            subMessageQueService.UpdateMessagesAsync(Arg.Any<IEnumerable<UpdateBatchRequest>>())
                .Returns(Task.FromResult(new ActionResult()));                

            var subMessageProcessingService = Substitute.For<IMessageProcessService>();
            subMessageProcessingService.ProccessMessageAsync(Arg.Any<RingbaUOW>())
                .Returns(new ActionResult { IsSuccessfull = true });

            service = new ImplementMeService(
                Substitute.For<IKVRepository>(),
                logService,
                subMessageProcessingService,
                subMessageQueService);

            //act
            Task.Run(() => service.DoWork());

            updateFinishedEvent.Wait();

            //assert
            await logService.Received(10).LogAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<LOG_LEVEL>(),
                Arg.Any<object[]>());
        }
    }
}
