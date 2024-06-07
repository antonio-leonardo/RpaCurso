# Rpa.Curso
Este projeto é alusivo ao desafio para desenvolvimento de um RPA que obtém dados básicos de cursos da Alura a partir de uma palavra-chave de entrada mediante a técnica Web-Scraping

## 1) Antes de começar: Prereq
Para que seja possível testar o Robô, requer a IDE [Visual Studio 2022 Community Edition](https://visualstudio.microsoft.com/pt-br/vs/community/) contendo o módulo para desenvolvimento Desktop com C# e a versão do .NET Core 8; além da IDE, é necessário que o ambiente de testes tenha instalado o [SQL Server Express Edition](https://www.microsoft.com/en-us/sql-server/sql-server-downloads), pois, caso não exista o Banco de dados, o RPA cria o banco de dados e faz a persistência em tempo de execução na instância LocalDB (configurada apenas com SQL Server Express)

## 2) Iniciando os teste: Arquivo de parâmetros
No entrypoint do RPA, contém um arquivo de configuração appsetting.json contendo 3 parâmetros: "ConnectionString", "UrlCursos", "RecriarBancoDeDadosEmCadaExecucao", cuja a recomendação é modificar apenas os 3 últimos

```json
{
  "ConnectionStrings": {
    "Cursos": "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=CursosDb;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False"
    //Altere apenas se possuir uma outra instância de Banco de Dados SQL Server personalizada
  },
  "UrlCursos": "https://www.alura.com.br/busca?query={0}&typeFilters=COURSE", //Não alterar
  "RecriarBancoDeDadosEmCadaExecucao": "true", // caso deseje recriar o Banco de Dados a cada nova execução, defina TRUE; caso contrário, defina FALSE
  "IncluirPalavraChaveNaConsole": "true", // caso deseje inserir a palavra-chave de pesquisa na console, defina TRUE; caso contrário, defina FALSE
  "PalavraChave": "Inteligência Artificial" // caso a opção de cima seja FALSE
}
```

## 3) Arquitetura e padrões de projeto adotados
A disposição dos projetos foram planejadas com base em conceitos do [Domain Driven Design](https://www.eduardopires.net.br/2016/08/ddd-nao-e-arquitetura-em-camadas/), com intuito de separar aspectos tecnicos da solução contra aspectos de negócio, além de proporcionar melhor manutenibilidade e reaproveitamento/reuso da codificação

## 4) Sobre a regra de negócio do RPA
As escolhas de desenvolvimento sobre a biblioteca Selenium e do fluxo do RPA foram decidas de acordo com o comportamento do site da Alura; no endereço do site, o mesmo dá suporte a uma rota de busca que permite incluir as queries strings que representam a palavra-chave da busca com intuito de ganhar performance na busca; logo, por isso que a URL foi definida como https://www.alura.com.br/busca?query={0}&typeFilters=COURSE, onde no numeral zero é onde será passada a palavra-chave que deseja pesquisar. Com isso, é possível chegar rapidamente na página da relação de cursos e coletar as URL de cada curso (cada curso tem uma URL própria); caso haja paginação, o RPA consegue navegas nas páginas assim coletando todas as URLs dos cursos. Após coletar todos os links dos cursos, o RPA navega em cada link para capturar as informações de cada curso; existem situações onde o RPA não fará a coleta, como por exemplo, quando o link do curso possui redirect. Após coletadas, todas as informações de cursos ficam na memória do Robô para em seguida serem persistidas no Banco de Dados
