# notatocsv
Funcionalidades deste utilitário:

    - Extrai dados de notas de corretagem;
    - Calcula o radeio da despesa da nota por operação;
    - Gera saída das operações e rateio da despesa em CSV.

Use a ajuda (help) para ver como usar o ```notatocsv```:
```bash
notatocsv --help
```

## Como construir?
O projeto gera o executável ```notatocsv```. 

Para construir, use o script ```build.sh``` na pasta raiz do projeto. 
```bash
./build.sh
```

A configuração padrão do script ```build.sh``` é a *Release*, mas você pode passar outras como argumento. Por exemplo:
```bash
./build.sh Debug
```

Para construir manualmente você pode usar o comando ```dotnet```. Por exemplo, na pasta ```src``` com o arquivo ```NotaCorretagem.sln```, use o comando ```dotnet``` com os seguintes parâmetros:
```bash
dotnet publish --configuration Release
```

O executável ```notatocsv``` deve ser gerado em:
```bash
src/cmd/dev/bin/Release/net7.0/linux-x64/publish/
```
## Como funciona? (Por baixo do capô)
### Dependências
O notatocsv depende do utilitário ```pdftotext``` instalado para extrair as informações das notas de corretagem em pdf.

### Como são extraídos os dados das notas em pdf?
Usando o utilitário ```pdftotext``` com a opção ``bbox``, é gerado um html com caixas com a posição e tamanho de cada palavra e página do pdf da nota de corretagem.

Então, a posição de palavras-chave são usadas como referência para descobrir a posição relativa das informações da nota.

Para gerar o html com as caixas de posição o comando ```pdftotext``` é usado da seguinte forma:
```bash
pdftotext -bbox /caminho/notadecorretagem.pdf
```

Exemplo de trecho de caixas de posição de nota de corretagem no formato html bbox:
```html
<doc>
  <page width="595.000000" height="842.000000">
    <word xMin="119.112926" yMin="44.737208" xMax="141.594407" yMax="52.637398">NOTA</word>
    <word xMin="143.324373" yMin="44.737208" xMax="154.939526" yMax="52.637398">DE</word>
    <word xMin="156.006559" yMin="44.737208" xMax="211.259964" yMax="52.637398">NEGOCIAÇÃO</word>
    <word xMin="431.267652" yMin="54.613641" xMax="437.747097" yMax="59.345254">Nr.</word>
    <word xMin="438.761672" yMin="54.613641" xMax="446.820625" yMax="59.345254">nota</word>
    <word xMin="475.655305" yMin="54.613641" xMax="486.020110" yMax="59.345254">Folha</word>
    <word xMin="437.896977" yMin="60.879862" xMax="465.982255" yMax="67.977281">99999999</word>
    <word xMin="129.777492" yMin="79.453343" xMax="139.119075" yMax="86.424098">XP</word>
    <word xMin="140.153826" yMin="79.453343" xMax="198.071066" yMax="86.424098">INVESTIMENTOS</word>
```

### Qual a estrutura do projeto?
- ```./```: Raiz do projeto.
    - ```src```: Raiz do projeto C# com o arquivo ```*.sln```.
        - ```lib```: Código da mineração do html bbox para cada modelo de nota suportado.
            - ```Extração```: Código de mineração dos modelos de nota suportados.
            - ```Calculo```: Código com os dados tipados e de rateio de despesas por operação.
            ```Parser```: Código base para recuperação dos elementos nos arquivos html bbox.
        - ```cmd/dev``` - Código do executável ```notatocsv```.
