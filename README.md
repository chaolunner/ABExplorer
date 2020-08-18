# ABExplorer - Unity AssetBundle 管理插件

安装方法
---

- 做为子模块添加 `git submodule add https://github.com/chaolunner/ABExplorer.git Assets/ABExplorer`

目录结构
---

- 所有需要打包成AssetBundle的资源都需要放在**AB_Resources**文件夹下（你可以在**Assets**根目录下的任意位置创建多个AB_Resources文件夹）

  ![](https://github.com/chaolunner/CloudNotes/blob/master/ABExplorer/assetbundle-assets-directory-structure.png)
- AB_Resources下面的一级目录可以按场景划分（即该目录下只包含与该场景相关的资源）或者按管理类划分（比如UIManager，则该目录下的资源都由UIManager管理，并且只包含与UI相关的资源）
- AB_Resources下面的二级目录可以按资源类型划分（如Prefabs、Textures、Materials等等）或者按功能划分（如UI、NPC、道具等等）
- AB_Resources的二级目录下面包含实际的资源文件或更多细分目录

打包流程
---

- 选择编辑器栏上的 Tools → AssetBundles → Set Labels - 设置资源的AssetBundle名字和变体
- 选择编辑器栏上的 Tools → AssetBundles → Build AssetBundles - 打包AssetBundle，导出到StreamingAssets文件夹下，平台取决于你当前所在的平台
- 选择编辑器栏上的 Tools → AssetBundles → Delete All - 删除StreamingAssets文件夹下，所有当前平台相关的AssetBundle包（进行一次干净的构建）
- 选择编辑器栏上的 Tools → AssetBundles → Clear Cache - 删除当前编辑器已缓存的所有AssetBundle内容（在编辑器中测试时，这非常有用）

  ![](https://github.com/chaolunner/CloudNotes/blob/master/ABExplorer/assetbundles-utilities.png)
  
运行测试服务器
---

- 打开编辑器栏上的 Edit → Preferences
- 选择 Http Server
- 勾选 Is Enable

  ![](https://github.com/chaolunner/CloudNotes/blob/master/ABExplorer/http-server.png)
- 启用服务器后，当您在运行编辑器时，http服务器将自动运行
- 请注意，当您想使用其他设备访问http服务器时，请记住将localhost更改为主机的真实IP地址（cmd → ipconfig → Ethernet adapter Ethernet: IPv4 Address）
