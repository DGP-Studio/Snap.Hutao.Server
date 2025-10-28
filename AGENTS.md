# Snap.Hutao.Server – Agent Handbook

欢迎来到 Snap.Hutao.Server 仓库。本指南面向未来的 AI Agent，帮助快速了解项目背景、目录结构、开发流程以及常见注意事项。

## 仓库概览
- **项目类型**：ASP.NET MVC Web API，配合 Quartz 任务、EF Core、Redis、Sentry等组件。
- **主要目标**：为胡桃客户端及相关服务提供后端接口，同时暴露部分静态页面（例如 `wwwroot/redeem.html`）。
- **代码风格**：启用 StyleCop Analyzers，自带 `stylecop.json`；所有 C# 源文件都包含版权头部注释。

## 目录速览
```
/ (仓库根目录)
├── src/                 解决方案与部署脚本
│   └── Snap.Hutao.Server/
│       ├── Snap.Hutao.Server.sln  主解决方案
│       ├── Dockerfile, docker-compose.yml, run.sh 等部署资产
│       └── Snap.Hutao.Server/     具体 Web API 项目（见该目录下 AGENTS.md）
├── README.md
└── LICENSE
```
进入子目录前，请先检查是否存在额外的 `AGENTS.md`，遵循就近原则：越靠近文件的说明优先级越高。

## 编码与提交守则
- 先阅读并遵守子目录的 `AGENTS.md`。
- 不得移除或修改现有版权头部注释。
- PR/提交信息需清晰描述改动目的；若引入 Breaking Change，需在说明中明确指出并更新相关文档。

如需在仓库任意位置新增说明，请补充或更新对应 `AGENTS.md`，确保后续 Agent 能快速接手。祝开发顺利！
