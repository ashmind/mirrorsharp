using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.IO;
using MirrorSharp.FSharp;
using MirrorSharp.FSharp.Advanced;
using MirrorSharp.FSharp.Internal;
using Xunit;

namespace MirrorSharp.Tests.FSharp;

public class FSharpSessionTests {
    [Fact]
    public async Task Compile_ProducesExecutableAssembly() {
        // Arrange
        var session = NewSession(@"
            [<EntryPoint>]
            let main args =
                5
        ");
        var assemblyStream = new MemoryStream();

        // Act
        var compilationResult = await session.CompileAsync(assemblyStream, CancellationToken.None);

        // Assert
        Assert.Empty(compilationResult.Item1);
        var main = Assembly.Load(assemblyStream.ToArray()).EntryPoint;
        Assert.NotNull(main);
        var result = main!.Invoke(null, new[] { new string[0] });
        Assert.Equal(5, result);
    }

    [Theory]
    [InlineData("--debug+")]
    [InlineData("--debug:full")]
    [InlineData("--debug:pdbonly")]
    [InlineData("-g+")]
    [InlineData("-g-")]
    [InlineData("-g:full")]
    [InlineData("-g:pdbonly")]
    public async Task Compile_ThrowsNotSupportedException_IfDebugOptionIsNotSupported(string option) {
        // Arrange
        var session = NewSession();
        session.ProjectOptions = session.ProjectOptions
            .WithoutOtherOption("-debug-")
            .WithOtherOption(option);

        // Act
        var exception = await Record.ExceptionAsync(() => session.CompileAsync(new(), CancellationToken.None).AsTask());

        // Assert
        Assert.IsType<NotSupportedException>(exception);
    }

    private FSharpSession NewSession(string code = "") {
        return new FSharpSession(
            code,
            new MirrorSharpFSharpOptions(),
            new RecyclableMemoryStreamManager()
        );
    }
}
