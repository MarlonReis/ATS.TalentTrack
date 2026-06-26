# language: pt
Funcionalidade: Paginação de resultados

  Como recrutador
  Quero navegar por listas grandes de registros de forma paginada
  Para consultar candidatos e vagas com desempenho adequado sem sobrecarregar o sistema

  Contexto:
    Dado que o sistema de paginação está disponível

  # ---------------------------------------------------------------------------
  # REGRAS GERAIS DE PAGINAÇÃO
  # (aplicam-se a Candidatos e Vagas)
  # ---------------------------------------------------------------------------

  @Paginacao @Smoke
  Cenário: Resultado paginado com primeira e última páginas
    Dado que existem 25 registros no sistema
    Quando o recrutador solicitar a listagem com página 1 e tamanho 10
    Então o sistema deve retornar 10 itens na primeira página
    E o total deve ser 25
    E o total de páginas deve ser 3
    E deve indicar que há próxima página
    E não deve indicar que há página anterior

  @Paginacao
  Cenário: Resultado paginado na última página
    Dado que existem 25 registros no sistema
    Quando o recrutador solicitar a listagem com página 3 e tamanho 10
    Então o sistema deve retornar 5 itens na última página
    E deve indicar que há página anterior
    E não deve indicar que há próxima página

  @Paginacao
  Cenário: Resultado paginado em página intermediária
    Dado que existem 30 registros no sistema
    Quando o recrutador solicitar a listagem com página 2 e tamanho 10
    Então o sistema deve retornar 10 itens
    E deve indicar que há página anterior
    E deve indicar que há próxima página

  @Paginacao
  Cenário: Resultado paginado com todos os registros em uma única página
    Dado que existem 5 registros no sistema
    Quando o recrutador solicitar a listagem com página 1 e tamanho 20
    Então o sistema deve retornar 5 itens
    E o total de páginas deve ser 1
    E não deve indicar que há página anterior
    E não deve indicar que há próxima página

  @Paginacao @Critical
  Esquema do Cenário: Rejeitar número de página inválido
    Quando o recrutador solicitar a listagem com página "<pagina>" e tamanho 20
    Então o sistema deve recusar a solicitação
    E deve informar que o número da página deve ser maior que zero

    Exemplos:
      | pagina |
      | 0      |
      | -1     |
      | -100   |

  @Paginacao @Critical
  Esquema do Cenário: Rejeitar tamanho de página fora dos limites permitidos
    Quando o recrutador solicitar a listagem com página 1 e tamanho "<tamanho>"
    Então o sistema deve recusar a solicitação
    E deve informar que o tamanho da página deve estar entre 1 e 100

    Exemplos:
      | tamanho |
      | 0       |
      | -1      |
      | 101     |
      | 200     |

  @Paginacao
  Cenário: Paginação padrão quando parâmetros não são informados
    Dado que existem registros no sistema
    Quando o recrutador solicitar a listagem sem informar parâmetros de paginação
    Então o sistema deve retornar a primeira página com até 20 itens por padrão
    # Padrão definido nas queries: ListCandidatosQuery(Pagina = 1, TamanhoPagina = 20)
    # e ListVagasQuery(Pagina = 1, TamanhoPagina = 20)

  @Paginacao
  Cenário: Calcular corretamente o total de páginas com divisão não inteira
    Dado que existem 21 registros no sistema
    Quando o recrutador solicitar a listagem com tamanho de 10 por página
    Então o total de páginas deve ser 3
    # Math.Ceiling(21 / 10) = 3

  @Paginacao
  Cenário: Total de páginas igual a zero quando não há registros
    Dado que não existem registros no sistema
    Quando o recrutador solicitar a listagem com qualquer tamanho de página
    Então o total de páginas deve ser 0
    E não deve indicar que há próxima página
    E não deve indicar que há página anterior
