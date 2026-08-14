# Game Design Document - Day of Cleansing

## Theme / Setting / Genre

  - Tema: Ficção científica e Distopia.
  - Ambientação: Arena que simula um bairro destruído e abandonado dentro de uma cidade cyberpunk na qual você foge de um robô que está lá para te matar.
  - Gênero: Terror de Sobrevivência/Psicológico.

## Core Gameplay Mechanics Brief

  - Sistema avançado e dinâmico de procura da IA
  - Controle dinâmico do jogador
  - Sistema de som universal
  - Comunicação entre computador e o celular
  - Sistema de coletar itens

## Targeted platforms
  - Computador

## Monetization model (Brief/Document) 
  - A primeira fase do jogo (a que será desenvolvida para a feira de jogos) seria lançada gratuitamente, com o passar do tempo, mais fases seria lançadas em forma de DLCs, que seriam vendidas em lojas de jogos.
  - Cada fase poderia ser patrocinada por uma empresa que teria sua imagem exibida por todas as partes dessa fase.

## Project Scope 
  - Game Time Scale
    - Custo: R$5.000,00 (custo relacionado aos gastos com IA, servidores e demais custos extras)
    - Tempo de desenvolvimento: 3 a 4 meses para a demo inicial e 2 anos para o lançamento de todas as fases completas (podendo ter expanção no futuro, em caso de sucesso)
  - ***Lucro dividido igualmente entre os membros***
  - Time Principal
    - Arthur Laurentino
      - Diretor gráfico, programador.
      - Desenvolvedor indie sem remuneração.
    - Matheus Schlichting
      - Diretor de desenvolvimento e gerenciador de marketing. 
      - Desenvolvedor indie sem remuneração.
    - Vinícius Anjos
      - Diretor da equipe, programador e administrador do jogo.
      - Desenvolvedor indie sem remuneração. 


## Influences (Brief)
  - Trilogia Blade Runner
    - Filmes
    - Foi usado como referência baseado na sua ambientação e adequação no tempo e história.
  - 1984
    - Livro
    - Usado como referência principalmente devido à sua história distópica, controle massivo da população, medias, etc.
  - Trilogia Jogos Vorazes
    - Filmes
    - Por conta da estruturação inicial da competição do jogo.
  - Half Life 2 (beta)
    - Jogo
    - Por conta da inspiração gráfica e ambientação da arena.
  - Blame!
    - Livro
    - Por conta do design arquitetônico das estruturas apresentados no livro.
  - Amnesia: The Dark Descent
    - Jogo
    - Inspirou principalmente as mecãnicas de terror (como perseguição, terror psicológico, etc).
  - Call of Duty: Black Ops 1
    - Jogo
    - Por conta dos experimentos com manipulação química da mente e lavagem cerebral que acontecem durante a ambientação do jogo.
  - Distrito 9 e Elysium
    - Filmes
    - Inspirou o inimigo central do jogo, além da premissa distópica do jogo.

## The elevator Pitch
Em um mundo onde tudo o que importa é a ordem e o consumo, um espetáculo mortal desfigura as bases da decência humana ao televisionar para o mundo todo ano uma batalha horrível entre um homem e uma máquina.

## Project Description (Brief):

Day of Cleansing é um jogo indie de terror psicológico em primeira pessoa ambientado em um Neo-Brasil distópico. O jogador participa de O Expurgo, um espetáculo mortal televisionado para bilhões de pessoas, onde prisioneiros são colocados em uma arena para sobreviver à perseguição de um robô humanoide chamado Chappie.

Controlando Dr. Salles, o jogador deve explorar a arena, fugir de Chappie e encontrar uma maneira de escapar antes que ele consiga eliminar o jogador.

## Project Description (Detailed)

Em um Neo-Brasil distópico, onde a população enfrenta a escassez de recursos enquanto o governo e as corporações mantêm o controle através da ordem e do consumo, milhões acompanham O Expurgo, um espetáculo mortal televisionado anualmente. Prisioneiros são enviados para uma arena e obrigados a sobreviver enquanto são perseguidos por Chappie, uma máquina criada para ser a estrela do evento.

O jogador assume o Dr. Salles, um homem de 56 anos capturado após desertar do projeto Arena Chappie, sem saber que foi um de seus idealizadores. Após ser submetido a sessões de hipnose e alucinações induzidas por drogas, ele perdeu completamente suas memórias e acorda sabendo apenas que foi condenado à morte.

Após as portas do elevador se abrirem, o Dr. entra na arena e precisa encontrar uma forma de escapar sem armas para enfrentar Chappie. O jogador deve explorar o ambiente e fugir do robô enquanto procura os recursos necessários para ativar um gerador: 2 galões de gasolina e um fusível.

