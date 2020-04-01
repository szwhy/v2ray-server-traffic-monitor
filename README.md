# v2ray-server-traffic-monitor

这是一个通过[V2Ctl API](https://www.v2fly.org/chapter_00/command.html#v2ctl-api)获取V2Ray服务端统计数据，列表显示各用户实时通过流量的Windows小工具。

## 内容列表

- [说明](#说明)
- [开启V2Ray统计功能](#开启V2Ray统计功能)
- [下载](#下载)
- [使用说明](#使用说明)

## 说明

这个工具只可获取并处理V2Ray服务端返回的统计信息，因此，显示的数据为V2Ray服务端统计所得，并不代表用户真实使用的流量。  
（例如，若服务器开启锐速等工具，则数据可能大幅偏离用户实际使用情况）  
使用前，请确保你的V2Ray已正确配置开启流量统计功能。  
你可以通过 [这些命令](https://guide.v2fly.org/advanced/traffic.html#%E6%9F%A5%E7%9C%8B%E6%B5%81%E9%87%8F%E4%BF%A1%E6%81%AF) 调用V2Ctl并观察是否正常返回统计信息。

## 开启V2Ray统计功能

关于V2Ray统计功能，可参考：  
官网(https://www.v2fly.org/chapter_02/stats.html)  
社区白话文指南(https://guide.v2fly.org/advanced/traffic.html)  
  
请注意将api的routing规则写在所有规则最前面，否则可能导致命中其他规则无法获得数据。  

## 下载

查看最新[Releases](https://github.com/szwhy/v2ray-server-traffic-monitor/releases)。  

## 使用说明

