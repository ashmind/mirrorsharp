using System.IO;
using System.Text;
using Microsoft.FSharp.Core;
using MirrorSharp.FSharp.Internal;
using Xunit;

namespace MirrorSharp.Tests.FSharp;

public class CustomFileSystemTests {
    [Fact]
    public void OpenFileForReadShim_ReturnsStreamWithDataFromRegister_ForVirtualFile() {
        // Arrange
        var system = CustomFileSystem.Instance;
        var data = Encoding.UTF8.GetBytes("Test");
        var file = system.RegisterVirtualFile(_ => new MemoryStream(data), (object?)null);
        var optionFalse = FSharpOption<bool>.Some(false);

        // Act
        var stream = system.OpenFileForReadShim(file.Path, optionFalse, optionFalse);

        // Assert
        var result = new StreamReader(stream).ReadToEnd();
        Assert.Equal("Test", result);
    }

    [Fact]
    public void OpenFileForReadShim_ReturnsSameStreamWithPositionAtStart_ForVirtualFile_EvenIfCalledTwice() {
        // Arrange
        var system = CustomFileSystem.Instance;
        var stream = new MemoryStream(new byte[5]);
        var file = system.RegisterVirtualFile(_ => stream, (object?)null);
        var optionFalse = FSharpOption<bool>.Some(false);

        // Act
        var first = system.OpenFileForReadShim(file.Path, optionFalse, optionFalse);
        first.Position = 3;
        first.Close();
        var second = system.OpenFileForReadShim(file.Path, optionFalse, optionFalse);

        // Assert
        Assert.Same(first, second);
        Assert.Equal(0, second.Position);
        Assert.True(second.CanRead);
    }
}
