using Rpa.Curso.Application;
using Rpa.Curso.Application.Helpers;
using Rpa.Curso.Application.Interfaces;
using Rpa.Curso.CrossCutting;
using Rpa.Curso.Extensions;
using Rpa.Curso.Infrastructure.Context;
using Rpa.Curso.IoC.ServiceCollectionExtensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

LoggingSingleton.InitializeLog();
Logging log = LoggingSingleton.GetLogging();

Preambulo:
log.Info("Definição do Host");
var host = Host.CreateDefaultBuilder(args);

log.Info("Configuração do arquivo AppSettings.json");
host.ConfigureAppSettings();

log.Info("Registro dos serviços e repositórios através de Injeção de Dependencia.");
host.ConfigureServices((context, services) =>
{
    services.RegisterServices(context.Configuration, log).AddAutoMapper(typeof(AutoMapperProfiles).Assembly);
});

log.Info("Constrói a aplicação.");
var builder = host.Build();

IServiceScope scope = null;
try
{
    log.Info("Cria escopo de Serviços");
    scope = builder.Services.CreateScope();

    log.Info("Obtém o contexto de dados");
    var dbContext = scope.ServiceProvider.GetRequiredService<DataContext>();
    var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

    string palavraChave = null;
    bool inserirPalavraChaveNoPrompt;
    if (bool.TryParse(configuration["IncluirPalavraChaveNaConsole"], out inserirPalavraChaveNoPrompt) && inserirPalavraChaveNoPrompt)
    {
        Console.Clear();
        Console.WriteLine(".Por favor, insira a palavra-chave que deseja pesquisar no Site de Cursos:");
        palavraChave = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(palavraChave))
        {
            log.Warning("Não é possível utilizar palavras vazias na pesquisa; por favor, tente novamente.");
            goto Preambulo;
        }
    }
    else
    {
        palavraChave = configuration["PalavraChave"];
        if (string.IsNullOrWhiteSpace(palavraChave))
        {
            log.Warning("Não é possível utilizar palavras vazias na pesquisa; por favor, defina a plavara-chave de pesquisa de cursos no arquivo de configuração e reinicie a execução do RPA..");
            while (true)
            {
                Thread.Sleep(TimeSpan.FromSeconds(60));
                log.Error("Aguardando interrupção do Robô... Feche a console!");
            }
        }
    }

    bool recriarBancoDeDados;
    if (bool.TryParse(configuration["RecriarBancoDeDadosEmCadaExecucao"], out recriarBancoDeDados) && recriarBancoDeDados)
    {
        log.Info("Verifica se foi feita a deleção da base de dados. Por favor, aguarde...");
        dbContext.Database.EnsureDeleted();
    }
    
    log.Info("Verifica se já existe ou se precisará ser feita a criação do banco de dados. Por favor, aguarde...");
    dbContext.Database.EnsureCreated();

    var service = scope.ServiceProvider.GetRequiredService<ICourseService>();

    string urlCursos = string.Format(configuration["UrlCursos"], palavraChave.Replace(" ", "+"));
    Uri uriResult;
    if (string.IsNullOrWhiteSpace(urlCursos) || !(Uri.TryCreate(urlCursos, UriKind.Absolute, out uriResult) && uriResult.Scheme == Uri.UriSchemeHttps))
    {
        log.Warning("É impossível navegar numa URL vazia, nula ou inválida; por favor, defina a URL do site de cursos no arquivo de configuração e reinicie a execução do RPA.");
        while (true)
        {
            Thread.Sleep(TimeSpan.FromSeconds(60));
            log.Error("Aguardando interrupção do Robô... Feche a console!");
        }
    }

    log.Info("Inicializará o fluxo do RPA");
    RpaBusinessRule.InitFlow(service, urlCursos);

    Console.Clear();
    Console.WriteLine(".Deseja fazer uma nova pesquisa?");
    Console.WriteLine(".digite S - para sim");
    Console.WriteLine(".ou digite N - para não");
    string desejaPesquisarNovamente = Console.ReadLine();
    if (desejaPesquisarNovamente == "S")
        goto Preambulo;
}
catch(Exception ex)
{
    log.Error($"Surgiu um erro inesperado ao tentar inicializar o RPA, vide mensagem: {ex.Message}");
}
finally
{
    scope.Dispose();
}