Com o gerador funcionando, o portão de saída pode finalmente ser aberto. Porém, quando o portao abre o que se revela não é uma saída, e sim o fim da linha, uma arma pronta para eliminar qualquer um espera aqueles que consiguiram "vencer" Chappie.

# What sets this project apart?
  - Otimização boa e planejada
  - Originalidade na premissa
  - Gráficos, programação e modelos avançados para o tempo que tivemos (3 meses)
  - Desenvolvedores com experiencia passada

## Core Gameplay Mechanics (Detailed)
  - Sistema avançado e dinâmico de procura da IA (Sistema de patrulha, investigação, caça e roteamento)
  - Controle dinâmico do jogador (Velocidade variável entre a rotação da camera e do personagem; Aceleração com stamina; Pulo e agachamento; Camera animada)
  - Sistema de som universal (Toda ação do jogador gera um nível de som, como correr, andar, pular ou interagir com itens pelo mapa, e dependendo da distância o Chappie pode ouvir e ir investigar)
  - Comunicação entre computador (o jogo) e o celular (O usuário do celular pode mandar um sinal no jogo, que dispara uma sirene e atrai o Chappie para algum lugar do mapa)
  - Sistema de coletar itens (Uso da gasolina e do fusível) + (pegar itens do chão e usar como distração)

# Story and Gameplay

## Story (Brief)

Em um futuro cyberpunk, um espetáculo transmitido anualmente chamado *O Expurgo* foi criado em conjunto pelo Estado e pelas grandes corporações, ele serve tanto para o Estado eliminar seus inimigos quanto para as corporações continuarem seu domínio sobre a população.
O idealizador do projeto é Dr. Salles, um brilhante cientista que projeta como o espetáculo funcionará. Quando Salles descobre as verdadeiras intenções que motivaram sua criação, ele tenta desertar mas é capturado em 2192 e se torna uma vítima de sua própria criação. 

## Story (Detailed)

### Contexto

A história de Day of Cleansing acontece em 2192, em um futuro distópico onde o Brasil se transformou no Neo-Brasil, um Superestado controlado por um governo autoritário em parceria com grandes corporações. Em meio à escassez de recursos e à desigualdade social, a população é mantida sob controle através da propaganda, da vigilância, da tecnologia e do consumo.

Um dos maiores símbolos desse sistema é O Expurgo, um espetáculo televisionado realizado anualmente. Oficialmente, ele é apresentado como entretenimento e justiça. Na realidade, serve para eliminar inimigos e indivíduos indesejados pelo Estado enquanto as corporações lucram com sua transmissão e audiência.

Prisioneiros são colocados dentro de uma arena e obrigados a sobreviver à perseguição de Chappie, um robô humanoide criado especificamente para caçá-los. Para o público, existe a possibilidade de vitória. Para os participantes, porém, a arena é uma prisão cuidadosamente planejada.

### Dr. Salles e a criação da Arena

O Dr. Salles era um brilhante cientista e um dos idealizadores do projeto Arena Chappie. Inicialmente, ele acreditava estar trabalhando em uma estrutura avançada de simulação e inteligência artificial. Chappie seria uma máquina capaz de navegar autonomamente, investigar sons, localizar pessoas e adaptar seu comportamento ao ambiente.

Com o avanço do projeto, Salles descobre sua verdadeira finalidade: a arena seria utilizada como palco para execuções televisionadas. Pessoas seriam colocadas em seu interior para serem perseguidas e mortas por Chappie em nome do entretenimento.

Ao perceber que ajudou, mesmo sem saber inicialmente, a criar um sistema de assassinato, Salles tenta desertar e impedir o projeto. Entretanto, ele é capturado em 2192.

Como conhece demais sobre o funcionamento da Arena Chappie, ele é submetido a sessões de hipnose e alucinações induzidas por drogas. Suas memórias são fragmentadas para que ele não se lembre completamente de sua participação na criação do projeto.

Sua punição é se tornar uma vítima de sua própria criação.

### Os eventos do jogo

O jogo começa com Dr. Salles acordando dentro de um elevador, sem se lembrar completamente de quem é ou por que foi condenado à morte. Quando as portas se abrem, ele entra na arena, uma enorme simulação de um bairro destruído e abandonado cercado pela arquitetura e pelas luzes de uma gigantesca cidade cyberpunk.

Seu objetivo inicial é simples: escapar.

Para abrir o portão de saída, Salles precisa ativar um gerador. Para isso, ele deve encontrar dois galões de gasolina e um fusível, espalhados pelo posto de gasolina e o canteiro de obras.

Enquanto procura pelos itens, ele é perseguido por Chappie. Sem armas capazes de enfrentá-lo, o jogador precisa explorar o ambiente, evitar ser visto e controlar os sons que produz.

Após encontrar todos os itens, Salles ativa o gerador e consegue abrir o portão.

