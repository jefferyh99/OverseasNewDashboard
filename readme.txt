我做了两件事：

1、在 Context.md 顶部新增了“5 分钟速读”摘要，接手的 Agent 打开就能先看到系统定位、已完成能力、当前最大缺口、运行方式和推荐入口。
2、清理了 tasks.json，删掉了之前从仓库根目录启动会失败的重复任务，只保留两个可直接用的启动任务：后端从 backend/Api 启动，前端从 frontend 启动。
我还顺手校验了一次， tasks.json 没有格式错误， Context.md 的摘要位置也正常。

如果你接下来要迁移环境，建议新的 Agent 先按这个顺序看：

Context.md
toDoTask.md
Program.cs
AnomaliesController.cs
DashboardController.cs
如果你要，我下一步可以继续帮你把迁移环境所需的“最小启动包”再整理一遍，比如哪些文件必须带走、哪些本地生成文件可以删。

