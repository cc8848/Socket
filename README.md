# Socket

[![NuGet](https://img.shields.io/nuget/v/ccSocket.svg?style=flat-square)](https://www.nuget.org/packages/ccSocket)

1、基于Tcp的Socket，实现客户端短线重连，服务端断线重连，保持长连接。
注意： 客户端虽然实现了短线重连，但是断线后的的队列里的消息没有做处理，如果需要消息不丢失需要获取队列里的消息进行自行存储。
本项目已经运用到公司项目中，目前运行良好，暂未发现Bug， 欢迎大家指导代码。
