using Manager.Errors.Common;

namespace Manager.Errors.Report;

public static class ReportErrors {
    public static DomainError HtmlError(string detalles) {
        return new ReportError.HtmlError(detalles);
    }

    public static DomainError PdfError(string detalles) {
        return new ReportError.PdfError(detalles);
    }
}