Porém, a suposta saída revela a verdade: não existe uma saída real.

Do outro lado está o fim da linha, uma metralhadora preparada para executar qualquer participante que consiga sobreviver a Chappie.

## Gameplay (Brief)

O jogador explora uma arena no meio de uma cidade cyberpunk em primeira pessoa enquanto foge de Chappie. O objetivo principal é encontrar 2 galões de gasolina e um fusível para ativar o gerador e abrir o portão de saída.

## Gameplay (Detailed)

### Visão geral

Day of Cleansing é um jogo de terror psicológico e sobrevivência em primeira pessoa focado em exploração, furtividade e perseguição.

O jogador não possui armas para enfrentar Chappie. A sobrevivência depende da capacidade de explorar o ambiente, controlar os sons produzidos, evitar ser detectado e escapar quando necessário.

O objetivo principal é encontrar os recursos necessários para ativar o gerador e abrir o portão de saída.

O fluxo principal do jogo é:

1. Explorar a arena;
1. Encontrar os dois galões de gasolina;
1. Encontrar o fusível;
1. Evitar ou escapar de Chappie;
1. Levar os recursos ao gerador;
1. Ativar o gerador;
1. Abrir o portão;
1. Descobrir que a saída era parte da execução.

### Exploração

A arena é formada por um bairro abandonado construído artificialmente dentro de uma grande estrutura. O mapa possui ruas, becos, áreas abertas e prédios, permitindo diferentes rotas de movimentação.

Entre as áreas principais estão:

- Posto de gasolina;
- Canteiro de obras;
- Praça com ônibus e carros;
- Ruas e becos;
- Prédios abandonados;
- Área do gerador;
- Portão de saída.

O jogador precisa explorar essas áreas para encontrar os objetivos, enquanto escolhe entre caminhos mais rápidos e expostos ou rotas mais seguras.

### Movimento do jogador

O jogador controla Dr. Salles em primeira pessoa e possui diferentes formas de movimentação:

- Andar;
- Trote para os lados;
- Corrida para frente;
- Pular;
- Agachar.

O sistema utiliza aceleração e desaceleração para tornar os movimentos mais naturais. A câmera também possui efeitos de movimento para aumentar a sensação de imersão.

Cada tipo de movimento possui consequências. Correr permite escapar mais rapidamente, mas produz mais som e pode revelar a posição do jogador.

### Sistema universal de som

O som é uma das principais mecânicas do jogo.

As ações do jogador produzem ruídos que podem ser detectados por Chappie. A intensidade do som determina a distância aproximada em que o robô consegue ouvi-lo.

Ações que produzem som incluem:

- Andar;
- Correr;
- Pular e aterrissar;
- Utilizar objetos como distração.

As superfícies também possuem sons diferentes, como terra e concreto.

Isso obriga o jogador a pensar antes de agir. Em algumas situações, correr pode ser necessário para escapar. Em outras, permanecer silencioso pode ser a melhor escolha.

O sistema também pode ser usado a favor do jogador: objetos e eventos sonoros podem atrair Chappie para outra área da arena.

### Inteligência Artificial de Chappie

A IA de Chappie funciona através de diferentes estados de comportamento:

#### Patrulha

Quando não possui informações sobre o jogador, Chappie percorre diferentes áreas do mapa.

#### Investigação

Quando escuta um som, ele se desloca até sua origem para investigar. O jogador pode utilizar isso para criar distrações e mudar a rota do robô.

#### Visão

Caso Chappie veja o jogador, a visão possui prioridade e inicia uma perseguição.

#### Perseguição

Após detectar visualmente o jogador, Chappie aumenta sua velocidade e tenta alcançá-lo. O jogador precisa fugir, utilizar obstáculos e quebrar a linha de visão.

#### Procura

Quando perde o jogador, Chappie procura na região onde o viu ou ouviu pela última vez. Caso não encontre nenhuma nova informação, eventualmente retorna à patrulha.

Esse sistema torna o comportamento da IA dinâmico. Chappie não sabe constantemente onde o jogador está, precisando encontrá-lo através de visão e som.

### Coleta e uso de itens

O jogador precisa encontrar:

 - **2 galões de gasolina;**
 - **1 fusível.**

Os itens estão espalhados pela arena e devem ser coletados através de interação.

Depois de reunir os recursos, o jogador deve levá-los até o gerador para ativá-lo.

Além dos itens principais, determinados objetos do cenário podem ser utilizados como distrações. Produzir um som em outro local pode fazer com que Chappie abandone temporariamente uma área e vá investigar.

### Comunicação entre computador e celular

Uma mecânica diferenciada do projeto é a comunicação entre o jogo no computador e um celular.

O dispositivo pode enviar um sinal para o jogo, ativando uma sirene em determinado ponto da arena.

