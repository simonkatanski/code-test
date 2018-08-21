# Ringba Simple Code Test

## What the project does

This project is designed to be deployed to a docker cluster. The main task of the project is to pull message off some sort of 
messaging que in batch form and process each of the messages in the batch. This service will be deployed to a docker cluster and will be auto scalled based on the message throughput. The project may have up to N instances running at a time. 

The goal of this project is to utilize good design and practices in processing the messages as quickly and efficiently as possible. It can be assumed the majority of the work that is done by each message is IO bound work.

## What to do

Utlilizing the 4 exisint interfaces, you should implement the `ImplementMeService` to process batches of messages in an inffectient manner. 

You will use the `IMessageQueService` to retrieve batches of messages to proccess by sending in the `RingbaUOW` of the message body to the `IMessageProcessService`. If the message is succesfully processed, you will update the message with the `MessageCompleted` flag set to true, if the message processing fails, you must set this flag to false wich will place the message back in the que to be tried again. However, every `RingbaUOW` has a `MaxNumberOfRetries` and `MaxAgeInSeconds` that dictates how long the message can be retried for and how many times. `-1` values for these fields should be treated as no limit. If the limit is reached, we should update the message with `MessageCompleted` set to true to remove it from the que.

A RingbaUOW should only ever be processing once at a time. In other words, there should never be to processes working on the same RingbaUOW at a time. The `Id` of every `RingbaUOW` is unique.

## Notes

Ringba takes logging very seriously and the interface `ILogService` should be used to log as much as possible.

The `IMessageQueService` has some limitations that must be accounted for:
* When messages are "in flight", under heavy loads, there is no garuntee that a duplicate message may be delivered with parrel calls to `GetMessagesFromQueAsync`
* Once a message is marked as completed, it is garunteed not to show back up after 2 seconds

The `IKVRespository` is a Key Value repository that allows atomic fast access to keys stored in a central repository.

The `ILogService` is a very optimized logging platform that does not block to ship and can be called at will with little to no affect to the performance of the app.

The code in the `ImplementMeService.DoWork` is not to be used but only there as an example of how the flow would run. 


## What is expected

A effecient implemenentation complete with unit tests must be completed inorder for your respone be considreed.

This project **IS NOT** designed to run, rather be ran from `dotnet test code-test.test\code-test.test.csproj`







