namespace ATS.Domain.Vagas.Enums;

/// <summary>
/// Representa o estado de ciclo de vida de uma vaga no sistema ATS.
/// </summary>
/// <remarks>
/// Transições permitidas:
/// <code>
///  Rascunho ──► Aberta ──► Fechada
///                 ▲           │
///                 └───────────┘  (Reabrir)
/// </code>
/// </remarks>
public enum StatusVaga
{
    /// <summary>
    /// Vaga criada mas ainda não publicada.
    /// Não aceita candidaturas e não é visível externamente.
    /// </summary>
    Rascunho = 0,

    /// <summary>
    /// Vaga publicada e disponível para candidaturas.
    /// Estado inicial após a publicação via <c>Vaga.Criar()</c>.
    /// </summary>
    Aberta = 1,

    /// <summary>
    /// Vaga encerrada. Não aceita novas candidaturas.
    /// Pode ser reaberta via <c>Vaga.Reabrir()</c>.
    /// </summary>
    Fechada = 2
}
