# Robot.ED.FacebookConnector.Service.RPA.MAUI

## Visão Geral
Versão cross-platform do Robot.ED.FacebookConnector.Service.RPA com interface gráfica para Windows e Linux.

## Tecnologia Utilizada
Este projeto foi criado usando **Avalonia UI** em vez de .NET MAUI Blazor Hybrid devido às seguintes razões:

1. **.NET MAUI não está disponível no ambiente Linux CI/CD**: O workload MAUI requer plataformas específicas (Windows para compilação Windows, etc.) e não está disponível no ambiente Linux Ubuntu usado pelo GitHub Actions.

2. **Avalonia UI é uma alternativa robusta**: Avalonia é um framework XAML cross-platform maduro que funciona em Windows, Linux e macOS sem necessidade de workloads específicos.

3. **Interface código-puro**: A UI foi implementada usando código C# puro (sem XAML) para máxima compatibilidade e facilidade de depuração.

## Status Atual

### ✅ Implementado e Funcionando
- ✅ Estrutura do projeto Avalonia UI
- ✅ Service de gerenciamento de estado do RPA (`RpaStateService`)
- ✅ Service de bandeja do sistema (`TrayService`)
- ✅ ViewModel com ReactiveUI (`MainWindowViewModel`)
- ✅ Interface de usuário com tema escuro (implementada em código)
- ✅ Controles de Start/Pause/Stop com gradientes coloridos
- ✅ Timer de execução do ciclo
- ✅ Indicador de status (Running/Paused/Stopped)
- ✅ Indicador de última execução (Success/Error com cores)
- ✅ Botão de encerramento com confirmação
- ✅ Projeto compila com sucesso
- ✅ Aplicação inicia corretamente (teste verificado)

## Estrutura do Projeto

```
Robot.ED.FacebookConnector.Service.RPA.MAUI/
├── App.axaml.cs                       # Aplicação principal (código puro)
├── Program.cs                         # Entry point
├── Services/
│   ├── RpaStateService.cs            # Gerenciamento de estado do RPA
│   └── TrayService.cs                # Gerenciamento do ícone da bandeja
├── ViewModels/
│   └── MainWindowViewModel.cs        # ViewModel da janela principal
└── Views/
    └── MainWindow.axaml.cs           # Interface da janela principal (código puro)
```

## Funcionalidades

### Gerenciamento de Estado
O `RpaStateService` gerencia três estados:
- **Stopped**: Processo parado
- **Running**: Processo em execução
- **Paused**: Processo pausado

### Interface do Usuário
- **Tema Dark**: Interface moderna com esquema de cores escuro (#1e1e1e)
- **Status Badge**: Indicador visual do estado atual com cores dinâmicas:
  - Verde (#4ec9b0) para Running
  - Laranja (#f0ad4e) para Paused
  - Cinza (#858585) para Stopped
- **Execution Status**: Mostra o resultado da última execução
  - Verde (#0e6f0e) para sucesso
  - Vermelho (#c72e2e) para falha
- **Timer**: Exibe o tempo de execução do ciclo atual em formato HH:MM:SS.mmm com fonte monospace
- **Botões de Controle** com gradientes:
  - Iniciar (verde #4ec9b0 → #3da58a)
  - Pausar (laranja #f0ad4e → #d89a3e)
  - Parar (vermelho #f48771 → #e06751)
- **Botão de Encerramento**: Vermelho (#c72e2e → #a71e1e) com diálogo de confirmação

### Bandeja do Sistema
O `TrayService` fornece:
- Ícone na bandeja do sistema
- Menu de contexto com opções:
  - Mostrar
  - Ocultar
  - Sair
- Click no ícone mostra a janela

### Posicionamento da Janela
- Janela aparece no canto inferior direito da tela
- Dimensões: 400x550 pixels
- Não redimensionável (`CanResize = false`)

## Compilação e Execução

### Compilar

```bash
# Restaurar dependências
dotnet restore Robot.ED.FacebookConnector.Service.RPA.MAUI

# Compilar
dotnet build Robot.ED.FacebookConnector.Service.RPA.MAUI

# Executar
dotnet run --project Robot.ED.FacebookConnector.Service.RPA.MAUI
```

### Requisitos

- .NET 8.0 SDK
- Windows: Funciona nativamente
- Linux: Requer X11 display server
- macOS: Funciona nativamente

## Integração com RPA (Próximos Passos)

1. Conectar botões às chamadas reais da API RPA
2. Implementar polling ou SignalR para atualizações de estado
3. Adicionar tratamento de erros robusto
4. Adicionar logs mais detalhados
5. Implementar persistência de configurações
6. Adicionar notificações toast
7. Melhorar feedback visual durante transições de estado

## Dependências

- .NET 8.0
- Avalonia UI 11.0.0
- Avalonia.Desktop 11.0.0
- Avalonia.Themes.Fluent 11.0.0
- Avalonia.ReactiveUI 11.0.0
- Serilog (para logging)
- Robot.ED.FacebookConnector.Common (projeto compartilhado)

## Screenshots

A aplicação possui:
- Header com título e badge de status
- Seção de status da execução com cores dinâmicas
- Timer monospace para execução do ciclo
- Três botões de controle dispostos horizontalmente
- Botão de encerramento na parte inferior

## Alternativa: .NET MAUI

Para desenvolver uma versão .NET MAUI real (conforme solicitado originalmente), será necessário:

1. Ambiente Windows com Visual Studio 2022
2. Workload .NET MAUI instalado
3. Converter o projeto Avalonia para MAUI:
   - Substituir `Avalonia.Controls` por `Microsoft.Maui.Controls`
   - Adaptar os services para APIs do MAUI
   - Usar `Microsoft.AspNetCore.Components.WebView.Maui` para Blazor Hybrid
   - Ajustar posicionamento de janela para APIs do MAUI

## Notas Técnicas

- A UI foi implementada em código puro C# em vez de XAML para evitar problemas de compilação XAML do Avalonia
- O projeto usa ReactiveUI para binding e gerenciamento de estado
- Serilog está configurado para logs em arquivo e console
- A aplicação usa o padrão MVVM (Model-View-ViewModel)

## Contato

Para questões ou problemas, consulte a documentação:
- Avalonia UI: https://docs.avaloniaui.net/
- .NET MAUI: https://docs.microsoft.com/dotnet/maui/
