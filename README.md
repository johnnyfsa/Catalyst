# Catalyst

> Um card game de química construído em Unity, no qual reações químicas se tornam sistemas jogáveis, recursos estratégicos e problemas de engenharia.

**Catalyst** é um jogo de cartas educativo e estratégico em desenvolvimento, criado para explorar como conceitos científicos podem ser transformados em mecânicas de jogo claras, consistentes e extensíveis.

Mais do que um projeto de game design, Catalyst também funciona como uma plataforma de demonstração das minhas capacidades em:

* arquitetura de sistemas de gameplay;
* desenvolvimento em Unity e C#;
* modelagem de domínio;
* design orientado a dados;
* testes automatizados;
* prototipação de mecânicas;
* integração entre regras científicas e experiência do jogador;
* planejamento incremental de produto.

---

## Visão do projeto

Em Catalyst, o jogador manipula elementos, moléculas, misturas e recursos para executar reações químicas e cumprir objetivos dentro de um número limitado de turnos.

As cartas não representam apenas itens colecionáveis. Elas são entidades de um sistema químico.

O jogador pode:

* comprar e organizar cartas;
* posicionar reagentes em uma mesa de reações;
* executar transformações estequiometricamente válidas;
* produzir novas substâncias;
* consumir ou gerar recursos como calor e eletricidade;
* administrar resíduos e poluentes;
* cumprir objetivos produtivos antes que os recursos ou turnos se esgotem.

A proposta é transformar química em uma linguagem sistêmica compreensível, sem abandonar consistência científica ou profundidade estratégica.

---

## Primeira fase: A Grande Seca

A primeira fase do jogo é centrada em uma crise hídrica.

O objetivo do jogador é produzir água utilizando os recursos químicos disponíveis, escolhendo entre diferentes cadeias produtivas e avaliando seus custos.

Entre os processos planejados ou implementados estão:

```text
2 H₂ + O₂ → 2 H₂O

CH₄ + 2 O₂ → CO₂ + 2 H₂O

2 H₂O + eletricidade → 2 H₂ + O₂

NaCl(aq) + calor → NaCl + H₂O
```

Essas reações permitem que o jogador explore diferentes rotas para alcançar um mesmo objetivo.

Algumas podem demandar mais energia. Outras podem gerar calor. Outras produzem poluentes ou subprodutos que precisam ser administrados.

A química deixa de ser apenas conteúdo apresentado ao jogador e passa a ser a estrutura central do gameplay.

---

## Objetivos de design

Catalyst foi concebido a partir de alguns princípios.

### Ciência como sistema

As reações devem ser coerentes com as proporções químicas representadas pelas cartas.

A estequiometria não aparece apenas como texto explicativo. Ela determina quantas cartas são consumidas, quantos produtos são criados e quais recursos participam da transformação.

### Escolhas, não respostas únicas

Sempre que possível, um objetivo pode ser alcançado por diferentes cadeias produtivas.

Produzir água por combustão de hidrogênio é diferente de produzi-la pela combustão de metano ou pela separação de uma solução salina.

Cada rota pode envolver custos, riscos, recursos e consequências diferentes.

### Complexidade progressiva

A arquitetura foi planejada para suportar fases futuras com novos sistemas químicos e industriais.

Exemplos:

* pilhas eletroquímicas;
* armazenamento e tratamento de poluentes;
* sistemas químicos adjacentes;
* geradores movidos a vapor;
* produção e consumo de eletricidade;
* dispositivos que abstraem processos químicos;
* cadeias produtivas com múltiplos estágios.

### Clareza de interação

Mesmo com regras científicas, o jogador deve compreender:

* quais cartas podem ser combinadas;
* por que uma reação foi ou não aceita;
* o que será consumido;
* o que será produzido;
* quais recursos serão gastos ou gerados;
* quais consequências a decisão terá.

---

## Arquitetura técnica

O projeto utiliza uma arquitetura orientada a domínio, separando definições estáticas, estado de runtime, serviços de gameplay e apresentação visual.

