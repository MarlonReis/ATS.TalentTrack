# language: pt
Funcionalidade: Gestão de candidatos

  Como recrutador
  Quero gerenciar o cadastro de candidatos no sistema
  Para manter uma base de talentos organizada e disponível para os processos seletivos

  Contexto:
    Dado que o sistema de gestão de candidatos está disponível

  # ---------------------------------------------------------------------------
  # CADASTRAR CANDIDATO
  # ---------------------------------------------------------------------------

  @Candidatos @CadastrarCandidato @Smoke @Critical
  Cenário: Cadastrar candidato com dados válidos
    Dado que não existe candidato cadastrado com o e-mail "joao.silva@email.com"
    Quando o recrutador cadastrar um candidato com os dados:
      | Nome       | E-mail                   | Telefone     |
      | João Silva | joao.silva@email.com     | 11912345678  |
    Então o candidato deve ser cadastrado com sucesso
    E o sistema deve retornar os dados do candidato cadastrado
    E o candidato deve possuir um identificador único gerado automaticamente
    E a data de cadastro deve ser registrada automaticamente

  @Candidatos @CadastrarCandidato @Critical
  Cenário: Impedir cadastro de candidato com e-mail já existente
    Dado que já existe um candidato cadastrado com o e-mail "maria@email.com"
    Quando o recrutador tentar cadastrar outro candidato com o mesmo e-mail "maria@email.com"
    Então o sistema deve recusar o cadastro
    E deve informar que já existe um candidato com o e-mail "maria@email.com"

  @Candidatos @CadastrarCandidato
  Esquema do Cenário: Impedir cadastro de candidato com nome inválido
    Dado que o recrutador está cadastrando um novo candidato
    Quando informar o nome "<nome>" com e-mail "teste@email.com" e telefone "11912345678"
    Então o sistema deve recusar o cadastro
    E deve informar que o nome do candidato é obrigatório

    Exemplos:
      | nome  |
      |       |
      |   ·   |

  @Candidatos @CadastrarCandidato
  Cenário: Impedir cadastro de candidato com nome superior a 150 caracteres
    Dado que o recrutador está cadastrando um novo candidato
    Quando informar um nome com 151 caracteres, e-mail "teste@email.com" e telefone "11912345678"
    Então o sistema deve recusar o cadastro
    E deve informar que o nome não pode exceder 150 caracteres

  @Candidatos @CadastrarCandidato
  Esquema do Cenário: Impedir cadastro de candidato com e-mail em formato inválido
    Dado que o recrutador está cadastrando um novo candidato
    Quando informar nome "Carlos" com e-mail "<email>" e telefone "11912345678"
    Então o sistema deve recusar o cadastro
    E deve informar que o e-mail possui formato inválido

    Exemplos:
      | email            |
      | semdoble         |
      | @semdominio.com  |
      | sem@ponto        |

  @Candidatos @CadastrarCandidato
  Esquema do Cenário: Impedir cadastro de candidato com e-mail vazio
    Dado que o recrutador está cadastrando um novo candidato
    Quando informar nome "Carlos" com e-mail "<email>" e telefone "11912345678"
    Então o sistema deve recusar o cadastro
    E deve informar que o e-mail não pode ser vazio

    Exemplos:
      | email |
      |       |
      |   ·   |

  @Candidatos @CadastrarCandidato
  Esquema do Cenário: Impedir cadastro de candidato com telefone inválido
    Dado que o recrutador está cadastrando um novo candidato
    Quando informar nome "Carlos" com e-mail "carlos@email.com" e telefone "<telefone>"
    Então o sistema deve recusar o cadastro
    E deve informar que o telefone é inválido

    Exemplos:
      | telefone   | descricao              |
      |            | vazio                  |
      | 1234567    | menos de 10 dígitos    |
      | 123456789012 | mais de 11 dígitos   |

  @Candidatos @CadastrarCandidato
  Esquema do Cenário: Aceitar telefones com formatações diferentes mas que resultem em 10 ou 11 dígitos
    Dado que não existe candidato cadastrado com o e-mail "teste@email.com"
    Quando o recrutador cadastrar um candidato com telefone "<telefone_formatado>"
    Então o candidato deve ser cadastrado com sucesso
    E o telefone deve ser armazenado somente com os dígitos numéricos

    Exemplos:
      | telefone_formatado |
      | (11) 91234-5678    |
      | 11912345678        |
      | 1191234-5678       |
      | (11)1234-5678      |

  @Candidatos @CadastrarCandidato
  Cenário: Normalizar e-mail para letras minúsculas ao cadastrar
    Dado que não existe candidato cadastrado com o e-mail "ana@email.com"
    Quando o recrutador cadastrar um candidato com o e-mail "ANA@EMAIL.COM"
    Então o candidato deve ser cadastrado com sucesso
    E o e-mail deve ser armazenado em letras minúsculas como "ana@email.com"

  @Candidatos @CadastrarCandidato
  Cenário: Remover espaços extras do nome ao cadastrar
    Dado que não existe candidato cadastrado com o e-mail "teste@email.com"
    Quando o recrutador cadastrar um candidato com o nome "  Carlos  "
    Então o candidato deve ser cadastrado com sucesso
    E o nome deve ser armazenado sem os espaços extras como "Carlos"

  # ---------------------------------------------------------------------------
  # CONSULTAR CANDIDATO
  # ---------------------------------------------------------------------------

  @Candidatos @ConsultarCandidato @Smoke
  Cenário: Consultar candidato existente por identificador
    Dado que existe um candidato cadastrado com o nome "João Silva"
    Quando o recrutador consultar o candidato pelo seu identificador
    Então o sistema deve retornar os dados completos do candidato
    E os dados devem incluir nome, e-mail, telefone e data de cadastro

  @Candidatos @ConsultarCandidato
  Cenário: Informar candidato não encontrado ao consultar identificador inexistente
    Dado que não existe candidato cadastrado com o identificador informado
    Quando o recrutador tentar consultar o candidato pelo identificador
    Então o sistema deve informar que o candidato não foi encontrado

  # ---------------------------------------------------------------------------
  # LISTAR CANDIDATOS
  # ---------------------------------------------------------------------------

  @Candidatos @ListarCandidatos @Smoke
  Cenário: Listar candidatos com paginação padrão
    Dado que existem candidatos cadastrados no sistema
    Quando o recrutador solicitar a listagem de candidatos sem informar parâmetros de paginação
    Então o sistema deve retornar a primeira página com até 20 candidatos
    E o resultado deve informar o total geral de candidatos
    E o resultado deve informar o número total de páginas

  @Candidatos @ListarCandidatos
  Cenário: Listar candidatos com paginação personalizada
    Dado que existem 50 candidatos cadastrados no sistema
    Quando o recrutador solicitar a página 2 com tamanho de 10 candidatos por página
    Então o sistema deve retornar até 10 candidatos
    E o resultado deve informar que há páginas anteriores
    E o resultado deve informar que há próximas páginas

  @Candidatos @ListarCandidatos
  Cenário: Indicar disponibilidade de páginas anteriores e próximas na listagem
    Dado que existem 60 candidatos cadastrados no sistema
    Quando o recrutador solicitar a página 3 com tamanho de 10 candidatos por página
    Então o resultado deve indicar que há página anterior
    E o resultado deve indicar que há próxima página
    E o total de páginas deve ser 6

  @Candidatos @ListarCandidatos
  Esquema do Cenário: Rejeitar parâmetros de paginação inválidos
    Dado que existem candidatos cadastrados no sistema
    Quando o recrutador solicitar a listagem com página "<pagina>" e tamanho "<tamanho>"
    Então o sistema deve recusar a listagem
    E deve informar que o parâmetro de paginação é inválido

    Exemplos:
      | pagina | tamanho | motivo                         |
      | 0      | 20      | página menor que 1             |
      | -1     | 20      | página negativa                |
      | 1      | 0       | tamanho menor que 1            |
      | 1      | 101     | tamanho superior a 100         |
      | 1      | -5      | tamanho negativo               |

  # ---------------------------------------------------------------------------
  # ATUALIZAR CANDIDATO
  # ---------------------------------------------------------------------------

  @Candidatos @AtualizarCandidato @Smoke
  Cenário: Atualizar dados de contato de candidato existente
    Dado que existe um candidato cadastrado com o e-mail "antigo@email.com"
    Quando o recrutador atualizar o candidato com nome "Novo Nome", e-mail "novo@email.com" e telefone "11988887777"
    Então os dados do candidato devem ser atualizados com sucesso
    E o sistema deve retornar os dados atualizados do candidato

  @Candidatos @AtualizarCandidato @Critical
  Cenário: Impedir atualização de candidato para um e-mail já em uso por outro candidato
    Dado que existe um candidato "João" com e-mail "joao@email.com"
    E existe um candidato "Maria" com e-mail "maria@email.com"
    Quando o recrutador tentar atualizar "João" para usar o e-mail "maria@email.com"
    Então o sistema deve recusar a atualização
    E deve informar que já existe outro candidato com o e-mail "maria@email.com"

  @Candidatos @AtualizarCandidato
  Cenário: Permitir atualizar candidato mantendo o mesmo e-mail
    Dado que existe um candidato com e-mail "mesmo@email.com"
    Quando o recrutador atualizar apenas o nome do candidato mantendo o e-mail "mesmo@email.com"
    Então os dados do candidato devem ser atualizados com sucesso

  @Candidatos @AtualizarCandidato
  Cenário: Informar candidato não encontrado ao tentar atualizar identificador inexistente
    Dado que não existe candidato cadastrado com o identificador informado
    Quando o recrutador tentar atualizar o candidato pelo identificador
    Então o sistema deve informar que o candidato não foi encontrado

  # ---------------------------------------------------------------------------
  # EXCLUIR CANDIDATO
  # ---------------------------------------------------------------------------

  @Candidatos @ExcluirCandidato @Smoke
  Cenário: Excluir candidato existente
    Dado que existe um candidato cadastrado com um identificador conhecido
    Quando o recrutador solicitar a exclusão do candidato pelo identificador
    Então o candidato deve ser excluído com sucesso
    E o sistema não deve retornar conteúdo como resposta

  @Candidatos @ExcluirCandidato
  Cenário: Informar candidato não encontrado ao tentar excluir identificador inexistente
    Dado que não existe candidato cadastrado com o identificador informado
    Quando o recrutador tentar excluir o candidato pelo identificador
    Então o sistema deve informar que o candidato não foi encontrado

  # ---------------------------------------------------------------------------
  # ADICIONAR CURRÍCULO
  # ---------------------------------------------------------------------------

  @Candidatos @Curriculo @Smoke
  Cenário: Adicionar currículo em formato PDF ao candidato
    Dado que existe um candidato cadastrado sem currículo
    Quando o recrutador adicionar o currículo "curriculo-joao.pdf" ao candidato
    Então o currículo deve ser vinculado ao candidato com sucesso
    E o sistema deve registrar automaticamente a data de upload do currículo
    E o sistema deve retornar os dados atualizados do candidato com o currículo vinculado

  @Candidatos @Curriculo
  Esquema do Cenário: Aceitar currículo nos formatos permitidos
    Dado que existe um candidato cadastrado sem currículo
    Quando o recrutador adicionar o currículo com o arquivo "<nome_arquivo>" ao candidato
    Então o currículo deve ser vinculado ao candidato com sucesso

    Exemplos:
      | nome_arquivo          |
      | curriculo.pdf         |
      | curriculo.doc         |
      | curriculo.docx        |
      | CURRICULO.PDF         |
      | MeuCurriculo.DOC      |

  @Candidatos @Curriculo @Critical
  Esquema do Cenário: Rejeitar currículo em formato não permitido
    Dado que existe um candidato cadastrado
    Quando o recrutador tentar adicionar um currículo com o arquivo "<nome_arquivo>"
    Então o sistema deve recusar o upload do currículo
    E deve informar que o formato não é permitido e que apenas PDF, DOC e DOCX são aceitos

    Exemplos:
      | nome_arquivo        |
      | curriculo.txt       |
      | curriculo.xlsx      |
      | curriculo.jpg       |
      | curriculo.png       |
      | curriculo.zip       |

  @Candidatos @Curriculo
  Cenário: Impedir adição de currículo sem nome de arquivo
    Dado que existe um candidato cadastrado
    Quando o recrutador tentar adicionar um currículo sem informar o nome do arquivo
    Então o sistema deve recusar o upload do currículo
    E deve informar que o nome do arquivo do currículo é obrigatório

  @Candidatos @Curriculo
  Cenário: Impedir adição de currículo sem conteúdo
    Dado que existe um candidato cadastrado
    Quando o recrutador tentar adicionar um currículo "curriculo.pdf" sem informar o conteúdo
    Então o sistema deve recusar o upload do currículo
    E deve informar que o conteúdo do currículo é obrigatório

  @Candidatos @Curriculo
  Cenário: Substituir currículo anterior ao adicionar novo currículo
    Dado que existe um candidato cadastrado com o currículo "antigo.pdf" já vinculado
    Quando o recrutador adicionar um novo currículo "novo.pdf" ao mesmo candidato
    Então o novo currículo deve substituir o anterior
    E o candidato deve possuir apenas o currículo "novo.pdf"

  @Candidatos @Curriculo
  Cenário: Informar candidato não encontrado ao tentar adicionar currículo a identificador inexistente
    Dado que não existe candidato cadastrado com o identificador informado
    Quando o recrutador tentar adicionar um currículo ao candidato
    Então o sistema deve informar que o candidato não foi encontrado

  # ---------------------------------------------------------------------------
  # UNICIDADE DE E-MAIL (banco de dados)
  # ---------------------------------------------------------------------------

  @Candidatos @Integridade
  Cenário: Garantir unicidade de e-mail a nível de armazenamento
    Dado que existe um candidato cadastrado com o e-mail "existente@email.com"
    Quando qualquer tentativa de cadastrar outro candidato com o mesmo e-mail ocorrer
    Então o sistema deve impedir o cadastro de forma definitiva
    # Nota: A unicidade de e-mail é garantida tanto pela regra de negócio no handler
    # quanto por índice único criado automaticamente no armazenamento (email.value).
    # A verificação no handler ocorre antes da persistência para fornecer mensagem amigável.
