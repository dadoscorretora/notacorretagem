# notatocsv
Extrai dados de corretagem de várias corretoras.

Use a ajuda (help) para ver como usar o ```notatocsv```:
```bash
notatocsv --help
```

## Construir
O projeto gera um executável autocontido (que não depende de dll): ```notatocsv```

Para construir, na pasta ```src``` do projeto com o arquivo ```NotaCorretagem.sln```, use o comando:
```bash
dotnet publish --configuration Release
```

O executável ```notatocsv``` será gerado na subpasta:
```bash
src/cmd/dev/bin/Release/net7.0/linux-x64/publish/
```

## Mineração de nota de corretagem em PDF
Esse projeto usa o utilitário ```pdftotext``` para minerar as notas de corretagem em PDF.

Usando a opção *bbox* o ```pdftotext``` gera um html com caixas com a posição e tamanho de cada palavra e página do PDF.

Então, a posição de palavras-chave são usadas como referência para descobrir informações da nota.

O comando ```pdftotext``` é usado da seguinte forma:
```bash
pdftotext -bbox /caminho/arquivo.pdf
```

## Estrutura do projeto
- ```src```
    - ```lib``` - Código da mineração de cada modelo de nota.
    - ```cmd/dev``` - Código do executável ```notatocsv```.