```text
Definitions
├── CardDefinition
├── ReactionDefinition
├── ReactionCardAmount
└── DeckEntry

Runtime
├── CardInstance
├── DeckRuntime
├── HandRuntime
├── ReactionTableRuntime
├── DiscardPileRuntime
├── ResourceCounterRuntime
└── GameSession

Services
├── CardMovementService
├── CardDrawService
├── DrawPhaseService
├── MainPhaseService
├── ManualDiscardService
├── EndPhaseService
├── GameSessionFlowService
├── ReactionMatcherService
├── ReactionResolutionPlanner
├── ReactionExecutionValidator
└── ReactionExecutionService
```

Essa divisão evita que regras importantes fiquem presas a componentes visuais da Unity.

O objetivo é manter o núcleo do jogo:

* independente da interface;
* testável;
* previsível;
* reutilizável;
* preparado para expansão.

---

## Definições e estado de runtime

As informações permanentes das cartas são armazenadas em definições orientadas a dados.

Uma definição pode indicar:

* identidade química;
* nome;
* fórmula;
* categoria;
* aparência;
* metadados de gameplay.

Durante uma partida, cada carta é representada por uma instância própria, com identidade imutável.

Essa separação permite que várias cartas compartilhem a mesma definição sem deixarem de ser objetos individuais dentro da sessão.

```text
CardDefinition
    ↓
CardInstance
    ↓
Deck / Hand / Reaction Table / Discard
```

A mesma abordagem é utilizada para separar a definição de uma reação do plano concreto de execução daquela reação durante uma partida.

---

## Motor de reações

O motor de reações foi dividido em etapas explícitas.

### Correspondência

O sistema compara as cartas posicionadas na mesa com uma definição de reação.

```text
Cartas na mesa
      ↓
ReactionMatcherService
      ↓
Reação correspondente ou falha explícita
```

### Planejamento

Quando uma reação é reconhecida, o jogo cria um plano de resolução.

Esse plano descreve:

* reagentes que serão consumidos;
* produtos que serão criados;
* calor consumido ou produzido;
* eletricidade consumida ou produzida;
* demais alterações previstas para a sessão.

### Validação

Antes de alterar o estado do jogo, o plano completo é validado.

Isso evita execuções parciais, como consumir reagentes sem conseguir criar os produtos correspondentes.

### Execução transacional

Somente depois da validação o estado da partida é alterado.

```text
Match
  ↓
Plan
  ↓
Validate
  ↓
Execute
```

Essa estrutura torna a execução previsível e facilita a criação de testes para cada etapa do processo.

---

## Fluxo de turnos

O fluxo principal da partida é controlado por serviços de domínio.

```text
Draw Phase
    ↓
Main Phase
    ↓
End Phase
    ↓
Next Turn
```

Algumas regras já modeladas incluem:

* compra bloqueada quando a mão está cheia;
* obrigação de liberar espaço antes de encerrar determinadas situações;
* reorganização dos índices das cartas na mão;
* impossibilidade de uma carta ocupar duas zonas simultaneamente;
* encerramento da sessão por deck-out;
* transições de fase explícitas;
* resultados de sucesso e falha representados por tipos próprios.

Em vez de depender de exceções ou valores booleanos genéricos, as operações retornam resultados que explicam o que ocorreu.

Exemplo conceitual:

```csharp
MainPhaseEndResult result =
    flowService.TryEndMainPhase(session);

if (!result.Succeeded)
{
    HandleFailure(result.Failure);
}
```

Isso permite que a interface apresente mensagens precisas ao jogador e reduz ambiguidades dentro do código.

---

## Recursos

Catalyst possui recursos que existem independentemente das cartas.

Os primeiros recursos implementados são:

* calor;
* eletricidade.

Eles podem ser utilizados como requisitos, custos ou produtos de uma reação.

Por exemplo:

```text
2 H₂O + eletricidade → 2 H₂ + O₂
```

seguido de:

```text
2 H₂ + O₂ → 2 H₂O + calor
```

