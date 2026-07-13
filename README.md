# Deadlock Detector - Resource Allocation Graph

## Descrição

Projeto responsável por simular processos requisitando e obtendo recursos compartilhados, e a partir disso detectar a ocorrência de deadlocks ocorrendo nestas interações. A simulação e detecção é ilustrada a partir de uma interface gráfica, que é responsável por as interações, como a requisição, alocação e deadlock.

## Estrutura do Projeto

```
DeadlockDetector/
├── Domain/                   # Lógica do grafo
├── Simulators/               # Simulador de processos
├── UI/                       # Interface gráfica
├── Program.cs                # Ponto de entrada
├── DeadlockDetector.csproj   # Configuração do projeto
└── README.md                 # Este arquivo
```

## Tecnologias utilizadas

Este software utilizou:

- **IDE:** Visual Studio Code
- **Linguagem:** C#
- **Plataforma de execução:** .NET 8
- **Framework de interface gráfica:** GTK# (GtkSharp 3.24)
- **Sistema de renderização gráfica:** Cairo (utilizado pelo GTK para desenho dos elementos do grafo)

## Ilustração dos processos

O programa mostra passo a passo:
- Processos (círculos azuis) e Recursos (quadrados verdes)
- Solicitações de recursos (setas laranjas)
- Alocações de recursos (setas azuis)
- Deadlocks quando detectados (processos envolvidos ficam vermelhos)

## Implementação da lógica

A simulação cria múltiplos processos concorrentes que requisitam e utilizam recursos compartilhados, registrando cada requisição, alocação e liberação em um grafo de alocação de recursos. Durante a execução, os processos seguem um comportamento aleatório seguindo uma probabilidade (que pode ser configurada) de formação de deadlocks, enquanto o sistema monitora continuamente a simulação para identificar ciclos no grafo de espera entre processos e recursos, que é exatamente o que caracteriza um deadlock.

## Referência utilizada

A referência principal foi um trecho da nona edição do livro "Operating System Concepts", dos autores Abraham Silberschatz, Greg Gagne, and Peter Baer Galvin. Capítulo 7, item 7.6 - Deadlock Detection.

No terceiro parágrafo do item 7.6.1 - Single Instance of Each Resource Type, se encontra o principal trecho utilizado nesta aplicação:

> As before, a deadlock exists in the system if and only if the wait-for graph contains a cycle. To detect deadlocks, the system needs to maintain the wait-for graph and periodically invoke an algorithm that searches for a cycle in the graph. An algorithm to detect a cycle in a graph requires an order of n2 operations, where n is the number of vertices in the graph.

Em tradução livre:

> Portanto, um deadlock existe no sistema se, e somente se, o grafo de espera contém um ciclo. Para detectar deadlocks, o sistema deve manter um grafo de espera e periodicamente invocar um algoritmo que busque por ciclos no grafo. Um algoritmo de detecção de ciclos em grafos possui ordem de no mínimo n2 operações, onde n é o número de vértices no grafo.

## Como Executar

### Instalação Manual

#### 1. Instalar .NET 8.0

Ubuntu/Debian:
```bash
sudo apt update
sudo apt install -y dotnet8
```

Fedora:
```bash
sudo dnf install dotnet-sdk-8.0
```

Windows:
Baixar e instalar de: https://dotnet.microsoft.com/download

macOS:
```bash
brew install dotnet-sdk
```

#### 2. Instalar GTK (para visualização)

Ubuntu/Debian:
```bash
sudo apt install -y libgtk-3-0 libcairo2
```

Fedora:
```bash
sudo dnf install gtk3 cairo
```

Windows:
O GTK será instalado automaticamente pelo NuGet

macOS:
```bash
brew install gtk+3 cairo
```

#### 3. Executar o Programa

Já dentro do diretório raíz do projeto, executar os seguintes comandos:

```bash
# Restaurar pacotes
dotnet restore

# Compilar para bin/Debug/net8.0/DeadlockDetector
dotnet build

# Executar arquivo gerado, ou utilizar o comando
dotnet run
```

## Como Usar o Programa

1. **Selecionar Chance de Deadlock**:
   - Baixa (15%): Raros deadlocks
   - Média (40%): Deadlocks ocasionais
   - Alta (70%): Deadlocks frequentes
   - Muito Alta (90%): Deadlocks muito comuns

2. **Clicar em "Iniciar"** 

Na barra superior, clicar em "Iniciar" para inicializar a simulação.

3. **Avançar passo a passo**:

Clicar sucessivas vezes em "Próximo", para avançar os passos da simulação e ver:
   - O grafo sendo desenhado com o avanço da simulação
   - Logs das operações
   - Deadlocks quando ocorrem

4. **Clicar em "Reset" para recomeçar** 

Caso queira começar um nova simulação, use o botão de Reset na barra superior, e volte do passo 2.

## Exemplo de Deadlock

Quando ocorre um deadlock, você verá no log:

```
[HH:MM:SS.fff] P1 adquiriu Impressora (modo conflito)
[HH:MM:SS.fff] P2 adquiriu Scanner (modo conflito)
[HH:MM:SS.fff] P1 tentando adquirir Scanner
[HH:MM:SS.fff] P2 tentando adquirir Impressora
[HH:MM:SS.fff] DEADLOCK DETECTADO! Processos: P1, P2 (Total: 2)
[HH:MM:SS.fff]   -> Deadlock entre P1 e P2 (Impressora <-> Scanner)
```

## Solução de Problemas

### Erro: GTK não encontrado
```bash
# Linux
sudo apt install libgtk-3-0 libcairo2

# macOS
brew install gtk+3 cairo
```