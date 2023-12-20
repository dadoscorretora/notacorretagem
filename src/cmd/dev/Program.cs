using DadosCorretora.NotaCorretagem.Extracao;
using DadosCorretora.NotaCorretagem.Parser;
using DadosCorretora.NotaCorretagem.Calculo;
using System.Diagnostics;

namespace DadosCorretora.NotaCorretagem.Cmd
{
    public class Program
    {
        private const string MODEL_OPT = "--model";
        private const string SHORT_MODEL_OPT = "-m";
        private const string FILE_OPT = "--file";
        private const string SHORT_FILE_OPT = "-f";
        private const string FILE_LIST_OPT = "--file-list";
        private const string SHORT_FILE_LIST_OPT = "-l";
        private const string PREPROCESS_OPT = "--pre-process";
        private const string SHORT_PREPROCESS_OPT = "-p";
        private const string HELP_OPT = "--help";
        private const string SHORT_HELP_OPT = "-h";
        private const string SEPARATOR_OPT = "--separator";
        private const string SHORT_SEPARATOR_OPT = "-s";
        private const string VERSION_OPT = "--version";
        private const string SHORT_VERSION_OPT = "-v";
        private const string VERBOSE_OPT = "--verbose";
        private const string SHORT_VERBOSE_OPT = "-V";
        private const string FILTER_TICKER_OPT = "--filter-ticker";
        private const string SHORT_FILTER_TICKER_OPT = "-t";
        private const string END_OPT = "--";
        private const string DEFAULT_SEPARATOR = ",";
        private static readonly List<string> MODEL_LIST = new List<string>() { "XP2023", "BTG2023" };
        private static readonly string NL = Environment.NewLine;
        private static readonly string VERSION = "0.0.1";
        private static readonly string VERSION_MSG = $"notatocsv version {VERSION}";

