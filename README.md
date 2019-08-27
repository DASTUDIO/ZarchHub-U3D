# 使用

## 1.首先需要把ThreadInvoker脚本拖动到场景的一个物体上
## 2.然后按照如下方式调用

```cs
ZarchHub.LoadResource(请求url,[接受完成的回调(传入参数是文件保存的位置)])
```

示例:
```csharp
ZarchHub.LoadResource("https://src.pub/", file => { textView.text += "\n" + System.IO.File.OpenText(file).ReadLine(); } );

```

PS:
* 1.下载任务不在主线程运行，传入的回调在主线程运行。 
* 2.如果异常中断，重新请求相同的url会断点续传。