O som produzido pela sirene é detectado pelo sistema de audição de Chappie, fazendo com que ele vá investigar sua origem.

Isso permite utilizar um segundo dispositivo como uma ferramenta estratégica para alterar temporariamente a movimentação do inimigo.

### Áudio e atmosfera

O áudio possui importância tanto para a ambientação quanto para o gameplay.

A arena possui sons como:

Vento;
Ruídos da cidade cyberpunk;
Propagandas transmitidas por megafones;
Sons de metal;
Música ambiente;
Drones;
Máquinas e estruturas funcionando.

Chappie possui uma identidade sonora própria. Seus passos metálicos e ruídos mecânicos ajudam o jogador a perceber que ele está próximo, mesmo sem conseguir vê-lo.

Durante perseguições, os sons se tornam mais intensos, aumentando a sensação de perigo.

### Condição de derrota

Se Chappie conseguir alcançar o jogador, uma animação de ataque é executada e a partida termina.

O jogador é enviado para a tela de morte, onde pode escolher entre tentar novamente ou retornar ao menu principal.

A experiência foi planejada para manter o jogador constantemente vulnerável. Não é possível derrotar Chappie: é necessário sobreviver, fugir, se esconder e utilizar o ambiente de forma inteligente.

O principal objetivo do gameplay é fazer com que o jogador nunca se sinta completamente seguro dentro da arena, transformando cada som, esquina e perseguição em uma possível ameaça.

# Assets Needed

## 2D
- Bandeira do Neo-Brasil (Superestado)
- Imagem do líder supremo do Neo-Brasil

## 3D
  - Characters List
    - Chappie (Robô)
    
  - Environmental Art Lists
    - Galão de Gasolina (2 espalhados pelo mapa)
    - Fusível
    - Prédios
    - Ruas
    - Iluminação
    - Placas de LED/NEON
    - Megafones
    - Textura da cidade cyberpunk ao redor
    - Posto de gasolina
    - Canteiro de obras
    - Praça (Onibus e carro)
    - Gerador
    - Portão
    - Elevador
    - Lixeira cheia

## Sound
  - Sound List (Ambiente)
    - Músicas de fundo
    - Efeitos sonoros (som de vento, som da cidade ao redor, sons do megafone, e som de metal)
  - Sound List (Player)
    - Interação com itens (gasolina, fusíveis)
    - Som de passos do player (na terra e concreto)
    - Som de pulo e queda
  - Sound List (Chappie)
    - Som de passos (com metal arranhando)
    - Som de metal de robô (como uma voz metálica)
    - Grito de perseguição
      
      
## Code
  - Player Scripts
    - `CameraController.cs` *Script de controle da câmera do jogador com mouse.*
    - `CameraEffects.cs` *Script que gera o efeito de balanço na corrida em primeira pessoa.*
    - `MusicManager.cs` *Script que controla as músicas tocadas no jogo.*
    - `PlayerFootsteps.cs` *Script que controla o som emitido pelo jogador ao se movimentar.*
    - `PlayerMovement.cs` *Script que controla o movimento do jogador pelo mapa usando teclado.*
    - `PlayerNoise.cs` *Script que controla o barulho virtual emitido pelo jogador para detecção pelo robô.*
  - Enemy Scripts (Chappie)
    - `EnemyAI.cs` *Script que gerencia todo o controle da IA do robô (perseguição, patrulha, etc).*
    - `EnemyAudio.cs` *Script que faz a emissão do som de passos do robô.*
    - `EnemyHearing.cs` *Script que controla a audição do robô*
    - `EnemyVision.cs` *Script que controla a visão do robô.*
  - Scene Scripts
    - `Restart.cs` *Script que faz o jogador retornar da tela de morte ao jogo.*
    - `SceneChange.cs` *Script que faz o jogador sair do jogo e ir para a tela de morte quando o robô o alcança.*

## Animation
  - Environment Animations 
    - Telões
    - Drone (pontos brilhantes no céu)
    - Elevador abrindo
    - Gerador ativando
  - Character Animations 
    - Chappie
      - Andando
      - Correndo
      - Parado
      - Atacando

# Schedule
  - Primeiro mês:
    - 01/09/2026
      - Protótipo da IA do chappie completo.
      - Planejamento e implementação de todos os sons do jogo.
      - Desenvolvimento do menu principal.
      - Menu e cutscene de morte.
      - Início do desenvolvimento do mapa principal.
  - Segundo mês:
    - 01/10/2026
      - Cutscene de início do jogo.
      - Protótipo inicial do mapa principal.
      - Mecânicas principais implementadas (coletar itens, ativas gerador, etc.)
      - Jogabilidade básica pronta para testes BETA.
  - Terceiro mês:
    - 01/11/2026
      - Jogo pronto.
      - Testes finais.
