# language: pt
Funcionalidade: Gestão de vagas

  Como recrutador
  Quero gerenciar as vagas abertas na empresa
  Para atrair candidatos qualificados e controlar o ciclo de vida de cada oportunidade de trabalho

  Contexto:
    Dado que o sistema de gestão de vagas está disponível

  # ---------------------------------------------------------------------------
  # PUBLICAR VAGA
  # ---------------------------------------------------------------------------

  @Vagas @PublicarVaga @Smoke @Critical
  Cenário: Publicar vaga com dados válidos
    Quando o recrutador publicar uma vaga com os dados:
      | Título                | Descrição                       | Requisitos              | Salário  |
      | Desenvolvedor Back-end | Desenvolvimento de APIs REST   | .NET, MongoDB, Docker  | 12000.00 |
    Então a vaga deve ser publicada com sucesso
    E o sistema deve retornar os dados da vaga publicada
    E a vaga deve possuir um identificador único gerado automaticamente
    E o status da vaga deve ser "Aberta"
    E a data de abertura deve ser registrada automaticamente

  @Vagas @PublicarVaga
  Cenário: Publicar vaga sem informar requisitos
    Quando o recrutador publicar uma vaga com título e descrição mas sem informar requisitos
    Então a vaga deve ser publicada com sucesso
    E os requisitos devem ser registrados como texto vazio

  @Vagas @PublicarVaga
  Cenário: Publicar vaga com salário zero quando o valor não for divulgado
    Quando o recrutador publicar uma vaga com salário igual a zero
    Então a vaga deve ser publicada com sucesso
    E o salário deve ser registrado como zero na moeda padrão

  @Vagas @PublicarVaga
  Cenário: Publicar vaga com a moeda padrão BRL quando não informada
    Quando o recrutador publicar uma vaga sem informar a moeda
    Então a vaga deve ser publicada com sucesso
    E a moeda deve ser registrada automaticamente como "BRL"

  @Vagas @PublicarVaga @Critical
  Esquema do Cenário: Rejeitar publicação de vaga com título inválido
    Dado que o recrutador está publicando uma nova vaga
    Quando informar o título "<titulo>" com uma descrição válida
    Então o sistema deve recusar a publicação da vaga
    E deve informar que o título da vaga é obrigatório

    Exemplos:
      | titulo |
      |        |
      |   ·    |

  @Vagas @PublicarVaga
  Cenário: Rejeitar publicação de vaga com título superior a 200 caracteres
    Dado que o recrutador está publicando uma nova vaga
    Quando informar um título com 201 caracteres
    Então o sistema deve recusar a publicação da vaga
    E deve informar que o título não pode exceder 200 caracteres

  @Vagas @PublicarVaga @Critical
  Esquema do Cenário: Rejeitar publicação de vaga com descrição inválida
    Dado que o recrutador está publicando uma nova vaga
    Quando informar um título válido com a descrição "<descricao>"
    Então o sistema deve recusar a publicação da vaga
    E deve informar que a descrição da vaga é obrigatória

    Exemplos:
      | descricao |
      |           |
      |     ·     |

  @Vagas @PublicarVaga
  Cenário: Rejeitar publicação de vaga com salário negativo
    Dado que o recrutador está publicando uma nova vaga com dados válidos de título e descrição
    Quando informar um salário negativo
    Então o sistema deve recusar a publicação da vaga
    E deve informar que o salário não pode ser negativo

  @Vagas @PublicarVaga
  Cenário: Remover espaços extras do título e da descrição ao publicar
    Quando o recrutador publicar uma vaga com título "  Dev Back-end  " e descrição "  Descrição  "
    Então a vaga deve ser publicada com sucesso
    E o título deve ser armazenado sem espaços extras como "Dev Back-end"
    E a descrição deve ser armazenada sem espaços extras como "Descrição"

  # ---------------------------------------------------------------------------
  # CONSULTAR VAGA
  # ---------------------------------------------------------------------------

  @Vagas @ConsultarVaga @Smoke
  Cenário: Consultar vaga existente por identificador
    Dado que existe uma vaga publicada com o título "Desenvolvedor Back-end"
    Quando o recrutador consultar a vaga pelo seu identificador
    Então o sistema deve retornar os dados completos da vaga
    E os dados devem incluir título, descrição, requisitos, salário, status e data de abertura

  @Vagas @ConsultarVaga
  Cenário: Informar vaga não encontrada ao consultar identificador inexistente
    Dado que não existe vaga cadastrada com o identificador informado
    Quando o recrutador tentar consultar a vaga pelo identificador
    Então o sistema deve informar que a vaga não foi encontrada

  # ---------------------------------------------------------------------------
  # LISTAR VAGAS
  # ---------------------------------------------------------------------------

  @Vagas @ListarVagas @Smoke
  Cenário: Listar vagas com paginação padrão
    Dado que existem vagas cadastradas no sistema
    Quando o recrutador solicitar a listagem de vagas sem informar parâmetros
    Então o sistema deve retornar a primeira página com até 20 vagas
    E o resultado deve informar o total geral de vagas
    E o resultado deve informar o número total de páginas

  @Vagas @ListarVagas
  Cenário: Filtrar vagas abertas na listagem
    Dado que existem vagas com status "Aberta" e "Fechada" no sistema
    Quando o recrutador solicitar a listagem filtrando apenas vagas abertas
    Então o sistema deve retornar somente as vagas com status "Aberta"

  @Vagas @ListarVagas
  Cenário: Filtrar vagas fechadas na listagem
    Dado que existem vagas com status "Aberta" e "Fechada" no sistema
    Quando o recrutador solicitar a listagem filtrando apenas vagas fechadas
    Então o sistema deve retornar somente as vagas com status "Fechada"

  @Vagas @ListarVagas
  Cenário: Listar todas as vagas sem filtro de status
    Dado que existem vagas com status "Aberta" e "Fechada" no sistema
    Quando o recrutador solicitar a listagem sem informar filtro de status
    Então o sistema deve retornar vagas de todos os status

  @Vagas @ListarVagas
  Esquema do Cenário: Rejeitar parâmetros de paginação inválidos na listagem de vagas
    Dado que existem vagas cadastradas no sistema
    Quando o recrutador solicitar a listagem com página "<pagina>" e tamanho "<tamanho>"
    Então o sistema deve recusar a listagem
    E deve informar que o parâmetro de paginação é inválido

    Exemplos:
      | pagina | tamanho | motivo                |
      | 0      | 20      | página menor que 1    |
      | -1     | 20      | página negativa       |
      | 1      | 0       | tamanho menor que 1   |
      | 1      | 101     | tamanho acima do máximo |
      | 1      | -1      | tamanho negativo      |

  # ---------------------------------------------------------------------------
  # ATUALIZAR VAGA
  # ---------------------------------------------------------------------------

  @Vagas @AtualizarVaga @Smoke
  Cenário: Atualizar dados de vaga aberta
    Dado que existe uma vaga com status "Aberta"
    Quando o recrutador atualizar o título, a descrição, os requisitos e o salário da vaga
    Então os dados da vaga devem ser atualizados com sucesso
    E o sistema deve retornar os dados atualizados da vaga

  @Vagas @AtualizarVaga @Critical
  Cenário: Impedir atualização de vaga fechada
    Dado que existe uma vaga com status "Fechada"
    Quando o recrutador tentar atualizar os dados da vaga
    Então o sistema deve recusar a atualização
    E deve informar que não é possível editar uma vaga fechada

  @Vagas @AtualizarVaga
  Cenário: Informar vaga não encontrada ao tentar atualizar identificador inexistente
    Dado que não existe vaga cadastrada com o identificador informado
    Quando o recrutador tentar atualizar a vaga pelo identificador
    Então o sistema deve informar que a vaga não foi encontrada

  @Vagas @AtualizarVaga
  Cenário: Rejeitar atualização de vaga com salário negativo
    Dado que existe uma vaga com status "Aberta"
    Quando o recrutador tentar atualizar a vaga informando salário negativo
    Então o sistema deve recusar a atualização
    E deve informar que o salário não pode ser negativo

  # ---------------------------------------------------------------------------
  # FECHAR VAGA
  # ---------------------------------------------------------------------------

  @Vagas @FecharVaga @Smoke @Critical
  Cenário: Fechar vaga aberta encerrando o processo seletivo
    Dado que existe uma vaga com status "Aberta"
    Quando o recrutador solicitar o encerramento da vaga
    Então a vaga deve ser encerrada com sucesso
    E o status da vaga deve ser alterado para "Fechada"
    E a data de encerramento deve ser registrada automaticamente

  @Vagas @FecharVaga @Critical
  Cenário: Impedir encerramento de vaga já fechada
    Dado que existe uma vaga com status "Fechada"
    Quando o recrutador tentar encerrar a vaga novamente
    Então o sistema deve recusar o encerramento
    E deve informar que a vaga já está fechada

  @Vagas @FecharVaga
  Cenário: Informar vaga não encontrada ao tentar fechar identificador inexistente
    Dado que não existe vaga cadastrada com o identificador informado
    Quando o recrutador tentar fechar a vaga pelo identificador
    Então o sistema deve informar que a vaga não foi encontrada

  # ---------------------------------------------------------------------------
  # EXCLUIR VAGA
  # ---------------------------------------------------------------------------

  @Vagas @ExcluirVaga @Smoke
  Cenário: Excluir vaga existente
    Dado que existe uma vaga cadastrada com um identificador conhecido
    Quando o recrutador solicitar a exclusão da vaga pelo identificador
    Então a vaga deve ser excluída com sucesso
    E o sistema não deve retornar conteúdo como resposta

  @Vagas @ExcluirVaga
  Cenário: Informar vaga não encontrada ao tentar excluir identificador inexistente
    Dado que não existe vaga cadastrada com o identificador informado
    Quando o recrutador tentar excluir a vaga pelo identificador
    Então o sistema deve informar que a vaga não foi encontrada

  # ---------------------------------------------------------------------------
  # CICLO DE VIDA DA VAGA
  # ---------------------------------------------------------------------------

  @Vagas @CicloDeVida
  Cenário: Vaga é publicada sempre no estado Aberta
    Quando o recrutador publicar uma nova vaga com dados válidos
    Então a vaga deve ser criada com status "Aberta"
    E a vaga deve estar imediatamente disponível para receber candidaturas

  @Vagas @CicloDeVida
  Cenário: Vaga fechada não aceita novas candidaturas
    Dado que existe uma vaga com status "Fechada"
    Quando um candidato tentar se candidatar à vaga
    Então o sistema deve recusar a candidatura
    E deve informar que não é possível se candidatar a uma vaga fechada
    # Regra verificada pelo CandidatarSeHandler antes de criar a candidatura.

  # ---------------------------------------------------------------------------
  # NOTA DE IMPLEMENTAÇÃO
  # ---------------------------------------------------------------------------
  # O domínio da Vaga define o método Reabrir(), que permite reverter o status
  # de "Fechada" para "Aberta". Entretanto, não existe caso de uso (handler)
  # exposto que invoque esse método. A reabertura de vaga é uma funcionalidade
  # parcialmente implementada no domínio mas não está disponível operacionalmente.
  # Localização: ATS.Domain/Vagas/Entities/Vaga.cs - método Reabrir()
  # Localização: ATS.Domain/Vagas/Enums/StatusVaga.cs - transições documentadas nos comentários do enum
