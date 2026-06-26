# language: pt
Funcionalidade: Tratamento de erros e respostas padronizadas

  Como desenvolvedor integrador do sistema
  Quero que os erros sejam comunicados de forma clara e padronizada
  Para que sistemas clientes possam tratar erros de forma consistente sem expor detalhes internos

  Contexto:
    Dado que o sistema está em operação

  # ---------------------------------------------------------------------------
  # ERROS DE DOMÍNIO (regras de negócio violadas)
  # ---------------------------------------------------------------------------

  @TratamentoDeErros @ErroDominio @Critical
  Cenário: Comunicar erro de negócio relacionado a recurso não encontrado
    Dado que o recrutador solicita uma operação com um identificador que não existe
    Quando a regra de negócio detectar que o recurso não foi encontrado
    Então o sistema deve informar que o recurso não foi encontrado
    E a resposta deve seguir o formato padronizado de erro
    E a resposta deve conter o caminho da requisição como instância do problema
    E a resposta deve conter o identificador de rastreamento da requisição

  @TratamentoDeErros @ErroDominio @Critical
  Cenário: Comunicar erro de negócio relacionado a conflito de estado
    Dado que o recrutador tenta uma operação inválida para o estado atual do recurso
    Quando a regra de negócio detectar o conflito
    Então o sistema deve informar o conflito com a mensagem da regra de negócio
    E a mensagem deve descrever exatamente a restrição de negócio violada

  @TratamentoDeErros @ErroDominio @Critical
  Cenário: Comunicar erro de negócio relacionado a validação de dados
    Dado que o recrutador envia dados inválidos em uma operação
    Quando a regra de negócio detectar a invalidez dos dados
    Então o sistema deve informar que a requisição é inválida
    E a mensagem deve descrever o campo ou regra que causou a rejeição

  @TratamentoDeErros @ErroDominio
  Esquema do Cenário: Mapear erros de negócio para respostas com texto descritivo de negócio
    Dado que ocorre um erro de negócio com a mensagem "<mensagem_dominio>"
    Então a resposta deve informar o status "<descricao_status>"
    E o título da resposta deve ser a própria mensagem de negócio

    Exemplos:
      | mensagem_dominio                                        | descricao_status     |
      | Candidato não encontrado.                               | não encontrado       |
      | Vaga não encontrada.                                    | não encontrado       |
      | Candidatura não encontrada.                             | não encontrado       |
      | Candidato já se candidatou a esta vaga.                 | conflito             |
      | Vaga já está fechada.                                   | conflito             |
      | Somente candidaturas 'Em Análise' podem ser aprovadas.  | conflito             |
      | Somente candidaturas 'Em Análise' podem ser reprovadas. | conflito             |
      | Candidatura já foi cancelada.                           | conflito             |
      | Não é possível editar uma vaga fechada.                 | conflito             |
      | Não é possível se candidatar a uma vaga fechada.        | conflito             |
      | E-mail não pode ser vazio.                              | inválido             |
      | Nome do candidato é obrigatório.                        | inválido             |
      | Título da vaga é obrigatório.                           | inválido             |
      | Salário não pode ser negativo.                          | inválido             |

  # ---------------------------------------------------------------------------
  # ERROS INESPERADOS (exceções internas do sistema)
  # ---------------------------------------------------------------------------

  @TratamentoDeErros @ErroInterno @Critical
  Cenário: Ocultar detalhes internos em erros inesperados do sistema
    Dado que ocorre um erro inesperado interno durante o processamento
    Então o sistema deve responder com indicação de erro interno do servidor
    E a mensagem para o usuário deve ser genérica: "Ocorreu um erro inesperado ao processar a requisição."
    E os detalhes técnicos do erro interno não devem ser expostos ao usuário
    # O ExceptionHandlingMiddleware mascara stack traces e mensagens de exceções
    # não tratadas para não expor detalhes de implementação ao cliente.

  @TratamentoDeErros @ErroInterno
  Cenário: Incluir identificador de rastreamento em todos os erros internos
    Dado que ocorre qualquer tipo de erro durante o processamento
    Então a resposta de erro deve sempre incluir o identificador de rastreamento da requisição
    # O traceId permite correlacionar a resposta com os logs internos sem expor
    # detalhes sensíveis ao usuário. Registrado em extensions.traceId ou traceId
    # dependendo da versão do ASP.NET Core.

  # ---------------------------------------------------------------------------
  # COMPORTAMENTO QUANDO A RESPOSTA JÁ FOI INICIADA
  # ---------------------------------------------------------------------------

  @TratamentoDeErros @RespostaIniciada
  Cenário: Propagar erro quando os cabeçalhos da resposta já foram enviados ao cliente
    Dado que o sistema começou a enviar uma resposta ao cliente
    Quando ocorrer um erro durante o envio da resposta
    Então o sistema não deve tentar sobrescrever a resposta em andamento
    E o erro deve ser propagado normalmente para o pipeline do servidor
    # Comportamento do ExceptionHandlingMiddleware: verifica context.Response.HasStarted
    # antes de tentar serializar o ProblemDetails. Se a resposta já começou, o erro
    # é relançado para que o servidor web lide com a conexão interrompida.

  # ---------------------------------------------------------------------------
  # FORMATO PADRONIZADO DE ERROS (Problem Details - RFC 7807)
  # ---------------------------------------------------------------------------

  @TratamentoDeErros @FormatoProblemDetails @Smoke
  Cenário: Resposta de erro sempre no formato Problem Details
    Dado que ocorre qualquer tipo de erro tratável pelo sistema
    Então a resposta deve estar no tipo de conteúdo "application/problem+json"
    E a resposta deve conter os campos: status, title, detail e instance
    E o campo instance deve conter o caminho da requisição que causou o erro
    E o campo traceId deve conter o identificador único da requisição
