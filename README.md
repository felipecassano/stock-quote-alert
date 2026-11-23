# Stock Quote Alert

Aplicação de console em C# (.NET 10) que monitora a cotação de um ativo da B3 e dispara um alerta por e-mail quando o preço sobe acima do valor de venda ou cai abaixo do valor de compra.

```bash
stock-quote-alert.exe PETR4 22.67 22.59
```

## Como funciona

A aplicação consulta a cotação atual na [brapi.dev](https://brapi.dev) logo ao iniciar e, depois, a cada intervalo configurado. Em cada consulta, avalia em qual faixa o preço está:

- **Acima do preço de venda (`preço > venda`):** envia um e-mail aconselhando a venda.
- **Entre os dois valores (`compra <= preço <= venda`):** faixa neutra, não envia nada.
- **Abaixo do preço de compra (`preço < compra`):** envia um e-mail aconselhando a compra.

Para evitar spam na caixa de entrada, o alerta só é disparado quando há **mudança de faixa**. Se o ativo passar várias horas acima do preço de venda, apenas um e-mail é enviado. Caso o preço volte para a faixa neutra e rompa novamente depois, um novo alerta é disparado. Como a aplicação começa na faixa neutra, um preço que já esteja fora dela na primeira consulta gera o alerta imediatamente.

Tratamento de falhas:

- **Ativo inexistente:** se a brapi responder que o ativo não existe (HTTP 404), a aplicação registra o erro e encerra com código de saída `1`.
- **Token ausente ou inválido:** se a brapi recusar a consulta por falta de token ou por token inválido (HTTP 401), a aplicação avisa que é preciso configurar a `BRAPI_KEY` e encerra com código de saída `1`.
- **Instabilidade na API:** as chamadas têm retry com backoff, circuit breaker e timeout. Se mesmo assim a consulta falhar, o estado atual é preservado, nenhum e-mail é enviado e a consulta é repetida no próximo ciclo.
- **Falha no envio do e-mail:** o estado **não** é atualizado, garantindo que o alerta seja tentado novamente no próximo ciclo.

## Pré-requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Conta de e-mail com acesso SMTP (no Gmail, é preciso ativar a verificação em duas etapas e gerar uma [senha de app](https://myaccount.google.com/apppasswords))
- Token da [brapi.dev](https://brapi.dev) (o plano gratuito já atende). Alguns ativos, como `PETR4` e `VALE3`, podem ser consultados sem token, mas a maioria exige.

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
    "Password": "",
    "Username": ""
  },
  "PollingIntervalSeconds": 30
}
```

- `ToEmail`: endereço que receberá os alertas.
- `SmtpServer` / `SmtpPort`: host e porta do servidor SMTP. A conexão usa STARTTLS, então use uma porta compatível (normalmente `587`).
- `FromName` / `FromEmail`: nome de exibição e endereço do remetente.
- `Password`: senha do SMTP. Pode ser preenchida direto no arquivo para testes locais, mas evite commitar credenciais (veja abaixo).
- `Username`: usuário para autenticação no SMTP. Se ficar vazio, o `FromEmail` é usado, como no Gmail. Preencha quando o provedor usar um login diferente do endereço de e-mail, como Mailtrap ou SendGrid.
- `PollingIntervalSeconds`: intervalo entre consultas, em segundos (padrão: `30`). Para deixar a aplicação rodando por muito tempo, considere aumentar o intervalo para não esgotar a cota de requisições do plano gratuito da brapi.

Na inicialização, o programa verifica se `ToEmail` e `Password` estão preenchidos e encerra com um aviso caso algum esteja vazio.

O arquivo também tem uma seção `Logging`, que esconde os logs automáticos do host, do `HttpClient` e do Polly e mostra cada mensagem em uma única linha, com horário.

### Credenciais fora do repositório

O token da brapi (`BRAPI_KEY`) e a senha do SMTP podem ficar fora do `appsettings.json` de duas formas.

Com [user-secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets), que a aplicação carrega em qualquer ambiente, tanto no `dotnet run` quanto no executável publicado (na mesma máquina):

```bash
cd src/stock-quote-alert
dotnet user-secrets set "BRAPI_KEY" "<seu-token-brapi>"
dotnet user-secrets set "EmailSettings:Password" "<sua-senha-de-app>"
```

Ou com variáveis de ambiente:

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

Qualquer propriedade do `appsettings.json` pode ser sobrescrita por variável de ambiente seguindo o mesmo padrão (ex.: `EmailSettings__ToEmail`). Se a mesma chave estiver definida nos dois lugares, o valor do user-secrets prevalece.

## Execução

Rodando direto pelo SDK:

```bash
dotnet run --project src/stock-quote-alert -- PETR4 22.67 22.59
```

Ordem dos argumentos:

1. Código do ativo (ex.: `PETR4`; maiúsculas e minúsculas são aceitas)
2. Preço de referência para venda
3. Preço de referência para compra

Os preços aceitam ponto ou vírgula como separador decimal (`22.67` ou `22,67`). No PowerShell, use aspas em valores com vírgula (`"22,67"`), porque a vírgula sem aspas é tratada como separador de lista. O preço de venda precisa ser maior que o de compra, e os dois precisam ser maiores que zero. Use `Ctrl+C` para encerrar o monitoramento.

Códigos de saída:

- `0`: encerramento normal com `Ctrl+C`.
- `1`: argumentos inválidos, configuração incompleta, ativo não encontrado ou token da brapi ausente/inválido.

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

A suíte usa xUnit, roda automaticamente no GitHub Actions a cada push e tem foco na regra de negócio:

- **`StockAlertServiceTest`:** alertas de compra e de venda, faixa neutra sem envio, antispam (o preço continua na mesma faixa e o e-mail não se repete), falha na API mantendo o estado e falha no envio do e-mail impedindo que o estado avance. A cotação e o envio são substituídos por fakes de `IQuoteService` e `IEmailSender`, então os testes não dependem de rede.
- **`ArgsParserTest`:** leitura de argumentos válidos.

`QuoteService` e `EmailSender` não têm testes unitários porque são integrações com serviços externos (brapi e SMTP); validá-los de verdade exigiria testes de integração. O `PeriodicHostedService` apenas agenda as consultas e guarda o estado entre elas.

## Estrutura do projeto

```text
src/stock-quote-alert/
├── Program.cs                    # setup de DI, configuração e HttpClient
├── appsettings.json
├── Config/                       # EmailSettings
├── Models/                       # Args
├── Interfaces/                   # IEmailSender, IQuoteService
└── Services/
    ├── ValidateService.cs        # ArgsParser: validação dos argumentos de entrada
    ├── QuoteService.cs           # integração com a brapi.dev
    ├── EmailSender.cs            # envio SMTP usando MailKit
    ├── StockAlertService.cs      # lógica de faixas e disparo de alertas
    └── PeriodicHostedService.cs  # loop de monitoramento (BackgroundService)
