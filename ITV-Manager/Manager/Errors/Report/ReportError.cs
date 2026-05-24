using Manager.Errors.Common;

namespace Manager.Errors.Report;

public abstract record ReportError(string Message) : DomainError(Message) {

    public sealed record HtmlError(string Detalles)
        : ReportError($"Error al guardar el informe en html: {Detalles}");

    public sealed record PdfError(string Detalles)
        : ReportError($"Error al guardar el informe en html: {Detalles}");
}