# Snap.Hutao.Server 项目级指南

适用范围：`src/Snap.Hutao.Server/Snap.Hutao.Server/` 目录及其下所有 C# 源码、配置与资源（若更深层目录包含自身的 `AGENTS.md`，则以该说明为准）。

## 目录结构速览
- `Program.cs`：应用入口与 DI/中间件配置。
- `Controller/`：ASP.NET Core 控制器。`Filter/` 子目录存放全局过滤器与 Action 扩展。
- `Service/`：领域服务，按业务域再分子文件夹（如 `GachaLog/`、`OAuth/`、`Redis` 相关等）。
- `Model/`：请求/响应模型与 EF Core 实体，包含 `Context/`、`Entity/` 等子目录。
- `Core/`：通用工具类（异步锁、随机、计时器等）。
- `Extension/`：扩展方法，按目标类型拆分。
- `Job/`：Quartz 定时任务。
- `Option/`：`IOptions<T>` 配置绑定对象。
- `Discord/`：与 Disqord Bot 相关的配置和事件处理。
- `Migrations/`：EF Core 迁移（请仅通过 `dotnet ef` 生成/更新）。
- `Properties/`：`launchSettings.json` 等运行配置。
- `wwwroot/`：静态资源与公开页面（详见该目录下的 `AGENTS.md`）。

## C# 代码规范
- **版权头部**：所有 C# 文件保留顶部 MIT 版权声明，不得删除或重复添加。
- **命名空间**：使用文件夹对齐的 `namespace Snap.Hutao.Server.*;`，一个文件仅声明一个命名空间。
- **Using 顺序**：按照 `.NET/BCL -> 第三方 -> Snap.Hutao.*` 分类，并保持字母顺序；使用 `global using` 时需同步更新 `GlobalUsing.cs`。
- **类型修饰**：
  - 服务类、控制器等默认使用 `sealed`，除非需要继承。
  - 依赖注入字段使用 `private readonly`，通过构造函数注入。
- **异步编程**：
  - 异步方法名以 `Async` 结尾，返回 `Task`/`ValueTask`，并在 `await` 后调用 `.ConfigureAwait(false)`（与现有代码保持一致）。
  - 优先使用集合表达式（`[]`）与 `record`/`record struct`（如项目已有示例）。
- **错误处理与日志**：
  - 使用 `ILogger<T>` 记录关键步骤或异常信息，避免吞掉异常。
  - 捕获异常时尽可能返回 `ReturnCode` 语义化结果。
- **StyleCop/Analyzers**：`dotnet build` 会触发检查；遵循 `stylecop.json`，例如：元素排序、`using` 在命名空间之外、文件内类顺序等。

## 控制器约定
- 按功能分组到对应文件，使用 `[ApiController]` 与 `[Route("[controller]")]` 或更明确的路由。
- 返回统一封装的响应对象：`Model.Response.Response` / `Response<T>` 或 `IActionResult`。
- 授权与过滤器通过属性声明（例如 `[Authorize]`、`[ServiceFilter]`），避免在控制器中手动解析。
- 针对客户端版本、Token 校验等逻辑，优先调用 `Controller` 扩展方法（位于 `Controller/` 下）。

## 服务与数据访问
- EF Core：
  - 数据库上下文位于 `Model/Context/`，使用 `DbContextPool` 配置。
  - 查询默认使用 `AsNoTracking()`，批量写入使用扩展方法（如 `AddRangeAndSaveAsync`）。
  - 修改实体时注意并发与约束，使用事务时统一封装在服务层。
- 缓存：统一通过 `IMemoryCache` 或 Redis 封装类访问，确保 Key 前缀与过期策略清晰。
- 外部服务（GitHub、Sentry、OAuth 等）位于对应子目录，新增服务时请：
  - 先在 `Option/` 下定义配置对象。
  - 在 `Program.cs`/`ServiceCollection` 配置中注册。
  - 使用 `HttpClientFactory` (`services.AddHttpClient()`) 获取客户端。

## 选项与配置
- 所有可配置项应创建 POCO 类放在 `Option/`，并在 `Program.cs` 中 `services.Configure<TOptions>`。
- 不直接从 `IConfiguration` 手动读取散落值，优先使用强类型配置。
- 若新增常量或开关，可考虑添加到 `ServerKeys.cs` 或相应配置文件。

## Quartz 作业
- 新增作业放置于 `Job/`，实现 `IJob` 并保持类 `sealed`。
- 在 `Program.cs` 中注册与排程，Cron 表达式需注释执行频率。
- 作业中访问服务请通过构造函数注入；避免自己创建作用域。

## Discord Bot
- 相关逻辑位于 `Discord/` 与 `Service/Discord/`。修改时留意权限与 Rate Limit，必要时更新 README 或注释。

## 静态资源
- `wwwroot/` 仅存放对外暴露的静态文件；若新增 SPA 或前端组件，请在该目录的 `AGENTS.md` 阅读更多约定。

如无特殊说明，所有新文件应放在最合适的子目录，并同步更新相关 `AGENTS.md`（例如新增业务域时可创建新的子目录说明）。
