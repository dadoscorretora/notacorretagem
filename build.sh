#!/bin/bash

config="${1:-Release}"

dotnet clean --configuration "$config" ./src 1>&2
dotnet build --configuration "$config" ./src 1>&2
dotnet publish --configuration "$config" ./src 1>&2

exec_file=$(find ./src -path */src/*/bin/Release/*/linux-x64/publish/notatocsv)
find_exit=$?

if [ $find_exit -ne 0 ]; then
    echo "" >&2
    echo "Não foi possível encontrar o executável notatocsv em:" >&2
    echo "$exec_file" >&2
    echo "Verifique se o projeto foi compilado corretamente." >&2
    exit 1
fi

echo ""
echo "Executável notatocsv foi gerado com sucesso!"
echo ""
echo "Você pode copiar o executável de:"
echo "$exec_file"
echo ""
echo "Foi usada a configuração '$config' para gerar o executável."
echo "Você pode passar outras configurações como parâmetro para gerar o binário."
echo "Exemplo:"
echo "  $0 Debug"
exit 0