        public static void Main(string[] args)
        {
            List<string> optNameList;
            Dictionary<string, string> optAltDict, optHelpDict, optMissingMsgDict;
            List<string>? fileList = new List<string>();
            ConfigOptions(out optNameList, out optAltDict, out optHelpDict, out optMissingMsgDict);

            try
            {
                var argValueDict = new Dictionary<string, string>();

                bool keepGoing = HandleArgs(args, optNameList, optAltDict, optHelpDict, optMissingMsgDict, argValueDict, fileList);

                if (!argValueDict.ContainsKey(SEPARATOR_OPT))
                {
                    argValueDict.Add(SEPARATOR_OPT, DEFAULT_SEPARATOR);
                }

                if (!keepGoing)
                {
                    System.Environment.Exit(0);
                }

                if (argValueDict.ContainsKey(PREPROCESS_OPT))
                {
                    ConvertPdfToHtmlBBox(argValueDict[PREPROCESS_OPT]);
                }
                else if (argValueDict.ContainsKey(MODEL_OPT))
                {
                    if (fileList.Count == 0)
                    {
                        throw new ArgumentException($"{optMissingMsgDict[FILE_OPT]}");
                    }
                    var dadosNota = new DadosNota();
                    foreach (var file in fileList)
                    {
                        argValueDict[FILE_OPT] = file;
                        var dados = HandleModelOption(optMissingMsgDict, MODEL_LIST, argValueDict);
                        dadosNota.Custos.AddRange(dados.Custos);
                        dadosNota.Operacoes.AddRange(dados.Operacoes);
                    }
                    RateioDespesa.Calcular(dadosNota);
                    Output(dadosNota, argValueDict);
                }
                else
                {
                    throw new ArgumentException("Falta informar opções nos argumentos!");
                }

                System.Environment.Exit(0);
            }
            catch (Exception ex)
            {
                if (System.Diagnostics.Debugger.IsAttached)
                    Console.Error.WriteLine(ex.ToString());
                Console.Error.WriteLine($"ERRO: {ex.Message}");
                ShowHelpText(optNameList, optAltDict, optHelpDict);
                System.Environment.Exit(1);
            }
        }
        private static bool HandleArgs(
            string[] args, List<string> optNameList, Dictionary<string, string> optAltDict,
            Dictionary<string, string> optHelpDict, Dictionary<string, string> optMissingMsgDict,
            Dictionary<string, string> argValueDict, List<string> fileList)
        {
            var argList = args.ToList();
            if (argList.Count == 0)
            {
                throw new ArgumentException("Falta informar opções nos argumentos!");
            }

            var argReadCount = 0;
            var jumpArg = 0;
            var onFileListOpt = false;
            foreach (var arg in argList)
            {
                argReadCount++;
                switch (arg)
                {
                    case MODEL_OPT:
                    case SHORT_MODEL_OPT:
                        onFileListOpt = false;
                        jumpArg = 1;
                        GetNextArgAsValue(MODEL_OPT, argList, argReadCount, optMissingMsgDict, argValueDict);
                        continue;
                    case FILE_OPT:
                    case SHORT_FILE_OPT:
                        onFileListOpt = false;
                        jumpArg = 1;
                        GetNextArgAsFileValue(FILE_OPT, argList, argReadCount, optMissingMsgDict, fileList);
                        continue;
                    case FILE_LIST_OPT:
                    case SHORT_FILE_LIST_OPT:
                        onFileListOpt = true;
                        continue;
                    case PREPROCESS_OPT:
                    case SHORT_PREPROCESS_OPT:
                        onFileListOpt = false;
                        jumpArg = 1;
                        GetNextArgAsValue(PREPROCESS_OPT, argList, argReadCount, optMissingMsgDict, argValueDict);
                        return true;
                    case HELP_OPT:
                    case SHORT_HELP_OPT:
                        onFileListOpt = false;
                        ShowHelpText(optNameList, optAltDict, optHelpDict);
                        return false;
                    case SEPARATOR_OPT:
                    case SHORT_SEPARATOR_OPT:
                        onFileListOpt = false;
                        jumpArg = 1;
                        GetNextArgAsValue(SEPARATOR_OPT, argList, argReadCount, optMissingMsgDict, argValueDict);
                        continue;
                    case FILTER_TICKER_OPT:
                    case SHORT_FILTER_TICKER_OPT:
                        onFileListOpt = false;
                        jumpArg = 1;
                        GetNextArgAsValue(FILTER_TICKER_OPT, argList, argReadCount, optMissingMsgDict, argValueDict);
                        continue;
                    case VERSION_OPT:
                    case SHORT_VERSION_OPT:
                        onFileListOpt = false;
                        Console.Error.WriteLine(VERSION_MSG);
                        return false;
                    case VERBOSE_OPT:
                    case SHORT_VERBOSE_OPT:
                        onFileListOpt = false;
                        if (argValueDict.ContainsKey(VERBOSE_OPT))
                        {
                            string verbose = argValueDict[VERBOSE_OPT];
                            argValueDict[VERBOSE_OPT] = $"{int.Parse(verbose) + 1}";
                        }
                        else
                        {
                            argValueDict.Add(VERBOSE_OPT, "1");
                        }
                        continue;
                    case END_OPT:
                        break;
                    default:
                        if (onFileListOpt)
                        {
                            AddArgToFileValue(FILE_LIST_OPT, arg, optMissingMsgDict, fileList);
                            continue;
                        }
                        if (jumpArg > 0)
                        {
                            jumpArg--;
                            continue;
                        }
                        throw new ArgumentException($"'{arg}' não é uma opção reconhecida!");
                }
            }
            return true;
        }

        private static DadosNota HandleModelOption(Dictionary<string, string> optMissingMsgDict,
            List<string> modelList, Dictionary<string, string> argValueDict)
        {
            argValueDict[MODEL_OPT] = argValueDict[MODEL_OPT].ToUpper();
            var model = argValueDict[MODEL_OPT];
            var found = modelList.Exists(v => v == model);
            if (found == false)
            {
                throw new ArgumentException($"Modelo '{model}' não é reconhecido.");
            }
            if (!argValueDict.ContainsKey(FILE_OPT))
            {
                throw new ArgumentException($"{optMissingMsgDict[FILE_OPT]}");
            }
            if (File.Exists(argValueDict[FILE_OPT]) == false)
            {
                throw new FileNotFoundException($"O arquivo '{argValueDict[FILE_OPT]}' não foi encontrado.");
            }

            DadosNota? dadosNota = null;
            var filePath = argValueDict[FILE_OPT];

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"O arquivo '{filePath}' não foi encontrado.");
            }

