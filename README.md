# v2ray-server-traffic-monitor

这是一个通过 [V2Ctl API](https://www.v2fly.org/chapter_00/command.html#v2ctl-api) 获取 V2Ray 服务端统计数据，列表显示各用户实时通过流量的 Windows 小工具。  
![](https://raw.githubusercontent.com/szwhy/v2ray-server-traffic-monitor/master/img/Windows.png)

## 内容列表

- [说明](#说明) 
- [开启V2Ray统计功能](#开启V2Ray统计功能) 
- [下载](#下载) 
- [使用说明](#使用说明) 
- [License](#License)

## 说明

这个工具只可获取并处理 V2Ray 服务端返回的统计信息，因此，显示的数据为 V2Ray 服务端统计所得，并不代表用户真实使用的流量。（若服务端开启锐速等工具，数据可能大幅偏离用户实际使用情况）  
使用前，请确保你的 V2Ray 已正确配置开启流量统计功能。  
你可以通过 [这些命令](https://guide.v2fly.org/advanced/traffic.html#%E6%9F%A5%E7%9C%8B%E6%B5%81%E9%87%8F%E4%BF%A1%E6%81%AF) 调用 V2Ctl 并观察是否正常返回统计信息。

## 开启V2Ray统计功能

关于 V2Ray 统计功能，可参考：  
官网(https://www.v2fly.org/chapter_02/stats.html)  
社区白话文指南(https://guide.v2fly.org/advanced/traffic.html)  
  
请注意将 `api` 的 `routing` 规则写在所有规则最前面，否则可能导致命中其他规则无法获得数据。  

## 下载

查看最新 [Releases](https://github.com/szwhy/v2ray-server-traffic-monitor/releases)。  

## 使用说明

1. 从 [V2Ray官方库](https://github.com/v2ray/v2ray-core/releases) 中下载 Windows 客户端，将 `v2ctl.exe` 与这个工具放在同一目录内。  
2. 打开工具后，输入 V2Ray **服务端IP**及服务端配置中 `tag` 为 `api` 的 `inbound` **监听端口**。  
3. 勾选 `实时刷新` 即自动开始获取数据。  
注：实时流量为两次获取的总流量之差，仅供参考。
4. `User` 对应 V2Ray 服务端配置中 `vmess` 协议 `clients` 的 `email`。
```
如果服务端配置没有为用户指定 Email，则不会开启统计。  
socks, http 等其他协议不支持统计。
```

## License
[MIT](LICENSE)
