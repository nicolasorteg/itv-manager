using System.IO;
using System.Text;
using CSharpFunctionalExtensions;
using Manager.Config;
using Manager.Errors.Common;
using Manager.Errors.Report;
using Manager.Models;
using SelectPdf;

namespace Manager.Service.Report;

public class ReportService: IReportService {
    
    private readonly string _directorioDestino = AppConfig.ReportDirectory;
    
    public ReportService() {
        if (!Directory.Exists(_directorioDestino)) Directory.CreateDirectory(_directorioDestino);cd .._directorioDestino
    }
    
    /// <inheritdoc cref="IReportService.ExportarCitaAHtml" />
    public Result<string, DomainError> ExportarCitaAHtml(Cita cita) {
        
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var nombreArchivo = $"FichaITV_{cita.Matricula}_{timestamp}.html";
        var rutaCompleta = Path.Combine(_directorioDestino, nombreArchivo);

        try {
            var contenidoHtml = GenerarPlantillaHtml(cita);
            File.WriteAllText(rutaCompleta, contenidoHtml, Encoding.UTF8);
            
            return Result.Success<string, DomainError>(rutaCompleta);
        }
        catch (Exception)
        {
            return Result.Failure<string, DomainError>(new ReportError.HtmlError(""));
        }
    }
    
    /// <inheritdoc cref="IReportService.ExportarCitaAPdf" />
    public Result<string, DomainError> ExportarCitaAPdf(Cita cita) {
        
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var nombreArchivo = $"FichaITV_{cita.Matricula}_{timestamp}.pdf";
        var rutaCompleta = Path.Combine(_directorioDestino, nombreArchivo);

        try {
            var contenidoHtml = GenerarPlantillaHtml(cita);

            // config del conversor de SelectPdf
            var conversor = new HtmlToPdf {
                Options = {
                    PdfPageSize = PdfPageSize.A4,
                    PdfPageOrientation = PdfPageOrientation.Portrait,
                    MarginLeft = 10,
                    MarginRight = 10,
                    MarginTop = 10,
                    MarginBottom = 10
                }
            };

            // conver y guardado
            var documento = conversor.ConvertHtmlString(contenidoHtml);
            documento.Save(rutaCompleta);
            documento.Close();

            return Result.Success<string, DomainError>(rutaCompleta);
        }
        catch (Exception) {
            return Result.Failure<string, DomainError>(new ReportError.PdfError(""));
        }
    }
    
    private string GenerarPlantillaHtml(Cita cita)
    {
        return $@"
        <!DOCTYPE html>
        <html lang='es'>
        <head>
            <meta charset='UTF-8'>
            <title>Comprobante Cita ITV</title>
            <style>
                body {{
                    font-family: 'Segoe UI', Arial, sans-serif;
                    color: #333333;
                    background-color: #ffffff;
                    margin: 0;
                    padding: 30px;
                }}
                .tarjeta-itv {{
                    max-width: 650px;
                    margin: 0 auto;
                    border: 2px solid #2B4C7E;
                    border-radius: 6px;
                    box-shadow: 0 3px 6px rgba(0,0,0,0.1);
                }}
                .cabecera {{
                    background-color: #2B4C7E;
                    color: #ffffff;
                    text-align: center;
                    padding: 25px;
                }}
                .cabecera h2 {{
                    margin: 0;
                    font-size: 22px;
                    letter-spacing: 1px;
                }}
                .linea-decorativa {{
                    height: 4px;
                    background-color: #4A90E2;
                    width: 60px;
                    margin: 8px auto 0 auto;
                }}
                .bloque-cuerpo {{
                    padding: 30px;
                }}
                .fila-datos {{
                    display: flex;
                    justify-content: space-between;
                    margin-bottom: 18px;
                    border-bottom: 1px solid #f0f0f0;
                    padding-bottom: 8px;
                }}
                .etiqueta {{
                    font-weight: bold;
                    color: #555555;
                    font-size: 13px;
                    text-transform: uppercase;
                }}
                .valor {{
                    color: #111111;
                    font-size: 15px;
                }}
                .matricula-destacada {{
                    color: #2B4C7E;
                    font-weight: bold;
                    font-size: 18px;
                }}
                .fecha-destacada {{
                    color: #2e7d32;
                    font-weight: bold;
                }}
                .pie-pagina {{
                    text-align: center;
                    background-color: #f9f9f9;
                    padding: 15px;
                    font-size: 12px;
                    color: #777777;
                    border-top: 1px solid #ebeeef;
                }}
            </style>
        </head>
        <body>
            <div class='tarjeta-itv'>
                <div class='cabecera'>
                    <h2>SISTEMA DE GESTIÓN ITV LUIS VIVES</h2>
                    <div class='linea-decorativa'></div>
                </div>
                <div class='bloque-cuerpo'>
                    <div class='fila-datos'>
                        <span class='etiqueta'>Matrícula Comercial</span>
                        <span class='valor matricula-destacada'>{cita.Matricula}</span>
                    </div>
                    <div class='fila-datos'>
                        <span class='etiqueta'>Marca del Vehículo</span>
                        <span class='valor'>{cita.Marca}</span>
                    </div>
                    <div class='fila-datos'>
                        <span class='etiqueta'>Modelo</span>
                        <span class='valor'>{cita.Modelo}</span>
                    </div>
                    <div class='fila-datos'>
                        <span class='etiqueta'>Sistema de Propulsión</span>
                        <span class='valor'>{cita.Motor}</span>
                    </div>
                    <div class='fila-datos'>
                        <span class='etiqueta'>Identificación Propietario (DNI)</span>
                        <span class='valor'>{cita.Dni}</span>
                    </div>
                    <div class='fila-datos'>
                        <span class='etiqueta'>Fecha Matriculación</span>
                        <span class='valor'>{cita.FechaMatriculacion:dd/MM/yyyy}</span>
                    </div>
                    <div class='fila-datos'>
                        <span class='etiqueta'>Planificación Inspección</span>
                        <span class='valor fecha-destacada'>{cita.FechaInspeccion:dd/MM/yyyy HH:mm}</span>
                    </div>
                </div>
                <div class='pie-pagina'>
                    Documento oficial emitido de forma automatizada el {DateTime.Now:dd/MM/yyyy a las HH:mm:ss}
                </div>
            </div>
        </body>
        </html>";
    }
}