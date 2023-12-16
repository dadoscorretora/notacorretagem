#!/bin/python3
import os
import sys
import pandas as pd
import argparse

parser = argparse.ArgumentParser(description='Converte um arquivo CSV para XLSX.', 
                                 formatter_class=argparse.ArgumentDefaultsHelpFormatter)

parser.add_argument('arquivo_csv', metavar='arquivo_csv', type=str, nargs=1,
                    help='Caminho do arquivo CSV a ser convertido em planilha xlsx.')
parser.add_argument('-x','--xlsx', metavar='arquivo_xlsx', type=str, nargs=1, default=argparse.SUPPRESS, required=False,
                    help='Caminho do arquivo XLSX a ser criado.')
parser.add_argument('-s','--separator', metavar='separator_csv', type=str, nargs=1, default=',', required=False,
                    help='Separador de campos do arquivo CSV.')
parser.add_argument('-c', '--column-as-sheet', metavar='column_as_sheet', type=str, nargs=1, default=argparse.SUPPRESS, required=False,
                    help='Seleciona uma coluna (posição) para seprar por aba na planilha.')

args = vars(parser.parse_args())

def show_help():
    parser.print_help()

in_file = args['arquivo_csv'][0]
if (not os.path.exists(in_file)):
    print(f'ERRO: O arquivo \'{in_file}\' CSV informado não existe.')
    show_help()
    sys.exit(1)

in_file_no_ext = in_file.split('.')[0]

def try_new_name(basefilename):
    try_out_file = f'{basefilename}.xlsx'
    count = 1
    while True:
        if os.path.exists(try_out_file):
            try_out_file = f'{basefilename}_copy{count}.xlsx'
            count += 1
        else:
            print(
                f'AVISO: Já existe a planilha \'{basefilename}.xlsx\'.',
                file=sys.stderr)
            return try_out_file

if 'xlsx' in args:
    out_file = args['xlsx'][0]
else :
    out_file = try_new_name(in_file_no_ext)

if 'separator' in args:
    delimiter = args['separator'][0]

def to_decimal(value):
    return float(value.replace('.', '').replace(',', '.'))

df_csv = pd.read_csv(filepath_or_buffer=in_file, delimiter=delimiter,
                     converters={'Movimentacao': to_decimal,
                                 'Custos': to_decimal}
                     )

df_csv['Amortizacao'] = 0
df_csv['Lucro/Prejuizo'] = 0
df_csv['Saldo'] = 0
df_csv['Saldo fisico'] = 0
df_csv['Preco Medio'] = 0

if 'Preco' in df_csv.columns:
    df_csv['Preco'] = df_csv['Preco'].apply(to_decimal)

if 'column_as_sheet' in args:
    column_as_sheet_pos = args['column_as_sheet'][0]
    if isinstance(column_as_sheet_pos, str) and column_as_sheet_pos.isnumeric():
        column_as_sheet_pos = int(column_as_sheet_pos)
    column_name = df_csv.columns[column_as_sheet_pos]
    print(f"AVISO: Usando os dados na coluna '{column_name}' como nomes para as abas.", 
          file=sys.stderr)
    sheet_names = df_csv[column_name].unique()
else:
    sheet_name = in_file_no_ext.split('/')[-1]
    print("AVISO: Usando o nome do arquivo como nome para a aba.",
        file=sys.stderr)
    if len(sheet_name) > 31:
        print(f"AVISO: O nome '{sheet_name}' é muito grande para aba.",
            file=sys.stderr)
        sheet_name = sheet_name[-31:]
        print(f"AVISO: Usando os últimos 31 caracteres para o nome da aba: '{sheet_name}'",
            file=sys.stderr)
    sheet_names = [sheet_name]

if os.path.exists(out_file) and not os.path.isfile(out_file):
    print(
        f'ERRO: O arquivo \'{out_file}\' já existe e não é um arquivo.',
        file=sys.stderr)
    show_help()
    sys.exit(1)

if not os.path.exists(out_file):
    fullpath = out_file
    if out_file.find('/') == -1:
        fullpath = f'{os.path.abspath(os.curdir)}/{out_file}'
    print(
        f'AVISO: Criando nova planilha em \'{fullpath}\'.',
        file=sys.stderr)

for sheet_name in sheet_names:
    df_to_excel = df_csv
    if 'column_as_sheet' in args:
        df_to_excel = df_csv[df_csv[column_name] == sheet_name]
        #df_to_excel['Saldo'] = (df_to_excel['Custos'] + df_to_excel['Movimentacao'] + df_to_excel['Amortizacao'] + 
        #                        df_to_excel['Saldo'] - df_to_excel['Lucro/Prejuizo']).cumsum()
    if os.path.exists(out_file):
        try:
            with pd.ExcelWriter(out_file, mode='a') as writer:
                df_to_excel.to_excel(writer, sheet_name=sheet_name, index=False)
        except ValueError as error:
            if str(error).find("Sheet") != -1 and str(error).find("already exists and if_sheet_exists is set to") != -1:
                print(f"A aba '{sheet_name}' já existe em '{out_file}' e não poderá ser recriada.")
    else:
        df_to_excel.to_excel(out_file, sheet_name=sheet_name, index=False)