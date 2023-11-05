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
        private const string PREPROCESS_OPT = "--pre-process";
        private const string SHORT_PREPROCESS_OPT = "-p";
        private const string HELP_OPT = "--help";
        private const string SHORT_HELP_OPT = "-h";
        private const string SEPARATOR_OPT = "--separator";
        private const string SHORT_SEPARATOR_OPT = "-s";
        private const string VERSION_OPT = "--version";
        private const string SHORT_VERSION_OPT = "-v";
        private const string DEFAULT_SEPARATOR = ";";
        private static readonly List<string> MODEL_LIST = new List<string>() { "XP2023", "BTG2023" };
        private static readonly string NL = Environment.NewLine;
        private static readonly string VERSION = "0.0.1";
        private static readonly string VERSION_MSG = $"notatocsv version {VERSION}";

        public static void Main(string[] args)
        {
            List<string> optNameList;
            Dictionary<string, string> optAltDict, optHelpDict, optMissingMsgDict;
            ConfigOptions(out optNameList, out optAltDict, out optHelpDict, out optMissingMsgDict);

            try
            {
                var argValueDict = new Dictionary<string, string>();

                bool keepGoing = HandleArgs(args, optNameList, optAltDict, optHelpDict, optMissingMsgDict, argValueDict);

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
                    HandleModelOption(optNameList, optAltDict, optHelpDict, optMissingMsgDict, MODEL_LIST, argValueDict);
                }

                System.Environment.Exit(0);
            }
            catch (ArgumentException ex)
            {
                Console.Error.WriteLine($"ERRO: {ex.Message}");
                ShowHelpText(optNameList, optAltDict, optHelpDict);
                System.Environment.Exit(1);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"ERRO: {ex.Message}");
                ShowHelpText(optNameList, optAltDict, optHelpDict);
                System.Environment.Exit(1);
            }
        }
        private static bool HandleArgs(
            string[] args, List<string> optNameList, Dictionary<string, string> optAltDict,
            Dictionary<string, string> optHelpDict, Dictionary<string, string> optMissingMsgDict,
            Dictionary<string, string> argValueDict)
        {
            var argList = args.ToList();
            if (argList.Count == 0)
            {
                throw new ArgumentException("Falta informar opções nos argumentos!");
            }

            var argReadCount = 0;
            var jumpArg = 0;
            foreach (var arg in argList)
            {
                argReadCount++;
                switch (arg)
                {
                    case MODEL_OPT:
                    case SHORT_MODEL_OPT:
                        GetOptionValue(MODEL_OPT, argList, argReadCount, optMissingMsgDict, argValueDict);
                        jumpArg = 1;
                        continue;
                    case FILE_OPT:
                    case SHORT_FILE_OPT:
                        GetOptionValue(FILE_OPT, argList, argReadCount, optMissingMsgDict, argValueDict);
                        jumpArg = 1;
                        continue;
                    case PREPROCESS_OPT:
                    case SHORT_PREPROCESS_OPT:
                        GetOptionValue(PREPROCESS_OPT, argList, argReadCount, optMissingMsgDict, argValueDict);
                        jumpArg = 1;
                        return true;
                    case HELP_OPT:
                    case SHORT_HELP_OPT:
                        ShowHelpText(optNameList, optAltDict, optHelpDict);
                        return false;
                    case SEPARATOR_OPT:
                    case SHORT_SEPARATOR_OPT:
                        GetOptionValue(SEPARATOR_OPT, argList, argReadCount, optMissingMsgDict, argValueDict);
                        jumpArg = 1;
                        continue;
                    case VERSION_OPT:
                    case SHORT_VERSION_OPT:
                        Console.Out.WriteLine(VERSION_MSG);
                        return false;
                    default:
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

        private static void HandleModelOption(
            List<string> optNameList, Dictionary<string, string> optAltDict,
            Dictionary<string, string> optHelpDict, Dictionary<string, string> optMissingMsgDict,
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
                throw new ArgumentException($"{optMissingMsgDict[FILE_OPT]}.");
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
                ExtractBTG2023(filePath);
            }
            else
            {
                throw new ArgumentException($"Modelo '{model}' não é reconhecido.");
            }

            if (dadosNota == null || dadosNota.Operacoes == null || dadosNota.Custos == null)
            {
                throw new Exception("Dados não encontrados após processar!");
            }
            RateioDespesa.Calcular(dadosNota);
            var separator = argValueDict[SEPARATOR_OPT];

            Console.WriteLine($"Data da Nota{separator}Numero da Nota{separator}Codigo do Ativo{separator}" +
                $"Quantidade do Ativo{separator}Valor da Operacao{separator}Custo da Operacao");

            var Br = new System.Globalization.CultureInfo("pt-BR");

            dadosNota.Custos.ForEach(c =>
            {
                dadosNota.Operacoes.Where(o => o.NumeroNota == c.NumeroNota).OrderBy(o => o.ValorOperacao).ToList().ForEach(o =>
                {
                    Console.WriteLine($"{o.DataNota}{separator}{o.NumeroNota}{separator}{o.CodigoAtivo}{separator}" +
                    $"{o.Quantidade}{separator}{o.ValorOperacao.ToString(Br)}{separator}{o.CustoOperacao.ToString(Br)}");
                });
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
                "Opções:" + NL;
            foreach (var arg in argNameList)
            {
                var option = argAltDict.ContainsKey(arg) ? $"{argAltDict[arg]}, {arg}" : $"{arg}";
                option = option.PadRight(20);
                txtHelp += $"{option}: {argHelpDict[arg]}" + NL;
            }
            txtHelp += NL +
                "Exemplos:" + NL +
                "\tnotatocsv --model XP2023 --file nota.html" + NL +
                "\tnotatocsv -m BTG2023 --file nota.pdf > nota.csv" + NL +
                "\tnotatocsv -m XP2023 -f nota.pdf -s \"|\"" + NL +
                "\tnotatocsv --pre-process nota.pdf" + NL +
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

        private static void GetOptionValue(
            string option, IList<string> argList, int argReadCount,
            IDictionary<string, string> optMissingMsgDict,
            IDictionary<string, string> argValueDict)
        {
            var copy = new List<string>(argList);
            var modelValue = copy.Skip(argReadCount).First();
            if (string.IsNullOrEmpty(modelValue))
            {
                throw new ArgumentException($"{optMissingMsgDict[option]}");
            }
            argValueDict.Add(option, modelValue);
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
            argHelpDict.Add(MODEL_OPT, $"O modelo da nota de corretagem. Pode ser: {string.Join(", ", MODEL_LIST)}");
            argMissingDict.Add(MODEL_OPT, "O modelo da nota não foi informado.");

            argNameList.Add(FILE_OPT);
            argAltDict.Add(FILE_OPT, SHORT_FILE_OPT);
            argHelpDict.Add(FILE_OPT, "O arquivo da nota de corretagem no formato HTML 'bbox'.");
            argMissingDict.Add(FILE_OPT, "O arquivo da nota não foi informado.");

            argNameList.Add(PREPROCESS_OPT);
            argAltDict.Add(PREPROCESS_OPT, SHORT_PREPROCESS_OPT);
            argHelpDict.Add(PREPROCESS_OPT, "Pré-processa o PDF da nota de corretagem para HTML 'bbox'.");
            argMissingDict.Add(PREPROCESS_OPT, "O arquivo PDF da nota não foi informado.");

            argNameList.Add(HELP_OPT);
            argAltDict.Add(HELP_OPT, SHORT_HELP_OPT);
            argHelpDict.Add(HELP_OPT, "Exibe esta mensagem de ajuda.");

            argNameList.Add(SEPARATOR_OPT);
            argAltDict.Add(SEPARATOR_OPT, SHORT_SEPARATOR_OPT);
            argHelpDict.Add(SEPARATOR_OPT, "O separador de campos do CSV. O padrão é ';'.");
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

        public static void ExtractBTG2023(string nomeArquivo)
        {
            var doc = new System.Xml.XmlDocument();
            doc.Load(nomeArquivo);
            var paginas = PdfToHtmlReader.Read(doc);
            foreach (var pagina in paginas)
            {
                var dados = BtgBolsa2023.Extrai(pagina).ToList();
                foreach (var dado in dados)
                {
                    var t = dado.Transacao;
                    if (t != null)
                        System.Console.WriteLine($"t,{t.DataPregao};{t.NumeroNota};{t.Folha};{t.Operacao};" +
                            $"{t.Titulo};{t.Quantidade};{t.Preco};{t.Valor}");
                    var c = dado.Custo;
                    if (c != null)
                        System.Console.WriteLine($"c;{c.DataPregao};{c.NumeroNota};{c.Folha};" +
                            $"{c.CustoTaxaLiquidacao};{c.CustoTaxaRegistro};{c.CustoTotalBolsa};" +
                            $"{c.CustoTotalCorretora};{c.IrrfSobreOperacoes}");
                }
            }
        }
    }
}