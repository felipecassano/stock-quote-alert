# Stock Quote Alert

Aplicação de console em C# (.NET 10) que monitora a cotação de um ativo da B3 e dispara um alerta por e-mail quando o preço ultrapassa o valor de venda ou cai abaixo do valor de compra.

```bash
stock-quote-alert.exe PETR4 22.67 22.59
```

## Como funciona

Em intervalos regulares, a aplicação consulta a cotação atual na [brapi.dev](https://brapi.dev) e avalia em qual faixa o preço está:

- **Acima do preço de venda (`preço > venda`):** envia um e-mail aconselhando a venda.
- **Entre os dois valores (`compra <= preço <= venda`):** zona neutra, não envia nada.
- **Abaixo do preço de compra (`preço < compra`):** envia um e-mail aconselhando a compra.

Para evitar spam na caixa de entrada, o alerta só é disparado quando há **mudança de faixa**. Se o ativo passar várias horas acima do preço de venda, apenas um e-mail é enviado. Caso o preço volte para a faixa neutra e rompa novamente depois, um novo alerta é disparado.

Dois cuidados adicionais no fluxo:

- Se a consulta à API falhar (instabilidade ou ticker inválido), o estado atual é preservado e nenhum e-mail é enviado.
- Se o envio do e-mail falhar, o estado **não** é atualizado, garantindo que o alerta seja tentado novamente no próximo ciclo.

## Pré-requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Token da [brapi.dev](https://brapi.dev) (o plano gratuito já atende)
- Conta de e-mail com acesso SMTP (no Gmail, é preciso ativar a verificação em duas etapas e gerar uma [senha de app](https://myaccount.google.com/apppasswords))

## Configuração

As configurações ficam em [`src/stock-quote-alert/appsettings.json`](src/stock-quote-alert/appsettings.json):

```json
{
  "EmailSettings": {
    "ToEmail": "destino@exemplo.com",
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "FromName": "Stock Quote Alert",
    "FromEmail": "remetente@gmail.com",
    "Password": ""
  },
  "PollingIntervalSeconds": 120
}
```

- `ToEmail`: endereço que receberá os alertas.
- `SmtpServer` / `SmtpPort`: host e porta do servidor SMTP (conexão com STARTTLS).
- `FromName` / `FromEmail`: nome e endereço do remetente (`FromEmail` também é usado na autenticação SMTP).
- `Password`: senha do SMTP. Pode ser preenchida direto no arquivo para testes locais, mas evite commitar credenciais — prefira `user-secrets` ou variáveis de ambiente.
- `PollingIntervalSeconds`: intervalo entre consultas em segundos. O padrão de `120` evita estourar o limite do plano gratuito da brapi, mas pode ser reduzido para testar mais rápido.

Na inicialização, o programa valida se `ToEmail` e `Password` estão preenchidos e encerra com aviso caso algum esteja vazio.

### Credenciais fora do repositório

Para rodar em desenvolvimento (`dotnet run`), você pode usar [user-secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets):

```bash
cd src/stock-quote-alert
dotnet user-secrets set "BRAPI_KEY" "<seu-token-brapi>"
dotnet user-secrets set "EmailSettings:Password" "<sua-senha-de-app>"
```

Para o executável publicado (ambiente `Production`, onde `user-secrets` não é carregado automaticamente), use variáveis de ambiente — ou preencha o `appsettings.json` na pasta do executável:

```bash
# Linux / macOS
export BRAPI_KEY="<seu-token-brapi>"
export EmailSettings__Password="<sua-senha-de-app>"
```

```powershell
# Windows (PowerShell)
$env:BRAPI_KEY = "<seu-token-brapi>"
$env:EmailSettings__Password = "<sua-senha-de-app>"
```

Qualquer propriedade do `appsettings.json` pode ser sobrescrita por variável de ambiente seguindo o mesmo padrão (ex.: `EmailSettings__ToEmail`).

## Execução

Rodando direto pelo SDK:

```bash
dotnet run --project src/stock-quote-alert -- PETR4 22.67 22.59
```

Ordem dos argumentos:

1. Código do ativo (ex.: `PETR4`)
2. Preço de referência para venda
3. Preço de referência para compra

O preço de venda precisa ser maior que o de compra e os valores usam ponto como separador decimal (`22.67`). Use `Ctrl+C` para encerrar o monitoramento.

### Gerando o executável

```bash
dotnet publish src/stock-quote-alert -c Release -r win-x64 -o publish
publish\stock-quote-alert.exe PETR4 22.67 22.59
```

Para Linux ou macOS, basta trocar `win-x64` por `linux-x64` ou `osx-arm64`. O `appsettings.json` é copiado junto no publish e lido a partir do diretório do executável, então o binário pode ser chamado de qualquer pasta.

## Testes

```bash
dotnet test
```

A suíte com xUnit cobre a validação dos parâmetros de linha de comando e roda automaticamente no GitHub Actions a cada push.

## Estrutura do projeto

```text
src/stock-quote-alert/
├── Program.cs                    # setup de DI, configuração e HttpClient
├── appsettings.json
├── Config/                       # EmailSettings
├── Models/                       # Args
├── Types/                        # IEmailSender
└── Services/
    ├── ValidateService.cs        # validação dos argumentos de entrada
    ├── QuoteService.cs           # integração com a brapi.dev
    ├── EmailSender.cs            # envio SMTP usando MailKit
    ├── StockAlertService.cs      # lógica de faixas e disparo de alertas
    └── PeriodicHostedService.cs  # loop de monitoramento (BackgroundService)
tests/stock-quote-alert.Tests/    # testes unitários (xUnit)
```

## Escolhas técnicas

- **Generic Host, `BackgroundService` e `PeriodicTimer`:** trazem injeção de dependência, configuração, logging e desligamento gracioso com `Ctrl+C` de forma nativa no .NET.
- **brapi.dev:** API voltada para ativos da B3, simples de integrar e com plano gratuito.
- **MailKit:** usado no lugar do `System.Net.Mail.SmtpClient`, que a própria Microsoft [não recomenda](https://learn.microsoft.com/dotnet/api/system.net.mail.smtpclient#remarks) para projetos novos.
- **`IEmailSender`:** desacopla a regra de disparo do cliente SMTP, facilitando manutenção e testes futuros.
- **Resiliência HTTP (`Microsoft.Extensions.Http.Resilience`):** aplica retry com backoff exponencial, circuit breaker e timeout nas chamadas à API de cotações.
- **Options pattern:** mapeia o `appsettings.json` para a classe tipada `EmailSettings`.

## Uso de IA

Ferramentas de IA foram usadas como apoio pontual ao longo do projeto. A arquitetura, a regra de negócio, a escolha da API e bibliotecas, os testes e a revisão final do código foram conduzidos por mim.

- **Claude Code (Anthropic — modelo Opus 5.5):** apoio na revisão de conformidade com os requisitos, ajustes de leitura/cópia do `appsettings.json` no build, configuração inicial de resiliência no `HttpClient`, workflow de CI e rascunho inicial deste README.
- **Gemini (Google):** dúvidas pontuais de sintaxe em C#, revisão de métodos e comparação de recursos do .NET/C# com outras linguagens com as quais já tenho familiaridade.
