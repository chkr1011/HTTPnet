<p align="center">
<img src="https://github.com/chkr1011/HTTPnet/blob/master/Images/Logo_128x128.png?raw=true" width="128">
</p>

[![NuGet Badge](https://buildstats.info/nuget/HTTPnet)](https://www.nuget.org/packages/HTTPnet)
[![BCH compliance](https://bettercodehub.com/edge/badge/chkr1011/HTTPnet?branch=master)](https://bettercodehub.com/)

# HTTPnet
HTTPnet is a .NET library for HTTP and WebSocket based communication. It provides a HTTP/WebSocket server and a powerful processing pipeline for HTTP request and their responses.

# Features

### General
* Async support
* HTTP context pipeline for powerful request and response processing
* Support for WebSocket connections
* Lightweight (only the low level implementation of HTTP, no overhead)
* Access to internal trace messages

# Supported frameworks
* .NET Standard 1.3+
* .NET Core 1.1+
* .NET Core App 1.1+
* .NET Framework 4.5.2+ (x86, x64, AnyCPU)
* Universal Windows (UWP) 10.0.10240+ (x86, x64, ARM, AnyCPU)
* Mono 5.2+

# Supported HTTP versions
* 1.1
* 1.0

# Supported HTTP features
* Compressed responses with Gzip
* Expect header (100-Continue) for large bodies
* Keep-Alive connections

# Nuget
This library is available as a nuget package: https://www.nuget.org/packages/HTTPnet/

# Examples
Please find examples and the documentation at the Wiki of this repository (https://github.com/chkr1011/HTTPnet/wiki).

# Contributions
If you want to contribute to this project just create a pull request.

# References
This library is used in the following projects:

* HA4IoT (Open Source Home Automation system for .NET, https://github.com/chkr1011/HA4IoT)

If you use this library and want to see your project here please let me know.