Essa relação permite construir ciclos produtivos, analisar eficiência e criar sistemas futuros envolvendo máquinas ou instalações.

---

## Zonas de jogo

O jogo é organizado em zonas com responsabilidades distintas.

### Deck

Contém as cartas ainda não compradas.

### Mão

Armazena as opções imediatamente disponíveis ao jogador.

### Mesa de reações

Recebe os reagentes selecionados e tenta identificar uma reação compatível.

### Descarte

Recebe cartas removidas permanentemente do ciclo produtivo da partida.

Cartas descartadas não retornam à mão, ao deck ou à mesa.

### Sistemas químicos adicionais

Planejados para fases futuras, permitem que cartas sejam posicionadas em estruturas persistentes, como tanques, dispositivos e sistemas eletroquímicos.

### Armazenamento de poluentes

Zona planejada para representar resíduos ainda não tratados.

O sistema diferencia:

* poluentes emitidos;
* poluentes armazenados;
* poluentes tratados.

Essa separação permitirá que o impacto ambiental se torne parte das decisões produtivas do jogador.

---

## Testes automatizados

O núcleo de gameplay é desenvolvido com testes EditMode.

As regras testadas incluem:

* criação e identificação de instâncias de cartas;
* embaralhamento determinístico;
* compra de cartas;
* capacidade máxima da mão;
* movimentação entre zonas;
* prevenção de duplicidade de localização;
* fluxo completo de turnos;
* encerramento por deck-out;
* correspondência entre reagentes e reações;
* validação de definições de reação;
* criação de planos de resolução;
* validação da execução;
* consumo e geração de recursos;
* comportamento transacional das reações.

Fontes de aleatoriedade e geração de identificadores são abstraídas por interfaces, permitindo testes reproduzíveis.

```csharp
SeededRandomSource random = new(seed: 12345);
SequentialCardInstanceIdSource ids = new();
```

O objetivo dos testes não é apenas verificar implementações isoladas, mas proteger as regras centrais do jogo enquanto o projeto cresce.

---

## Decisões de engenharia

Algumas decisões importantes do projeto incluem:

### Lógica desacoplada de GameObjects

As principais regras não dependem de `MonoBehaviour`, cenas ou prefabs.

Isso permite testar o jogo sem carregar a interface da Unity.

### Resultados explícitos

Operações retornam objetos de resultado contendo sucesso, falha e contexto.

Isso reduz estados inválidos e facilita a comunicação com a camada visual.

### Estado controlado

As cartas não alteram diretamente sua própria zona.

Movimentações são realizadas por serviços responsáveis por preservar as invariantes da sessão.

### Planejamento antes da mutação

Reações são planejadas e validadas antes que cartas ou recursos sejam modificados.

### Aleatoriedade injetável

O embaralhamento utiliza uma fonte de aleatoriedade substituível, permitindo sessões determinísticas em testes.

### Desenvolvimento incremental

O projeto é construído em ciclos pequenos:

```text
Modelar
→ Implementar
→ Testar
→ Integrar
→ Refatorar
→ Expandir
```

---

## Desafios técnicos explorados

Catalyst funciona como um laboratório para problemas comuns de desenvolvimento de jogos.

Entre eles:

* representar regras complexas sem acoplá-las à UI;
* manter consistência entre múltiplas zonas;
* trabalhar com entidades definidas por ScriptableObjects e instâncias de runtime;
* executar transformações atômicas de estado;
* criar sistemas extensíveis sem antecipar toda a complexidade futura;
* conciliar fidelidade científica com legibilidade;
* testar fluxos de jogo que normalmente dependeriam de cenas;
* transformar falhas de domínio em feedback de interface;
* projetar mecânicas capazes de crescer sem reescrever o núcleo.

---

## Roadmap

### Fundação de runtime

* [x] Instâncias de cartas
* [x] Deck de runtime
* [x] Mão do jogador
* [x] Mesa de reações
* [x] Pilha de descarte
* [x] Movimentação entre zonas
* [x] Compra de cartas
* [x] Fluxo básico de turnos
* [x] Encerramento por deck-out
* [x] Recursos de calor e eletricidade

