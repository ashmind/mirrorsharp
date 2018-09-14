using Microsoft.VisualStudio.Text;

namespace Microsoft.VisualStudio.Language.Intellisense {
    public interface IQuickInfoSourceProvider {
        IQuickInfoSource TryCreateQuickInfoSource(ITextBuffer textBuffer);
    }
}
