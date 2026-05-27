namespace Manager.Errors.Common;

/// <summary> Contenedor para todos los errores del sistema </summary>
/// <param name="Mensaje">Mensaje de error</param>
public abstract record DomainError(string Mensaje);