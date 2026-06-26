# language: pt
Funcionalidade: Gestão de candidaturas

  Como recrutador
  Quero gerenciar as candidaturas recebidas para cada vaga
  Para conduzir o processo seletivo de forma organizada, transparente e rastreável

  Contexto:
    Dado que o sistema de gestão de candidaturas está disponível

  # ---------------------------------------------------------------------------
  # REALIZAR CANDIDATURA
  # ---------------------------------------------------------------------------

  @Candidaturas @RealizarCandidatura @Smoke @Critical
  Cenário: Realizar candidatura com candidato e vaga válidos
    Dado que existe um candidato cadastrado no sistema
    E existe uma vaga com status "Aberta" no sistema
    Quando o recrutador registrar a candidatura do candidato para a vaga
    Então a candidatura deve ser registrada com sucesso
    E o sistema deve retornar os dados da candidatura com nome do candidato e título da vaga
    E o status da candidatura deve ser "Em Análise"
    E a data da candidatura deve ser registrada automaticamente

  @Candidaturas @RealizarCandidatura @Critical
  Cenário: Impedir candidatura duplicada do mesmo candidato para a mesma vaga
    Dado que existe uma candidatura ativa de "João Silva" para a vaga "Desenvolvedor Back-end"
    Quando o recrutador tentar registrar uma nova candidatura de "João Silva" para a mesma vaga
    Então o sistema deve recusar a candidatura
    E deve informar que o candidato já se candidatou a esta vaga

  @Candidaturas @RealizarCandidatura @Critical
  Cenário: Impedir candidatura para vaga fechada
    Dado que existe um candidato cadastrado no sistema
    E existe uma vaga com status "Fechada" no sistema
    Quando o recrutador tentar registrar a candidatura do candidato para a vaga fechada
    Então o sistema deve recusar a candidatura
    E deve informar que não é possível se candidatar a uma vaga fechada

  @Candidaturas @RealizarCandidatura
  Cenário: Informar candidato não encontrado ao realizar candidatura com identificador inválido
    Dado que não existe candidato cadastrado com o identificador informado
    E existe uma vaga com status "Aberta" no sistema
    Quando o recrutador tentar registrar a candidatura com o identificador inválido do candidato
    Então o sistema deve informar que o candidato não foi encontrado

  @Candidaturas @RealizarCandidatura
  Cenário: Informar vaga não encontrada ao realizar candidatura com identificador inválido
    Dado que existe um candidato cadastrado no sistema
    E não existe vaga cadastrada com o identificador informado
    Quando o recrutador tentar registrar a candidatura para a vaga com identificador inválido
    Então o sistema deve informar que a vaga não foi encontrada

  @Candidaturas @RealizarCandidatura
  Cenário: Permitir que o mesmo candidato se candidate a vagas diferentes
    Dado que existe um candidato cadastrado no sistema
    E existe uma candidatura do candidato para a vaga "Vaga A"
    E existe uma vaga "Vaga B" com status "Aberta"
    Quando o recrutador registrar a candidatura do mesmo candidato para a vaga "Vaga B"
    Então a candidatura deve ser registrada com sucesso

  @Candidaturas @RealizarCandidatura
  Cenário: Garantir unicidade da combinação candidato-vaga a nível de armazenamento
    Dado que o sistema garante que a combinação candidato-vaga é única
    Então um candidato nunca poderá ter mais de uma candidatura ativa para a mesma vaga
    # Nota: A unicidade é garantida tanto pela regra de negócio no CandidatarSeHandler
    # (verificação via ExisteAsync) quanto por índice composto único no armazenamento
    # (candidatoId + vagaId). Localização: ATS.Infrastructure/Persistence/Repositories/CandidaturaRepository.cs

  # ---------------------------------------------------------------------------
  # CONSULTAR CANDIDATURA
  # ---------------------------------------------------------------------------

  @Candidaturas @ConsultarCandidatura @Smoke
  Cenário: Consultar candidatura existente por identificador
    Dado que existe uma candidatura registrada no sistema
    Quando o recrutador consultar a candidatura pelo seu identificador
    Então o sistema deve retornar os dados completos da candidatura
    E os dados devem incluir nome do candidato, título da vaga, status e data da candidatura

  @Candidaturas @ConsultarCandidatura
  Cenário: Informar candidatura não encontrada ao consultar identificador inexistente
    Dado que não existe candidatura cadastrada com o identificador informado
    Quando o recrutador tentar consultar a candidatura pelo identificador
    Então o sistema deve informar que a candidatura não foi encontrada

  # ---------------------------------------------------------------------------
  # LISTAR CANDIDATOS POR VAGA
  # ---------------------------------------------------------------------------

  @Candidaturas @ListarCandidatosPorVaga @Smoke
  Cenário: Listar todos os candidatos de uma vaga
    Dado que existe uma vaga com candidaturas registradas
    Quando o recrutador solicitar a listagem de candidatos da vaga
    Então o sistema deve retornar a lista de candidaturas da vaga
    E cada candidatura deve incluir nome, e-mail, telefone e indicação de currículo do candidato
    E cada candidatura deve incluir o status e a data da candidatura

  @Candidaturas @ListarCandidatosPorVaga
  Cenário: Retornar lista vazia quando não houver candidaturas para a vaga
    Dado que existe uma vaga sem candidaturas registradas
    Quando o recrutador solicitar a listagem de candidatos da vaga
    Então o sistema deve retornar uma lista vazia

  @Candidaturas @ListarCandidatosPorVaga
  Cenário: Informar vaga não encontrada ao listar candidatos de identificador inválido
    Dado que não existe vaga cadastrada com o identificador informado
    Quando o recrutador tentar listar os candidatos da vaga com identificador inválido
    Então o sistema deve informar que a vaga não foi encontrada

  @Candidaturas @ListarCandidatosPorVaga
  Cenário: Excluir da listagem candidatos que não existem mais no sistema
    Dado que existe uma vaga com candidaturas de candidatos removidos do sistema
    Quando o recrutador solicitar a listagem de candidatos da vaga
    Então os candidatos que não existem mais no sistema não devem aparecer na listagem
    # Nota: O ListCandidatosPorVagaHandler exclui silenciosamente candidaturas
    # cujo candidato não é mais encontrado no repositório (candidato == null → ignora).

  @Candidaturas @ListarCandidatosPorVaga
  Cenário: Listar candidatos de vaga com informação de currículo disponível
    Dado que existe uma vaga com candidaturas
    E um dos candidatos possui currículo vinculado
    E outro candidato não possui currículo vinculado
    Quando o recrutador solicitar a listagem de candidatos da vaga
    Então cada candidatura deve indicar corretamente se o candidato possui ou não currículo
    E para os candidatos com currículo, deve ser informado o nome do arquivo do currículo

  # ---------------------------------------------------------------------------
  # APROVAR CANDIDATURA
  # ---------------------------------------------------------------------------

  @Candidaturas @AprovarCandidatura @Smoke @Critical
  Cenário: Aprovar candidatura em análise sem observações
    Dado que existe uma candidatura com status "Em Análise"
    Quando o recrutador aprovar a candidatura sem informar observações
    Então a candidatura deve ser aprovada com sucesso
    E o status da candidatura deve ser alterado para "Aprovado"
    E as observações devem permanecer em branco

  @Candidaturas @AprovarCandidatura @Critical
  Cenário: Aprovar candidatura em análise com observações
    Dado que existe uma candidatura com status "Em Análise"
    Quando o recrutador aprovar a candidatura informando "Perfil excelente para a vaga."
    Então a candidatura deve ser aprovada com sucesso
    E o status da candidatura deve ser alterado para "Aprovado"
    E as observações devem ser registradas como "Perfil excelente para a vaga."

  @Candidaturas @AprovarCandidatura @Critical
  Esquema do Cenário: Impedir aprovação de candidatura em status que não seja Em Análise
    Dado que existe uma candidatura com status "<status_atual>"
    Quando o recrutador tentar aprovar a candidatura
    Então o sistema deve recusar a aprovação
    E deve informar que somente candidaturas em análise podem ser aprovadas

    Exemplos:
      | status_atual |
      | Aprovado     |
      | Reprovado    |
      | Cancelado    |

  @Candidaturas @AprovarCandidatura
  Cenário: Informar candidatura não encontrada ao tentar aprovar identificador inexistente
    Dado que não existe candidatura cadastrada com o identificador informado
    Quando o recrutador tentar aprovar a candidatura pelo identificador
    Então o sistema deve informar que a candidatura não foi encontrada

  @Candidaturas @AprovarCandidatura
  Cenário: Retornar dados completos da candidatura após aprovação
    Dado que existe uma candidatura com status "Em Análise"
    Quando o recrutador aprovar a candidatura
    Então o sistema deve retornar os dados completos da candidatura
    E os dados devem incluir o nome do candidato e o título da vaga

  # ---------------------------------------------------------------------------
  # REPROVAR CANDIDATURA
  # ---------------------------------------------------------------------------

  @Candidaturas @ReprovarCandidatura @Smoke @Critical
  Cenário: Reprovar candidatura em análise sem observações
    Dado que existe uma candidatura com status "Em Análise"
    Quando o recrutador reprovar a candidatura sem informar observações
    Então a candidatura deve ser reprovada com sucesso
    E o status da candidatura deve ser alterado para "Reprovado"
    E as observações devem permanecer em branco

  @Candidaturas @ReprovarCandidatura @Critical
  Cenário: Reprovar candidatura em análise com observações
    Dado que existe uma candidatura com status "Em Análise"
    Quando o recrutador reprovar a candidatura informando "Experiência insuficiente para o cargo."
    Então a candidatura deve ser reprovada com sucesso
    E o status da candidatura deve ser alterado para "Reprovado"
    E as observações devem ser registradas como "Experiência insuficiente para o cargo."

  @Candidaturas @ReprovarCandidatura @Critical
  Esquema do Cenário: Impedir reprovação de candidatura em status que não seja Em Análise
    Dado que existe uma candidatura com status "<status_atual>"
    Quando o recrutador tentar reprovar a candidatura
    Então o sistema deve recusar a reprovação
    E deve informar que somente candidaturas em análise podem ser reprovadas

    Exemplos:
      | status_atual |
      | Aprovado     |
      | Reprovado    |
      | Cancelado    |

  @Candidaturas @ReprovarCandidatura
  Cenário: Informar candidatura não encontrada ao tentar reprovar identificador inexistente
    Dado que não existe candidatura cadastrada com o identificador informado
    Quando o recrutador tentar reprovar a candidatura pelo identificador
    Então o sistema deve informar que a candidatura não foi encontrada

  @Candidaturas @ReprovarCandidatura
  Cenário: Retornar dados completos da candidatura após reprovação
    Dado que existe uma candidatura com status "Em Análise"
    Quando o recrutador reprovar a candidatura
    Então o sistema deve retornar os dados completos da candidatura
    E os dados devem incluir o nome do candidato e o título da vaga

  # ---------------------------------------------------------------------------
  # CANCELAR CANDIDATURA
  # ---------------------------------------------------------------------------

  @Candidaturas @CancelarCandidatura @Smoke @Critical
  Cenário: Cancelar candidatura em análise
    Dado que existe uma candidatura com status "Em Análise"
    Quando o recrutador solicitar o cancelamento da candidatura
    Então a candidatura deve ser cancelada com sucesso
    E o status da candidatura deve ser alterado para "Cancelado"

  @Candidaturas @CancelarCandidatura @Critical
  Cenário: Cancelar candidatura aprovada
    Dado que existe uma candidatura com status "Aprovado"
    Quando o recrutador solicitar o cancelamento da candidatura
    Então a candidatura deve ser cancelada com sucesso
    E o status da candidatura deve ser alterado para "Cancelado"

  @Candidaturas @CancelarCandidatura @Critical
  Cenário: Cancelar candidatura reprovada
    Dado que existe uma candidatura com status "Reprovado"
    Quando o recrutador solicitar o cancelamento da candidatura
    Então a candidatura deve ser cancelada com sucesso
    E o status da candidatura deve ser alterado para "Cancelado"

  @Candidaturas @CancelarCandidatura @Critical
  Cenário: Impedir cancelamento de candidatura já cancelada
    Dado que existe uma candidatura com status "Cancelado"
    Quando o recrutador tentar cancelar a candidatura novamente
    Então o sistema deve recusar o cancelamento
    E deve informar que a candidatura já foi cancelada

  @Candidaturas @CancelarCandidatura
  Cenário: Informar candidatura não encontrada ao tentar cancelar identificador inexistente
    Dado que não existe candidatura cadastrada com o identificador informado
    Quando o recrutador tentar cancelar a candidatura pelo identificador
    Então o sistema deve informar que a candidatura não foi encontrada

  @Candidaturas @CancelarCandidatura
  Cenário: Retornar dados completos da candidatura após cancelamento
    Dado que existe uma candidatura com status "Em Análise"
    Quando o recrutador cancelar a candidatura
    Então o sistema deve retornar os dados completos da candidatura
    E os dados devem incluir o nome do candidato e o título da vaga

  # ---------------------------------------------------------------------------
  # ESTADOS E TRANSIÇÕES DO CICLO DE VIDA DA CANDIDATURA
  # ---------------------------------------------------------------------------

  @Candidaturas @CicloDeVida @Critical
  Cenário: Candidatura iniciada sempre no estado Em Análise
    Dado que existe um candidato e uma vaga aberta válidos
    Quando a candidatura for registrada
    Então o status inicial da candidatura deve ser "Em Análise"

  @Candidaturas @CicloDeVida
  Esquema do Cenário: Verificar status disponível para transição de aprovação
    Dado que existe uma candidatura com status "<status>"
    Quando o recrutador tentar aprovar a candidatura
    Então o resultado deve ser "<resultado>"

    Exemplos:
      | status    | resultado                                                   |
      | Em Análise | aprovada com sucesso                                       |
      | Aprovado   | recusada - somente candidaturas em análise podem ser aprovadas |
      | Reprovado  | recusada - somente candidaturas em análise podem ser aprovadas |
      | Cancelado  | recusada - somente candidaturas em análise podem ser aprovadas |

  @Candidaturas @CicloDeVida
  Esquema do Cenário: Verificar status disponível para transição de reprovação
    Dado que existe uma candidatura com status "<status>"
    Quando o recrutador tentar reprovar a candidatura
    Então o resultado deve ser "<resultado>"

    Exemplos:
      | status    | resultado                                                    |
      | Em Análise | reprovada com sucesso                                       |
      | Aprovado   | recusada - somente candidaturas em análise podem ser reprovadas |
      | Reprovado  | recusada - somente candidaturas em análise podem ser reprovadas |
      | Cancelado  | recusada - somente candidaturas em análise podem ser reprovadas |

  @Candidaturas @CicloDeVida
  Esquema do Cenário: Verificar status disponível para transição de cancelamento
    Dado que existe uma candidatura com status "<status>"
    Quando o recrutador tentar cancelar a candidatura
    Então o resultado deve ser "<resultado>"

    Exemplos:
      | status    | resultado                           |
      | Em Análise | cancelada com sucesso              |
      | Aprovado   | cancelada com sucesso              |
      | Reprovado  | cancelada com sucesso              |
      | Cancelado  | recusada - candidatura já cancelada |

  # ---------------------------------------------------------------------------
  # DESCRIÇÃO DE STATUS
  # ---------------------------------------------------------------------------

  @Candidaturas @DescricaoStatus
  Esquema do Cenário: Verificar descrição legível de cada status da candidatura
    Dado que existe uma candidatura com status interno "<status_interno>"
    Quando o recrutador consultar a candidatura
    Então a descrição do status deve ser "<descricao_legivel>"

    Exemplos:
      | status_interno | descricao_legivel |
      | EmAnalise      | Em Análise        |
      | Aprovado       | Aprovado          |
      | Reprovado      | Reprovado         |
      | Cancelado      | Cancelado         |
