// Copyright (c) 2025 EFramework Innovation. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

using NUnit.Framework;
using EFramework.Unity.Utility;

/// <summary>
/// TestXMani 是 XMani 的单元测试。
/// </summary>
public class TestXMani
{
    [Test]
    public void Parse()
    {
        // Arrange
        var manifest = new XMani.Manifest();
        var data = "file1.txt|d41d8cd98f00b204e9800998ecf8427e|0\nfile2.txt|d41d8cd98f00b204e9800998ecf8427e|123";

        // Act
        var result = manifest.Parse(data, out string error);

        // Assert
        Assert.That(result, Is.True, "清单解析应该成功完成");
        Assert.That(error, Is.Empty, "解析过程不应产生错误信息");
        Assert.That(manifest.Files.Count, Is.EqualTo(2), "清单应包含两个文件记录");
        Assert.That(manifest.Files[0].Name, Is.EqualTo("file1.txt"), "第一个文件名称应正确解析");
        Assert.That(manifest.Files[0].Size, Is.EqualTo(0), "第一个文件大小应为 0");
        Assert.That(manifest.Files[1].Name, Is.EqualTo("file2.txt"), "第二个文件名称应正确解析");
        Assert.That(manifest.Files[1].Size, Is.EqualTo(123), "第二个文件大小应为 123");
    }

    [Test]
    public void Read()
    {
        var tempDir = XFile.PathJoin(XEnv.LocalPath, "TestXMani-" + XTime.GetMillisecond());
        var tempFilePath = XFile.PathJoin(tempDir, XMani.Default);
        var tempSecret = "12345678";

        XFile.SaveText(tempFilePath, "file1.txt|d41d8cd98f00b204e9800998ecf8427e|100\nfile2.txt|d41d8cd98f00b204e9800998ecf8427e|123".Encrypt(tempSecret));

        // Arrange
        var manifest = new XMani.Manifest();

        // Act
        var handler = manifest.Read(uri: tempFilePath, secret: tempSecret, onPreRequest: req =>
        {
            req.timeout = 10;
        });

        manifest.Files.Add(new XMani.FileInfo { Name = "file1.txt", MD5 = "d41d8cd98f00b204e9800998ecf8427e", Size = 100 });

        // Simulate the completion of the request
        while (!handler.Invoke()) { }

        // Assert
        Assert.That(string.IsNullOrEmpty(manifest.Error), Is.True, "读取清单文件不应产生错误");
        Assert.That(manifest.Files, Is.Not.Empty, "清单文件应包含文件记录");

        var fileInfo = manifest.Files.Find(ele => ele.Name == "file1.txt");
        Assert.That(fileInfo, Is.Not.Null, "清单文件应包含 file1.txt 的文件记录");
        Assert.That(fileInfo.MD5, Is.EqualTo("d41d8cd98f00b204e9800998ecf8427e"), "清单文件解析的 file1.txt 文件哈希值应当和保存的一致");
        Assert.That(fileInfo.Size, Is.EqualTo(100), "清单文件解析的 file1.txt 文件大小应当和保存的一致");

        XFile.DeleteDirectory(tempDir);
    }

    [Test]
    public void Compare()
    {
        // Arrange
        var manifest1 = new XMani.Manifest();
        manifest1.Files.Add(new XMani.FileInfo { Name = "file1.txt", MD5 = "md5_1", Size = 100 });
        manifest1.Files.Add(new XMani.FileInfo { Name = "file2.txt", MD5 = "md5_2", Size = 200 });

        var manifest2 = new XMani.Manifest();
        manifest2.Files.Add(new XMani.FileInfo { Name = "file1.txt", MD5 = "md5_1", Size = 100 });
        manifest2.Files.Add(new XMani.FileInfo { Name = "file3.txt", MD5 = "md5_3", Size = 300 });

        // Act
        var diff = manifest1.Compare(manifest2);

        // Assert
        Assert.That(diff.Deleted.Count, Is.EqualTo(1), "应检测到一个被删除的文件（file2.txt）");
        Assert.That(diff.Added.Count, Is.EqualTo(1), "应检测到一个新增的文件（file3.txt）");
        Assert.That(diff.Modified.Count, Is.EqualTo(0), "不应有被修改的文件");
    }

    [Test]
    public void Stringify()
    {
        // Arrange
        var manifest = new XMani.Manifest();
        manifest.Files.Add(new XMani.FileInfo { Name = "file1.txt", MD5 = "md5_1", Size = 100 });
        manifest.Files.Add(new XMani.FileInfo { Name = "file2.txt", MD5 = "md5_2", Size = 200 });

        // Act
        var result = manifest.ToString();

        // Assert
        Assert.That(result.Contains("file1.txt|md5_1|100"), Is.True, "清单文本应包含第一个文件的完整信息");
        Assert.That(result.Contains("file2.txt|md5_2|200"), Is.True, "清单文本应包含第二个文件的完整信息");
    }
}
