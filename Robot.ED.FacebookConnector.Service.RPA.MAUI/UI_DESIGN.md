# Robot.ED.FacebookConnector.Service.RPA.MAUI - Interface Visual

## Descrição da Interface

A aplicação apresenta uma janela moderna com tema escuro (400x550 pixels) posicionada no canto inferior direito da tela.

## Layout da Janela

```
┌─────────────────────────────────────────────────┐
│ HEADER (#252526)                                 │
│ ┌─────────────────────────────────────────────┐ │
│ │ RPA Facebook Connector (20px, SemiBold)     │ │
│ │                                             │ │
│ │ ┌─────────────────────────────────────────┐ │ │
│ │ │  [EM EXECUÇÃO] (Verde #4ec9b0)          │ │ │
│ │ │  ou [PAUSADO] (Laranja #f0ad4e)         │ │ │
│ │ │  ou [PARADO] (Cinza #858585)            │ │ │
│ │ └─────────────────────────────────────────┘ │ │
│ └─────────────────────────────────────────────┘ │
├─────────────────────────────────────────────────┤
│ CONTENT AREA (Fundo #1e1e1e)                    │
│                                                  │
│ ┌─────────────────────────────────────────────┐ │
│ │ STATUS DA EXECUÇÃO (12px, #858585)         │ │
│ │ ┌─────────────────────────────────────────┐ │ │
│ │ │ Última execução: Sucesso                │ │ │
│ │ │ (Verde #0e6f0e se sucesso)              │ │ │
│ │ │ (Vermelho #c72e2e se falha)             │ │ │
│ │ └─────────────────────────────────────────┘ │ │
│ └─────────────────────────────────────────────┘ │
│                                                  │
│ ┌─────────────────────────────────────────────┐ │
│ │ TEMPO DE EXECUÇÃO DO CICLO (12px, #858585) │ │
│ │ ┌─────────────────────────────────────────┐ │ │
│ │ │       00:15:42.735                      │ │ │
│ │ │   (32px, Bold, Courier New, #007acc)    │ │ │
│ │ └─────────────────────────────────────────┘ │ │
│ └─────────────────────────────────────────────┘ │
│                                                  │
│ ┌──────────┬──────────┬──────────┐             │
│ │ ▶ Iniciar│ ⏸ Pausar │ ⏹ Parar  │             │
│ │  Verde   │  Laranja │  Vermelho│             │
│ │ Gradient │ Gradient │ Gradient │             │
│ └──────────┴──────────┴──────────┘             │
│                                                  │
│ ─────────────────────────────────────────────── │
│                                                  │
│ ┌─────────────────────────────────────────────┐ │
│ │    ✕ Encerrar Aplicação (Vermelho)         │ │
│ └─────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────┘
```

## Paleta de Cores

### Cores de Fundo
- **Background Principal**: `#1e1e1e` (Preto suave)
- **Background Secundário**: `#252526` (Cinza escuro)
- **Background Terciário**: `#2d2d30` (Cinza médio)
- **Bordas**: `#3f3f46` (Cinza)

### Cores de Texto
- **Texto Primário**: `#ffffff` (Branco)
- **Texto Secundário**: `#cccccc` (Cinza claro)
- **Texto Muted**: `#858585` (Cinza médio)
- **Accent Blue**: `#007acc` (Azul)

### Cores de Status
- **Success Green**: `#0e6f0e` (Background), `#4ec9b0` (Badge/Border)
- **Error Red**: `#c72e2e` (Background), `#f48771` (Border)
- **Warning Orange**: `#f0ad4e` (Badge)
- **Neutral Gray**: `#858585` (Badge Stopped)

### Gradientes dos Botões

#### Botão Iniciar (Verde)
```
Cor 1: #4ec9b0 (Verde água)
Cor 2: #3da58a (Verde escuro)
Texto: Preto
Ícone: ▶
```

