using System.IO;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Manager.Config;
using Manager.Models;
using Manager.Service.Report;
using Microsoft.Win32;

namespace Manager.ViewModels.Informe;

public partial class InformeViewModel(IReportService reportService) : ObservableObject {
    
    [RelayCommand(CanExecute = nameof(CanExportar))]
    private void ExportarCita(Cita? cita) {
        if (cita == null) return;

        var result = MessageBox.Show(
            "¿Desea exportar la ficha en formato PDF?\n\nSÍ -> PDF\nNO -> HTML", 
            "Seleccionar Formato de Ficha", MessageBoxButton.YesNoCancel, MessageBoxImage.Question
        );

        if (result == MessageBoxResult.Cancel) return;

        var saveFileDialog = new SaveFileDialog {
            FileName = $"FichaITV_{cita.Matricula}",
            InitialDirectory = AppConfig.ReportDirectory
        };

        switch (result) {
            case MessageBoxResult.Yes: {
                saveFileDialog.Filter = "Documento PDF (*.pdf)|*.pdf";
                saveFileDialog.DefaultExt = "pdf";

                if (saveFileDialog.ShowDialog() == true) {
                    var resPdf = reportService.ExportarCitaAPdf(cita);
                    if (resPdf.IsSuccess && File.Exists(resPdf.Value)) {
                        File.Move(resPdf.Value, saveFileDialog.FileName, overwrite: true);
                        MessageBox.Show($"¡Ficha PDF guardada correctamente!", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }

                break;
            }
            case MessageBoxResult.No: {
                saveFileDialog.Filter = "Archivo Web HTML (*.html)|*.html";
                saveFileDialog.DefaultExt = "html";

                if (saveFileDialog.ShowDialog() == true) {
                    var resHtml = reportService.ExportarCitaAHtml(cita);
                    if (resHtml.IsSuccess && File.Exists(resHtml.Value)) {
                        File.Move(resHtml.Value, saveFileDialog.FileName, overwrite: true);
                        MessageBox.Show($"¡Ficha HTML generada correctamente!", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }

                break;
            }
            case MessageBoxResult.None:
            case MessageBoxResult.OK:
            case MessageBoxResult.Cancel:
            case MessageBoxResult.Abort:
            case MessageBoxResult.Retry:
            case MessageBoxResult.Ignore:
            case MessageBoxResult.TryAgain:
            case MessageBoxResult.Continue:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    /// <summary> Comprueba que una cita no sea nula </summary>
    private bool CanExportar(Cita? cita) => cita != null;
}