using CSharpFunctionalExtensions;
using Manager.Errors.Common;
using Manager.Models;

namespace Manager.Service.Report;

public interface IReportService {
    
    /// <summary> Exporta una cita a html </summary>
    Result<string, DomainError> ExportarCitaAHtml(Cita cita);
    
    /// <summary> Exporta una cita a pdf </summary>
    Result<string, DomainError> ExportarCitaAPdf(Cita cita);
}