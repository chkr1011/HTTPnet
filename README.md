<p align="center">
<img src="https://github.com/chkr1011/HTTPnet/blob/master/Images/Logo_128x128.png?raw=true" width="128">
</p>

[![NuGet Badge](https://buildstats.info/nuget/HTTPnet)](https://www.nuget.org/packages/HTTPnet)
[![BCH compliance](https://bettercodehub.com/edge/badge/chkr1011/HTTPnet?branch=master)](https://bettercodehub.com/)

# HTTPnet
HTTPnet is a high performance .NET library for HTTP and WebSocket based communication. It provides a server and a processing pipeline for HTTP request and their responses including modules for MVC controllers, WebSocket sessions and static files.

## Features
* Async support
* HTTP context pipeline for request and response processing
* Support for WebSocket connections
* Lightweight (only the low level implementation of HTTP, no overhead)
* MVC controllers with parameter marshalling
* Module for static file hosting
* Access to internal trace messages

## Supported HTTP features
* Compressed responses with Gzip
* Expect header (100-Continue) for large bodies
* Keep-Alive connections
* WebSockets

## Supported frameworks
* .NET Standard 1.3+
* .NET Core 1.1+
* .NET Core App 1.1+
* Universal Windows Platform (UWP) 10.0.10240+ (x86, x64, ARM, AnyCPU, Windows 10 IoT Core)
* .NET Framework 4.5.2+ (x86, x64, AnyCPU)
* Mono 5.2+
* Xamarin.Android 7.5+
* Xamarin.iOS 10.14+

## Supported HTTP versions
* 1.1
* 1.0

## Nuget
This library is available as a nuget package: https://www.nuget.org/packages/HTTPnet/

## Examples
Please find examples and the documentation at the Wiki of this repository (https://github.com/chkr1011/HTTPnet/wiki).

## MIT License

Copyright (c) 2017 Christian Kratky

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.