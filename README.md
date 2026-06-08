# Deadlock Detector - Resource Allocation Graph

## Descrição

Simulador visual de detecção de deadlocks usando grafo de alocação de recursos. Demonstra como deadlocks ocorrem em sistemas operacionais quando processos competem por recursos compartilhados.


## Estrutura do Projeto

```
DeadlockDetector/
├── Domain/                  # Lógica do grafo
├── Simulators/              # Simulador de processos
├── UI/                      # Interface gráfica
├── Program.cs              # Ponto de entrada
├── DeadlockDetector.csproj # Configuração do projeto
├── run.sh                  # Script para Linux/macOS
├── run.bat                 # Script para Windows
├── Dockerfile              # Container Docker
└── README.md              # Este arquivo
```


## Demonstração

O programa mostra em tempo real:
- Processos (círculos azuis) e Recursos (quadrados verdes)
- Solicitações de recursos (setas vermelhas)
- Alocações de recursos (setas verdes)
- Deadlocks quando detectados (nós vermelhos)

## Como Executar

### Instalação Manual

#### Instalar .NET 6.0

**Ubuntu/Debian:**
```bash
sudo apt update
sudo apt install -y dotnet6
```

**Fedora:**
```bash
sudo dnf install dotnet-sdk-6.0
```

**Windows:**
Baixar e instalar de: https://dotnet.microsoft.com/download

**macOS:**
```bash
brew install dotnet-sdk
```

#### Instalar GTK (para visualização)

**Ubuntu/Debian:**
```bash
sudo apt install -y libgtk-3-0 libcairo2
```

**Fedora:**
```bash
sudo dnf install gtk3 cairo
```

**Windows:**
O GTK será instalado automaticamente pelo NuGet

**macOS:**
```bash
brew install gtk+3 cairo
```

#### Executar o Programa

```bash
# Entrar no diretório
cd DeadlockDetector

# Restaurar pacotes
dotnet restore

# Compilar
dotnet build

# Executar
dotnet run --project DeadlockDetector
```

## Como Usar o Programa

1. **Selecionar Velocidade**:
   - Muito Lento: Bom para observar cada operação
   - Normal: Equilíbrio entre velocidade e observação
   - Muito Rápido: Deadlocks ocorrem rapidamente

2. **Selecionar Chance de Deadlock**:
   - Baixa (15%): Raros deadlocks
   - Média (40%): Deadlocks ocasionais
   - Alta (70%): Deadlocks frequentes
   - Muito Alta (90%): Deadlocks muito comuns

3. **Clicar em "Iniciar"** para começar a simulação

4. **Observar**:
   - O grafo sendo desenhado em tempo real
   - Logs das operações
   - Deadlocks quando ocorrem

5. **Clicar em "Parar"** ou aguardar o tempo da simulação

## Exemplo de Deadlock

Quando ocorre um deadlock, você verá no log:

```
[HH:MM:SS.fff] P1 adquiriu Impressora (modo conflito)
[HH:MM:SS.fff] P2 adquiriu Scanner (modo conflito)
[HH:MM:SS.fff] P1 tentando adquirir Scanner
[HH:MM:SS.fff] P2 tentando adquirir Impressora
[HH:MM:SS.fff] DEADLOCK DETECTADO! Processos: P1, P2 (Total: 1)
[HH:MM:SS.fff]   -> Deadlock entre P1 e P2 (Impressora <-> Scanner)
```

## Solução de Problemas

### Erro: "Não foi possível localizar um projeto"
```bash
# Use o comando completo:
dotnet run --project DeadlockDetector
```

### Erro: GTK não encontrado
```bash
# Linux
sudo apt install libgtk-3-0 libcairo2

# macOS
brew install gtk+3 cairo
```