tests/stock-quote-alert.Tests/
├── ArgsParserTest.cs
└── StockAlertServiceTest.cs
```

## Escolhas técnicas

- **Generic Host, `BackgroundService` e `PeriodicTimer`:** trazem injeção de dependência, configuração, logging e desligamento gracioso com `Ctrl+C` de forma nativa no .NET.
- **brapi.dev:** API voltada para ativos da B3, simples de integrar e com plano gratuito.
- **MailKit:** usado no lugar do `System.Net.Mail.SmtpClient`, que a própria Microsoft [não recomenda](https://learn.microsoft.com/dotnet/api/system.net.mail.smtpclient#remarks) para projetos novos.
- **`IEmailSender` e `IQuoteService`:** desacoplam a regra de alerta do cliente SMTP e da API de cotações. Nos testes, as duas interfaces são substituídas por fakes.
- **Resiliência HTTP (`Microsoft.Extensions.Http.Resilience`):** aplica retry com backoff exponencial, circuit breaker e timeout nas chamadas à API de cotações.
- **Options pattern:** mapeia o `appsettings.json` para a classe tipada `EmailSettings`.
- **Encerramento controlado:** quando o ativo não existe ou a brapi recusa o token, situações em que repetir a consulta não adiantaria, a aplicação usa `IHostApplicationLifetime.StopApplication()` em vez de `Environment.Exit`, para que o host encerre de forma limpa.
- **Build sem warnings:** o projeto principal usa `TreatWarningsAsErrors`, então qualquer warning novo quebra o build e o CI.

## Uso de IA

Ferramentas de IA foram usadas como apoio ao longo do projeto. A arquitetura, a regra de negócio, a escolha da API e das bibliotecas e a revisão final do código foram conduzidas por mim.

- **Claude Code (Anthropic — modelo Opus 5.5):** revisão de conformidade com os requisitos e sugestões de refatoração/robustez, apoio na escrita dos testes do StockAlertService, workflow de CI e rascunho/revisão deste README.
- **Gemini (Google):** dúvidas pontuais de sintaxe em C#, revisão de métodos e comparação de recursos do .NET/C# com outras linguagens com as quais já tenho familiaridade.