#### Botão Pausar (Laranja)
```
Cor 1: #f0ad4e (Laranja claro)
Cor 2: #d89a3e (Laranja escuro)
Texto: Preto
Ícone: ⏸
```

#### Botão Parar (Vermelho)
```
Cor 1: #f48771 (Vermelho claro)
Cor 2: #e06751 (Vermelho escuro)
Texto: Branco
Ícone: ⏹
```

#### Botão Encerrar (Vermelho Escuro)
```
Cor 1: #c72e2e (Vermelho)
Cor 2: #a71e1e (Vermelho muito escuro)
Texto: Branco
Ícone: ✕
```

## Estados da Interface

### Estado: Parado
- Badge Header: Cinza (#858585) - "PARADO"
- Status Message: "Pronto para iniciar"
- Timer: "00:00:00.000"
- Botão Iniciar: **Habilitado**
- Botão Pausar: Desabilitado
- Botão Parar: Desabilitado

### Estado: Em Execução
- Badge Header: Verde (#4ec9b0) - "EM EXECUÇÃO"
- Status Message: "Processando..."
- Timer: Atualizado em tempo real (ex: "00:15:42.735")
- Botão Iniciar: Desabilitado
- Botão Pausar: **Habilitado**
- Botão Parar: **Habilitado**

### Estado: Pausado
- Badge Header: Laranja (#f0ad4e) - "PAUSADO"
- Status Message: "Pausado"
- Timer: Congelado no último valor
- Botão Iniciar: **Habilitado**
- Botão Pausar: Desabilitado
- Botão Parar: **Habilitado**

### Indicador de Última Execução

#### Sucesso
- Background: Verde escuro (#0e6f0e)
- Border: Verde claro (#4ec9b0)
- Texto: "Última execução: Sucesso"

#### Falha
- Background: Vermelho escuro (#c72e2e)
- Border: Vermelho claro (#f48771)
- Texto: "Última execução: Falha"

## Características Visuais

1. **Bordas Arredondadas**: Todas as seções e botões têm bordas arredondadas (6-12px)
2. **Espaçamento**: Padding consistente de 16-20px
3. **Tipografia**:
   - Títulos: 20px, SemiBold
   - Labels: 12px, Regular, Uppercase
   - Timer: 32px, Bold, Courier New (monospace)
   - Botões: 14px, SemiBold
4. **Sombras**: Bordas sutis (#3f3f46) para separação de seções
5. **Gradientes**: Botões usam gradientes lineares 135° para aspecto moderno

## Comportamento da Janela

1. **Posicionamento**: Canto inferior direito da tela
2. **Tamanho Fixo**: 400x550 pixels (não redimensionável)
3. **Sempre no Topo**: A janela pode ficar sobre outras janelas
4. **Minimizar para Bandeja**: Ao fechar, minimiza para bandeja do sistema
5. **Menu da Bandeja**:
   - Mostrar: Exibe a janela novamente
   - Ocultar: Esconde a janela
   - Sair: Encerra a aplicação (com confirmação)

## Animações e Feedback

1. **Status Badge**: Animação de pulso quando em execução
2. **Timer**: Atualização suave a cada 100ms
3. **Botões**: Efeito hover com leve mudança de cor
4. **Transições**: Mudanças de cor suaves (fade) ao alternar estados

## Acessibilidade

1. **Contraste**: Todas as combinações de cores atendem WCAG AA
2. **Tamanho de Fonte**: Mínimo 12px, ideal 14px
3. **Alvos de Toque**: Botões com mínimo 44px de altura
4. **Feedback Visual**: Estados claramente diferenciados por cor

Este design segue princípios modernos de UI/UX com foco em:
- **Legibilidade**: Alto contraste, fontes claras
- **Usabilidade**: Controles grandes e bem espaçados
- **Feedback**: Estados visuais claros
- **Estética**: Design dark moderno e profissional