### Motor de reações

* [x] Definições de reação
* [x] Validação de definições
* [x] Correspondência de reagentes
* [x] Planejamento de resolução
* [x] Validação de execução
* [x] Execução transacional
* [x] Criação de produtos
* [ ] Integração completa com o fluxo da sessão

### Interface e interação

* [ ] Visualização da mão
* [ ] Drag and drop
* [ ] Snap de cartas na mesa
* [ ] Feedback de reação válida
* [ ] Feedback de falha
* [ ] Animações de consumo e criação
* [ ] Contadores de recursos
* [ ] Indicadores de objetivo

### Conteúdo da primeira fase

* [ ] Deck completo
* [ ] Cadeias de produção de água
* [ ] Objetivos da Grande Seca
* [ ] Balanceamento de turnos
* [ ] Introdução a calor e eletricidade
* [ ] Poluentes básicos
* [ ] Tutorial

### Expansões planejadas

* [ ] Pilha de Daniell
* [ ] Sistemas químicos persistentes
* [ ] Tanques de reagentes
* [ ] Ponte salina
* [ ] Gerador a vapor
* [ ] Tratamento de resíduos
* [ ] Diferentes cenários produtivos
* [ ] Modo sandbox

---

## Tecnologias e práticas

* Unity
* C#
* ScriptableObjects
* Programação orientada a objetos
* Modelagem de domínio
* Arquitetura orientada a serviços
* Injeção de dependências manual
* Testes automatizados com NUnit
* Desenvolvimento orientado a dados
* Randomização determinística
* Git e versionamento incremental

---

## O que este projeto demonstra

Catalyst foi criado para demonstrar não apenas capacidade de implementação, mas capacidade de condução de produto.

O projeto envolve:

### Game design

Transformação de conceitos científicos em regras, objetivos, recursos e decisões estratégicas.

### Systems design

Criação de sistemas interdependentes envolvendo cartas, zonas, reações, recursos, turnos e condições de encerramento.

### Gameplay programming

Implementação das regras do jogo com atenção a estados inválidos, transições e integração futura com a interface.

### Arquitetura de software

Separação entre dados, domínio, serviços, runtime e apresentação.

### Qualidade

Uso de testes automatizados para proteger regras importantes e permitir refatorações seguras.

### Planejamento

Divisão do desenvolvimento em fases, identificação de dependências e controle de escopo para construção de um protótipo funcional.

### Comunicação

Documentação das decisões, justificativas de arquitetura e relação entre objetivos de design e soluções técnicas.

---

## Sobre o desenvolvimento

Catalyst é desenvolvido por **João Gabriel Moraes**, engenheiro de computação, game designer e desenvolvedor Unity.

Meu foco profissional está na interseção entre:

* game design;
* programação de gameplay;
* arquitetura de sistemas;
* prototipação;
* ferramentas e fluxos de produção;
* tradução de requisitos criativos em implementações concretas.

Este repositório registra não apenas o resultado final, mas também a evolução das decisões de design e engenharia do projeto.

---

## Status

O projeto está atualmente em desenvolvimento.

A fundação do runtime, o fluxo básico de turnos e as principais etapas do motor de reações já foram modelados e cobertos por testes automatizados.

O próximo marco é a integração entre o domínio de gameplay e a camada visual da Unity, incluindo:

* representação das cartas;
* drag and drop;
* posicionamento na mesa;
* execução visual das reações;
* atualização dos recursos e objetivos.

---

## Contato

**João Gabriel Moraes**

Game Designer · Unity Developer · Gameplay Programmer

* LinkedIn: https://linkedin.com/in/jglmoraes
* Portfólio: https://johnnymoraes.com
* E-mail: joaofeirense@gmail.com

---

## Licença

A licença do projeto ainda será definida.

Os códigos, assets e conceitos apresentados neste repositório não devem ser reutilizados comercialmente sem autorização.
