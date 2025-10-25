# Snap.Hutao.Server – Agent Handbook

欢迎来到 Snap.Hutao.Server 仓库。本指南面向未来的 AI Agent，帮助快速了解项目背景、目录结构、开发流程以及常见注意事项。

## 仓库概览
- **项目类型**：.NET 9 Web API，配合 Quartz 任务、EF Core、Redis、Sentry、Disqord Bot 等组件。
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

## 推荐工作流程
1. **环境准备**：确保安装 .NET SDK 9.x、Node（如需前端构建）、Docker（如需调试容器化）。
2. **同步依赖**：运行 `dotnet restore src/Snap.Hutao.Server/Snap.Hutao.Server.sln`。
3. **编译校验**：执行 `dotnet build --configuration Release src/Snap.Hutao.Server/Snap.Hutao.Server.sln`。这一步会触发 StyleCop、Roslyn 分析器。
4. **测试**：当前仓库没有独立的测试项目，如需新增测试，请置于 `tests/`（若创建）或与解决方案保持一致，并在此处更新说明。
5. **运行本地服务**：可使用 `dotnet run --project src/Snap.Hutao.Server/Snap.Hutao.Server/Snap.Hutao.Server.csproj`，或通过 `run.sh` / Dockerfile 进行容器化调试。
6. **检查 Swagger**：本项目集成 Swashbuckle，启动后访问 `/swagger` 验证 API。

## 编码与提交守则
- 先阅读并遵守子目录的 `AGENTS.md`。
- 代码必须通过 `dotnet build` 并保持 StyleCop 警告为零；必要时使用 `dotnet format`（仅修复安全的自动格式问题）。
- 不得移除或修改现有版权头部注释。
- 不要将机密配置写入仓库（`appsettings.Production.json`、Sentry DSN 等应使用环境变量或 Secret Manager）。
- 在进行 EF Core 迁移时，请同步更新 `Migrations/` 目录，并记录新增命令（例如 `dotnet ef migrations add <Name> --project src/Snap.Hutao.Server/Snap.Hutao.Server`）。
- PR/提交信息需清晰描述改动目的；若引入 Breaking Change，需在说明中明确指出并更新相关文档。

## 调试与排错提示
- 连接字符串在 `appsettings*.json`（或环境变量）中，数据库采用 MySQL (`Pomelo.EntityFrameworkCore.MySql`)；调试时可指向本地或容器实例。
- Quartz 任务配置在 `Program.cs` 与 `Job/` 子目录；如需新增任务，请确保 Cron 合法，并在文档或注释中说明用途。
- Redis 通过 `StackExchange.Redis` 使用，位于服务层；新增缓存逻辑时需注意 Key 命名与过期策略。
- Sentry 配置在启动时注入，避免在源码中硬编码新的 DSN。

## 文档与国际化
- README 当前为简要中文说明，如新增功能或部署方式，建议同步补充。
- 注释与 API 描述建议使用中文或中英双语，保持与现有风格一致。

如需在仓库任意位置新增说明，请补充或更新对应 `AGENTS.md`，确保后续 Agent 能快速接手。祝开发顺利！