            if (filePath.EndsWith(".pdf", StringComparison.InvariantCultureIgnoreCase))
            {
                filePath = ConvertPdfToHtmlBBox(filePath);
            }

            if (model == "XP2023")
            {
                dadosNota = ExtractXP2023(filePath);
            }
            else if (model == "BTG2023")
            {
                dadosNota = ExtractBTG2023(filePath);
            }
            else
            {
                throw new ArgumentException($"Modelo '{model}' não é reconhecido.");
            }

            if (dadosNota == null || dadosNota.Operacoes == null || dadosNota.Custos == null)
            {
                throw new Exception("Dados não encontrados após processar!");
            }
            return dadosNota;
        }

        private static void Output(DadosNota dadosNota, Dictionary<string, string> argValueDict)
        {
            var separator = argValueDict[SEPARATOR_OPT];
            var verbose = argValueDict.ContainsKey(VERBOSE_OPT) ? int.Parse(argValueDict[VERBOSE_OPT]) : 0;

            var header = "";
            if (verbose > 0)
            {
                header += $"Titulo{separator}";
            }
            header += $"Ativo{separator}Data{separator}Nota{separator}";
            header += $"Quantidade{separator}Preco{separator}Custos";
            if (verbose > 0)
            {
                header += $"{separator}Movimentacao";
            }
            Console.WriteLine(header);

            var Br = new System.Globalization.CultureInfo("pt-BR");

            string ByCodigoOuTitulo(DadosNota.Operacao o)
            {
                if (o.CodigoAtivo != null && o.CodigoAtivo.Length > 0)
                    return o.CodigoAtivo;
                else
                {
                    return o.Titulo;
                }
            }

            var operacoes = dadosNota.Operacoes;
            if (argValueDict.ContainsKey(FILTER_TICKER_OPT))
            {
                bool Match(DadosNota.Operacao o, string filter)
                {
                    if (!string.IsNullOrEmpty(o.CodigoAtivo))
                    {
                        return o.CodigoAtivo == filter;
                    }
                    else if (!string.IsNullOrEmpty(o.Titulo))
                    {
                        return o.Titulo == filter;
                    }
                    return false;
                }
                operacoes = operacoes.FindAll(o => Match(o, argValueDict[FILTER_TICKER_OPT]));
            }
            operacoes.OrderBy(o => ByCodigoOuTitulo(o)).
                ThenBy(o => o.DataNota).ThenBy(o => o.NumeroNota).
                ToList().ForEach(o =>
            {
                var dataNota = o.DataNota.ToString("dd/MM/yyyy");
                var preco = $"{o.Preco.ToString(Br)}";
                if (preco.Contains(separator))
                {
                    preco = $"\"{preco}\"";
                }
                var custoOperacao = $"{o.CustoOperacao.ToString(Br)}";
                if (custoOperacao.Contains(separator))
                {
                    custoOperacao = $"\"{custoOperacao}\"";
                }
                var valorOperacao = $"{o.ValorOperacao.ToString(Br)}";
                if (valorOperacao.Contains(separator))
                {
                    valorOperacao = $"\"{valorOperacao}\"";
                }
                var row = "";
                if (verbose > 0)
                {
                    row += $"{o.Titulo}{separator}";
                }
                row += $"{o.CodigoAtivo}{separator}{dataNota}{separator}{o.NumeroNota}{separator}";
                row += $"{o.Quantidade}{separator}{preco}{separator}{custoOperacao}";
                if (verbose > 0)
                {
                    row += $"{separator}{valorOperacao}";
                }
                Console.WriteLine(row);
            });
        }

        private static string ConvertPdfToHtmlBBox(string filePath)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo() { FileName = "pdftotext", Arguments = $"-bbox {filePath}", };
            Process proc = new Process() { StartInfo = startInfo, };
            var started = proc.Start();
            if (!started)
            {
                throw new Exception("Não foi possível iniciar o processo pdftotext, verifique se ele está no PATH.");
            }
            var exited = proc.WaitForExit(10000);
            if (!exited)
            {
                throw new Exception($"O processo pdftotext não terminou em 10 segundos.\n" +
                    $"Tente manualmente com 'pdftotext -bbox {filePath}'.");
            }
            var htmlBBoxFilePath = filePath.Replace(".pdf", ".html");
            if (!File.Exists(htmlBBoxFilePath))
            {
                var pdftotxtOutput = proc.StandardOutput.ReadToEnd() + NL + proc.StandardError.ReadToEnd();
                throw new FileNotFoundException($"O HTML 'bbox' do PDF '{filePath}' não foi encontrado." +
                $"E a saida do pdftotext foi:" + NL + $"{pdftotxtOutput}");
            }
            return htmlBBoxFilePath;
        }

        public static void ShowHelpText(
            List<string> argNameList,
            Dictionary<string, string> argAltDict,
            Dictionary<string, string> argHelpDict)
        {
            var txtHelp = NL +
                VERSION_MSG + NL +
                "Uso: notatocsv [OPCAO]..." + NL +
                "Converte nota de corretagem em formato PDF ou HTML para CSV na saida (stdout)." + NL +
                NL +
                "Atenção: Este programa depende do pdftotext estar instalado." + NL +
                NL +
                "Opções:" + NL;
            foreach (var arg in argNameList)
            {
                var option = argAltDict.ContainsKey(arg) ? $"{argAltDict[arg]}, {arg}" : $"{arg}";
                option = option.PadRight(20);
                txtHelp += $"{option}: {argHelpDict[arg]}" + NL;
            }
            txtHelp += NL +
                "Exemplos de uso:" + NL +
                "notatocsv --model XP2023 --file nota.html" + NL +
                "notatocsv -m BTG2023 --file nota.pdf > nota.csv" + NL +
                "notatocsv -m XP2023 -f nota.pdf -s \"|\"" + NL +
                "notatocsv --pre-process nota.pdf" + NL +
                NL;
            txtHelp += NL +
                "BSD Zero Clause License" + NL +
                "Copyright (C) 2023" + NL +
                NL +
                "Permission to use, copy, modify, and/or distribute this software for any" + NL +
                "purpose with or without fee is hereby granted." + NL +
                NL +
                "THE SOFTWARE IS PROVIDED \"AS IS\" AND THE AUTHOR DISCLAIMS ALL WARRANTIES WITH" + NL +
                "REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF MERCHANTABILITY AND" + NL +
                "FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY SPECIAL, DIRECT, " + NL +
                "INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES WHATSOEVER RESULTING FROM LOSS" + NL +
                "OF USE, DATA OR PROFITS, WHETHER IN AN ACTION OF CONTRACT, NEGLIGENCE OR OTHER " + NL +
                "TORTIOUS ACTION, ARISING OUT OF OR IN CONNECTION WITH THE USE OR PERFORMANCE OF " + NL +
                "THIS SOFTWARE.";
            Console.Out.WriteLine(txtHelp);
        }

        private static void GetNextArgAsValue(
            string option, IList<string> argList, int argReadCount,
            IDictionary<string, string> optMissingMsgDict,
            IDictionary<string, string> argValueDict)
        {
            var copy = new List<string>(argList);
            string? modelValue;
            try
            {
                modelValue = copy.Skip(argReadCount).First();
            } catch (Exception) {
                throw new ArgumentException($"{optMissingMsgDict[option]}");
            }
            if (string.IsNullOrEmpty(modelValue))
            {
                throw new ArgumentException($"{optMissingMsgDict[option]}");
            }
            argValueDict.Add(option, modelValue);
        }
        private static void GetNextArgAsFileValue(
            string option, IList<string> argList, int argReadCount,
            IDictionary<string, string> optMissingMsgDict,
            List<string> argValueList)
        {
            var copy = new List<string>(argList);
            string? modelValue;
            try
            {
                modelValue = copy.Skip(argReadCount).First();
            } catch (Exception) {
                throw new ArgumentException($"{optMissingMsgDict[option]}");
            }
            if (string.IsNullOrEmpty(modelValue))
            {
                throw new ArgumentException($"{optMissingMsgDict[option]}");
            }
            argValueList.Add(modelValue);
        }

        private static void AddArgToFileValue(
            string option, string fileValue,
            IDictionary<string, string> optMissingMsgDict,
            List<string> fileList)
        {
            if (string.IsNullOrEmpty(fileValue))
            {
                throw new ArgumentException($"{optMissingMsgDict[option]}");
            }
            fileList.Add(fileValue);
        }

        private static void ConfigOptions(
            out List<string> argNameList, out Dictionary<string, string> argAltDict,
            out Dictionary<string, string> argHelpDict, out Dictionary<string, string> argMissingDict)
        {
            argNameList = new List<string>();
            argAltDict = new Dictionary<string, string>();
            argHelpDict = new Dictionary<string, string>();
            argMissingDict = new Dictionary<string, string>();
            argNameList.Add(MODEL_OPT);
            argAltDict.Add(MODEL_OPT, SHORT_MODEL_OPT);
            argHelpDict.Add(MODEL_OPT, $"O modelo da nota de corretagem. Pode ser: {string.Join(", ", MODEL_LIST)}.");
            argMissingDict.Add(MODEL_OPT, "O modelo da nota não foi informado.");

            argNameList.Add(FILE_OPT);
            argAltDict.Add(FILE_OPT, SHORT_FILE_OPT);
            argHelpDict.Add(FILE_OPT, "O arquivo da nota de corretagem no formato pdf ou html 'bbox'.");
            argMissingDict.Add(FILE_OPT, $"Um ou mais arquivos de nota devem ser informados com '{FILE_OPT}' ou '{FILE_LIST_OPT}'.");

            argNameList.Add(FILE_LIST_OPT);
            argAltDict.Add(FILE_LIST_OPT, SHORT_FILE_LIST_OPT);
            argHelpDict.Add(FILE_LIST_OPT, "Uma lista de arquivos de nota de corretagem no formato pdf ou html 'bbox'.");
            argMissingDict.Add(FILE_LIST_OPT, $"Um ou mais arquivos de nota devem ser informados com '{FILE_OPT}' ou '{FILE_LIST_OPT}'.");

            argNameList.Add(PREPROCESS_OPT);
            argAltDict.Add(PREPROCESS_OPT, SHORT_PREPROCESS_OPT);
            argHelpDict.Add(PREPROCESS_OPT, "Pré-processa o pdf da nota de corretagem para html 'bbox'.");
            argMissingDict.Add(PREPROCESS_OPT, "O arquivo PDF da nota não foi informado.");

            argNameList.Add(FILTER_TICKER_OPT);
            argAltDict.Add(FILTER_TICKER_OPT, SHORT_FILTER_TICKER_OPT);
            argHelpDict.Add(FILTER_TICKER_OPT, "Filtra pelo código do ativo, se não tiver tenta pelo título.");
            argMissingDict.Add(FILTER_TICKER_OPT, "O nome ativo a ser filtrado não foi informado.");

            argNameList.Add(HELP_OPT);
            argAltDict.Add(HELP_OPT, SHORT_HELP_OPT);
            argHelpDict.Add(HELP_OPT, "Exibe esta mensagem de ajuda.");

            argNameList.Add(SEPARATOR_OPT);
            argAltDict.Add(SEPARATOR_OPT, SHORT_SEPARATOR_OPT);
            argHelpDict.Add(SEPARATOR_OPT, "O separador de campos do CSV. O padrão é ','.");
            argMissingDict.Add(SEPARATOR_OPT, "O caractere separador do CSV não foi informado.");

            argNameList.Add(END_OPT);
            argHelpDict.Add(END_OPT, "Indica explicitamente o fim das opções.");
        }

        public static DadosNota ExtractXP2023(string nomeArquivo)
        {
            var xmlDoc = new System.Xml.XmlDocument();
            xmlDoc.Load(nomeArquivo);
            IEnumerable<IEnumerable<TextCell>> paginas = PdfToHtmlReader.Read(xmlDoc);

            var dados = new List<Dados>();
            foreach (var pagina in paginas)
            {
                var dadosPg = XPBolsa2023.ExtraiDeHTML_BBox(pagina);
                dados.AddRange(dadosPg);
            }
            var dadosNota = XPBolsa2023.TraduzDadosTipados(dados);
            return dadosNota;
        }

        public static DadosNota ExtractBTG2023(string nomeArquivo)
        {
            var ret = new DadosNota();

            var doc = new System.Xml.XmlDocument();
            doc.Load(nomeArquivo);
            var paginas = PdfToHtmlReader.Read(doc);

            foreach (var pagina in paginas)
                BtgBolsa2023.Extrai(pagina, ret);

            return ret;
        }
    }
}