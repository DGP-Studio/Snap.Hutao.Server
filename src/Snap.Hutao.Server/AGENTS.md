# Snap.Hutao.Server/src 指南

适用范围：`src/Snap.Hutao.Server/` 目录及其子目录（除非另有更深层的 `AGENTS.md`）。本目录包含解决方案、部署脚本与容器化资源。

## 解决方案管理
- 本目录的主解决方案为 `Snap.Hutao.Server.sln`，仅包含 `Snap.Hutao.Server` Web API 项目。如需新增项目（测试、工具等），请更新解决方案并在根目录 `AGENTS.md` 的目录结构中同步说明。
- 恢复依赖、构建与运行命令示例：
  - `dotnet restore Snap.Hutao.Server.sln`
  - `dotnet build --configuration Release Snap.Hutao.Server.sln`
  - `dotnet run --project Snap.Hutao.Server/Snap.Hutao.Server.csproj`
- 添加 EF Core 迁移时，请使用解决方案根作为工作目录，确保 `--project` 与 `--startup-project` 指向 `Snap.Hutao.Server/Snap.Hutao.Server.csproj`。

## 部署脚本与容器
- `Dockerfile` 与 `docker-compose.yml` 采用多阶段构建，更新时保持镜像体积最小化，并确保暴露端口、环境变量与卷挂载说明清晰。
- `run.sh` 面向 Linux 环境的快速部署脚本：
  - 避免在脚本内硬编码敏感信息。
  - 修改时保持 `bash` 兼容语法，并确保 `set -euo pipefail` 的最佳实践（若添加，请说明原因）。
  - 如变更镜像名、容器名或端口，请在脚本顶部常量区域更新并保持格式一致。

## 配置与密钥
- 不在仓库内新增实际密钥或生产连接字符串。如需示例配置，请以占位符（如 `YOUR_VALUE_HERE`）代替。
- 若新增 `appsettings.*.json` 样例文件，请提供最小字段集合，并在注释/README 中提醒用户使用 Secret Manager。

## 文档
- 修改部署相关文件（Docker、脚本、README）时，优先更新根 README 或新增 `docs/` 文档以说明步骤。
- 若新增命令行工具或脚本，请在此目录下补充简单的 `README` 或注释，帮助后续维护者理解用途。

继续深入 `Snap.Hutao.Server/` 项目目录时，请阅读该目录下的专用指南。
