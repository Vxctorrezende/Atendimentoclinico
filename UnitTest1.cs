using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Atendimento_Clinico
{
    // ==================== STRUCTS ====================
    /// <summary>
    /// Struct para representar a prioridade de um chamado (RF05)
    /// </summary>
    public struct Prioridade
    {
        public int Nivel { get; set; }
        public string Descricao { get; set; }

        public Prioridade(int nivel, string descricao)
        {
            Nivel = nivel;
            Descricao = descricao;
        }

        public override string ToString()
        {
            return $"{Nivel} - {Descricao}";
        }
    }

    // ==================== ENUMS ====================
    /// <summary>
    /// Enum para representar os status possíveis de um chamado
    /// </summary>
    public enum StatusChamado
    {
        ABERTO,
        EM_ATENDIMENTO,
        RESOLVIDO,
        CANCELADO
    }

    // ==================== CLASSES DE DOMÍNIO ====================
    /// <summary>
    /// Classe para representar um cliente (RF01)
    /// </summary>
    public class Cliente
    {
        public int Codigo { get; set; }
        public string Nome { get; set; }
        public string Telefone { get; set; }
        public string Email { get; set; }

        public Cliente() { }

        public Cliente(int codigo, string nome, string telefone, string email)
        {
            Codigo = codigo;
            Nome = nome;
            Telefone = telefone;
            Email = email;
        }

        public override string ToString()
        {
            return $"Código: {Codigo} | Nome: {Nome} | Tel: {Telefone} | Email: {Email}";
        }

        public string ToFileFormat()
        {
            return $"{Codigo}|{Nome}|{Telefone}|{Email}";
        }

        public static Cliente FromFileFormat(string linha)
        {
            var partes = linha.Split('|');
            if (partes.Length == 4)
            {
                return new Cliente(
                    int.Parse(partes[0]),
                    partes[1],
                    partes[2],
                    partes[3]
                );
            }
            return null;
        }
    }

    /// <summary>
    /// Classe para representar um chamado (RF02)
    /// </summary>
    public class Chamado
    {
        public int Numero { get; set; }
        public int CodigoCliente { get; set; }
        public string Descricao { get; set; }
        public Prioridade Prioridade { get; set; }
        public StatusChamado Status { get; set; }
        public DateTime DataCriacao { get; set; }

        public Chamado() { }

        public Chamado(int numero, int codigoCliente, string descricao, Prioridade prioridade)
        {
            Numero = numero;
            CodigoCliente = codigoCliente;
            Descricao = descricao;
            Prioridade = prioridade;
            Status = StatusChamado.ABERTO;
            DataCriacao = DateTime.Now;
        }

        public override string ToString()
        {
            return $"Chamado #{Numero} | Cliente: {CodigoCliente} | Status: {Status} | Prioridade: {Prioridade} | {Descricao}";
        }

        public string ToFileFormat()
        {
            return $"{Numero}|{CodigoCliente}|{Descricao}|{Prioridade.Nivel}|{Prioridade.Descricao}|{(int)Status}|{DataCriacao:yyyy-MM-dd HH:mm:ss}";
        }

        public static Chamado FromFileFormat(string linha)
        {
            var partes = linha.Split('|');
            if (partes.Length == 7)
            {
                var chamado = new Chamado(
                    int.Parse(partes[0]),
                    int.Parse(partes[1]),
                    partes[2],
                    new Prioridade(int.Parse(partes[3]), partes[4])
                );
                chamado.Status = (StatusChamado)int.Parse(partes[5]);
                chamado.DataCriacao = DateTime.Parse(partes[6]);
                return chamado;
            }
            return null;
        }
    }

    /// <summary>
    /// Classe para representar uma operação realizada no sistema (RF06)
    /// </summary>
    public class Operacao
    {
        public string Descricao { get; set; }
        public DateTime Data { get; set; }
        public string Tipo { get; set; }

        public Operacao(string descricao, string tipo)
        {
            Descricao = descricao;
            Tipo = tipo;
            Data = DateTime.Now;
        }

        public override string ToString()
        {
            return $"[{Data:yyyy-MM-dd HH:mm:ss}] {Tipo}: {Descricao}";
        }

        public string ToFileFormat()
        {
            return $"{Data:yyyy-MM-dd HH:mm:ss}|{Tipo}|{Descricao}";
        }

        public static Operacao FromFileFormat(string linha)
        {
            var partes = linha.Split('|');
            if (partes.Length >= 3)
            {
                var op = new Operacao(partes[2], partes[1]);
                op.Data = DateTime.Parse(partes[0]);
                return op;
            }
            return null;
        }
    }

    // ==================== CAMADA DE PERSISTÊNCIA ====================
    /// <summary>
    /// Classe responsável pela persistência de dados em arquivos (PERSISTÊNCIA)
    /// </summary>
    public class PersistenciaDados
    {
        private readonly string _caminhoClientes;
        private readonly string _caminhoChamados;
        private readonly string _caminhoOperacoes;
        private readonly string _diretorioDados;

        public PersistenciaDados(string diretorioDados = "dados")
        {
            _diretorioDados = diretorioDados;

            if (!Directory.Exists(_diretorioDados))
            {
                Directory.CreateDirectory(_diretorioDados);
            }

            _caminhoClientes = Path.Combine(_diretorioDados, "clientes.txt");
            _caminhoChamados = Path.Combine(_diretorioDados, "chamados.txt");
            _caminhoOperacoes = Path.Combine(_diretorioDados, "operacoes.txt");
        }

        // ========== Operações com Clientes ==========
        public void SalvarClientes(List<Cliente> clientes)
        {
            try
            {
                var linhas = clientes.Select(c => c.ToFileFormat()).ToList();
                File.WriteAllLines(_caminhoClientes, linhas);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao salvar clientes: {ex.Message}");
            }
        }

        public List<Cliente> CarregarClientes()
        {
            var clientes = new List<Cliente>();
            try
            {
                if (File.Exists(_caminhoClientes))
                {
                    var linhas = File.ReadAllLines(_caminhoClientes);
                    foreach (var linha in linhas)
                    {
                        if (!string.IsNullOrWhiteSpace(linha))
                        {
                            var cliente = Cliente.FromFileFormat(linha);
                            if (cliente != null)
                                clientes.Add(cliente);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao carregar clientes: {ex.Message}");
            }
            return clientes;
        }

        // ========== Operações com Chamados ==========
        public void SalvarChamados(List<Chamado> chamados)
        {
            try
            {
                var linhas = chamados.Select(c => c.ToFileFormat()).ToList();
                File.WriteAllLines(_caminhoChamados, linhas);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao salvar chamados: {ex.Message}");
            }
        }

        public List<Chamado> CarregarChamados()
        {
            var chamados = new List<Chamado>();
            try
            {
                if (File.Exists(_caminhoChamados))
                {
                    var linhas = File.ReadAllLines(_caminhoChamados);
                    foreach (var linha in linhas)
                    {
                        if (!string.IsNullOrWhiteSpace(linha))
                        {
                            var chamado = Chamado.FromFileFormat(linha);
                            if (chamado != null)
                                chamados.Add(chamado);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao carregar chamados: {ex.Message}");
            }
            return chamados;
        }

        // ========== Operações com Operações ==========
        public void SalvarOperacoes(Stack<Operacao> operacoes)
        {
            try
            {
                var lista = operacoes.Reverse().ToList();
                var linhas = lista.Select(o => o.ToFileFormat()).ToList();
                File.WriteAllLines(_caminhoOperacoes, linhas);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao salvar operações: {ex.Message}");
            }
        }

        public Stack<Operacao> CarregarOperacoes()
        {
            var operacoes = new Stack<Operacao>();
            try
            {
                if (File.Exists(_caminhoOperacoes))
                {
                    var linhas = File.ReadAllLines(_caminhoOperacoes);
                    for (int i = linhas.Length - 1; i >= 0; i--)
                    {
                        if (!string.IsNullOrWhiteSpace(linhas[i]))
                        {
                            var op = Operacao.FromFileFormat(linhas[i]);
                            if (op != null)
                                operacoes.Push(op);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao carregar operações: {ex.Message}");
            }
            return operacoes;
        }
    }

    // ==================== SISTEMA MONOLÍTICO ====================
    /// <summary>
    /// Classe principal do sistema de atendimento (Arquitetura Monolítica)
    /// Integra todas as funcionalidades em uma única classe
    /// </summary>
    public class SistemaAtendimento
    {
        // Estruturas de dados
        private List<Cliente> clientes;
        private List<Chamado> chamados;
        private Queue<Chamado> filaChamados; // RF03 - Fila de atendimento
        private Stack<Operacao> pilhaOperacoes; // RF06 - Pilha de operações
        private PersistenciaDados persistencia;

        // Contadores
        private int proximoCodigoCliente = 1;
        private int proximoNumeroChamado = 1;

        public SistemaAtendimento(string diretorioDados = "dados")
        {
            clientes = new List<Cliente>();
            chamados = new List<Chamado>();
            filaChamados = new Queue<Chamado>();
            pilhaOperacoes = new Stack<Operacao>();
            persistencia = new PersistenciaDados(diretorioDados);

            CarregarDados();
            AtualizarContadores();
        }

        // ==================== RF01 - CADASTRAR CLIENTE ====================
        public bool CadastrarCliente(string nome, string telefone, string email)
        {
            if (string.IsNullOrWhiteSpace(nome) || string.IsNullOrWhiteSpace(telefone) || string.IsNullOrWhiteSpace(email))
            {
                var op = new Operacao("Falha ao cadastrar cliente - dados inválidos", "ERRO");
                pilhaOperacoes.Push(op);
                return false;
            }

            var cliente = new Cliente(proximoCodigoCliente++, nome, telefone, email);
            clientes.Add(cliente);

            var operacao = new Operacao($"Cliente '{nome}' cadastrado com código {cliente.Codigo}", "CADASTRO_CLIENTE");
            pilhaOperacoes.Push(operacao);

            SalvarDados();
            return true;
        }

        public Cliente ConsultarCliente(int codigoCliente)
        {
            return clientes.FirstOrDefault(c => c.Codigo == codigoCliente);
        }

        public List<Cliente> ListarClientes()
        {
            return new List<Cliente>(clientes);
        }

        // ==================== RF02 - CADASTRAR CHAMADO ====================
        public bool CadastrarChamado(int codigoCliente, string descricao, int nivelPrioridade, string descricaoPrioridade)
        {
            var cliente = ConsultarCliente(codigoCliente);
            if (cliente == null)
            {
                var op = new Operacao($"Falha ao cadastrar chamado - cliente {codigoCliente} não encontrado", "ERRO");
                pilhaOperacoes.Push(op);
                return false;
            }

            if (string.IsNullOrWhiteSpace(descricao))
            {
                var op = new Operacao("Falha ao cadastrar chamado - descrição vazia", "ERRO");
                pilhaOperacoes.Push(op);
                return false;
            }

            var prioridade = new Prioridade(nivelPrioridade, descricaoPrioridade);
            var chamado = new Chamado(proximoNumeroChamado++, codigoCliente, descricao, prioridade);

            chamados.Add(chamado);
            filaChamados.Enqueue(chamado); // RF03 - Adicionar à fila

            var operacao = new Operacao($"Chamado #{chamado.Numero} cadastrado para cliente {cliente.Nome}", "CADASTRO_CHAMADO");
            pilhaOperacoes.Push(operacao);

            SalvarDados();
            return true;
        }

        // ==================== RF03 - FILA DE ATENDIMENTO ====================
        public Chamado ProximoChamadoFila()
        {
            if (filaChamados.Count > 0)
            {
                var chamado = filaChamados.Peek();
                var operacao = new Operacao($"Chamado #{chamado.Numero} consultado na fila", "CONSULTA_FILA");
                pilhaOperacoes.Push(operacao);
                return chamado;
            }
            return null;
        }

        public Chamado DequeueChamado()
        {
            if (filaChamados.Count > 0)
            {
                var chamado = filaChamados.Dequeue();
                var operacao = new Operacao($"Chamado #{chamado.Numero} removido da fila", "DEQUEUE_CHAMADO");
                pilhaOperacoes.Push(operacao);
                SalvarDados();
                return chamado;
            }
            return null;
        }

        public int TamanhoFila()
        {
            return filaChamados.Count;
        }

        // ==================== RF04 - LISTA DE CHAMADOS ====================
        public Chamado ConsultarChamado(int numeroChamado)
        {
            return chamados.FirstOrDefault(c => c.Numero == numeroChamado);
        }

        public List<Chamado> ListarChamados()
        {
            return new List<Chamado>(chamados);
        }

        public List<Chamado> ListarChamadosCliente(int codigoCliente)
        {
            return chamados.Where(c => c.CodigoCliente == codigoCliente).ToList();
        }

        public List<Chamado> ListarChamadosPorStatus(StatusChamado status)
        {
            return chamados.Where(c => c.Status == status).ToList();
        }

        // ==================== ALTERAR STATUS ====================
        public bool AlterarStatusChamado(int numeroChamado, StatusChamado novoStatus)
        {
            var chamado = ConsultarChamado(numeroChamado);
            if (chamado == null)
            {
                var op = new Operacao($"Falha ao alterar chamado - chamado #{numeroChamado} não encontrado", "ERRO");
                pilhaOperacoes.Push(op);
                return false;
            }

            var statusAnterior = chamado.Status;
            chamado.Status = novoStatus;

            var operacao = new Operacao(
                $"Chamado #{numeroChamado} status alterado de {statusAnterior} para {novoStatus}",
                "ALTERACAO_STATUS"
            );
            pilhaOperacoes.Push(operacao);

            SalvarDados();
            return true;
        }

        // ==================== RF06 - PILHA DE OPERAÇÕES ====================
        public List<Operacao> ListarOperacoes()
        {
            return new List<Operacao>(pilhaOperacoes);
        }

        public List<Operacao> ListarUltimasOperacoes(int quantidade)
        {
            return pilhaOperacoes.Take(quantidade).ToList();
        }

        public int TamanhoPilhaOperacoes()
        {
            return pilhaOperacoes.Count;
        }

        // ==================== PERSISTÊNCIA ====================
        public void SalvarDados()
        {
            persistencia.SalvarClientes(clientes);
            persistencia.SalvarChamados(chamados);
            persistencia.SalvarOperacoes(pilhaOperacoes);
        }

        private void CarregarDados()
        {
            clientes = persistencia.CarregarClientes();
            chamados = persistencia.CarregarChamados();
            pilhaOperacoes = persistencia.CarregarOperacoes();

            // Reconstruir fila de chamados abertos
            foreach (var chamado in chamados.Where(c => c.Status == StatusChamado.ABERTO).OrderBy(c => c.DataCriacao))
            {
                filaChamados.Enqueue(chamado);
            }
        }

        private void AtualizarContadores()
        {
            proximoCodigoCliente = clientes.Any() ? clientes.Max(c => c.Codigo) + 1 : 1;
            proximoNumeroChamado = chamados.Any() ? chamados.Max(c => c.Numero) + 1 : 1;
        }

        // ==================== RELATÓRIOS ====================
        public void ExibirRelatorioChamados()
        {
            Console.WriteLine("\n========== RELATÓRIO DE CHAMADOS ==========");
            if (chamados.Count == 0)
            {
                Console.WriteLine("Nenhum chamado cadastrado.");
                return;
            }

            foreach (var chamado in chamados)
            {
                var cliente = ConsultarCliente(chamado.CodigoCliente);
                Console.WriteLine($"{chamado}");
                if (cliente != null)
                    Console.WriteLine($"  └─ Cliente: {cliente.Nome}");
            }
            Console.WriteLine("==========================================\n");
        }

        public void ExibirRelatorioClientes()
        {
            Console.WriteLine("\n========== RELATÓRIO DE CLIENTES ==========");
            if (clientes.Count == 0)
            {
                Console.WriteLine("Nenhum cliente cadastrado.");
                return;
            }

            foreach (var cliente in clientes)
            {
                Console.WriteLine(cliente);
            }
            Console.WriteLine("==========================================\n");
        }

        public void ExibirRelatorioOperacoes()
        {
            Console.WriteLine("\n========== RELATÓRIO DE OPERAÇÕES ==========");
            if (pilhaOperacoes.Count == 0)
            {
                Console.WriteLine("Nenhuma operação registrada.");
                return;
            }

            int contador = 1;
            foreach (var op in pilhaOperacoes.Take(10))
            {
                Console.WriteLine($"{contador}. {op}");
                contador++;
            }
            Console.WriteLine("===========================================\n");
        }

        public void ExibirStatusFila()
        {
            Console.WriteLine($"\n========== STATUS DA FILA ==========");
            Console.WriteLine($"Chamados na fila: {TamanhoFila()}");

            if (filaChamados.Count > 0)
            {
                Console.WriteLine("\nPróximos chamados:");
                int contador = 1;
                foreach (var chamado in filaChamados.Take(5))
                {
                    Console.WriteLine($"{contador}. Chamado #{chamado.Numero} - Prioridade: {chamado.Prioridade}");
                    contador++;
                }
            }
            Console.WriteLine("====================================\n");
        }
    }

    // ==================== TESTES UNITÁRIOS ====================
    public class UnitTest1
    {
        /// <summary>
        /// Método helper para gerar diretório único para cada teste
        /// </summary>
        private string CriarDiretorioTeste(string prefixo = "test")
        {
            return $"{prefixo}_{Guid.NewGuid().ToString().Substring(0, 8)}";
        }

        // ==================== TESTES 3.4 - TESTES COM [FACT] ====================

        /// <summary>
        /// T01: Cadastrar um cliente corretamente
        /// </summary>
        [Fact]
        public void T01_DeveCadastrarClienteCorretamente()
        {
            // Arrange
            string dirTeste = $"dados_t01_{Guid.NewGuid()}";
            var sistema = new SistemaAtendimento(dirTeste);
            string nome = "João Pedro Silva";
            string telefone = "11987654321";
            string email = "joao.silva@email.com";

            // Act
            bool resultado = sistema.CadastrarCliente(nome, telefone, email);
            var cliente = sistema.ConsultarCliente(1);

            // Assert
            Assert.True(resultado);
            Assert.NotNull(cliente);
            Assert.Equal(nome, cliente.Nome);
            Assert.Equal(telefone, cliente.Telefone);
            Assert.Equal(email, cliente.Email);
        }

        /// <summary>
        /// T02: Cadastrar um chamado
        /// </summary>
        [Fact]
        public void T02_DeveCadastrarChamado()
        {
            // Arrange
            var sistema = new SistemaAtendimento("dados_t02");
            sistema.CadastrarCliente("Maria Santos", "11988888888", "maria@email.com");
            string descricao = "Sistema fora do ar";
            int nivelPrioridade = 1;
            string descricaoPrioridade = "Crítica";

            // Act
            bool resultado = sistema.CadastrarChamado(1, descricao, nivelPrioridade, descricaoPrioridade);
            var chamado = sistema.ConsultarChamado(1);

            // Assert
            Assert.True(resultado);
            Assert.NotNull(chamado);
            Assert.Equal(descricao, chamado.Descricao);
            Assert.Equal(nivelPrioridade, chamado.Prioridade.Nivel);
            Assert.Equal(descricaoPrioridade, chamado.Prioridade.Descricao);
            Assert.Equal(StatusChamado.ABERTO, chamado.Status);
        }

        /// <summary>
        /// T03: Adicionar chamado à fila
        /// </summary>
        [Fact]
        public void T03_DeveAdicionarChamadoNaFila()
        {
            // Arrange
            var sistema = new SistemaAtendimento("dados_t03");
            sistema.CadastrarCliente("Carlos Costa", "11986666666", "carlos@email.com");
            sistema.CadastrarChamado(1, "Erro ao fazer login", 2, "Alta");

            // Act
            int tamanhoFilaAntes = sistema.TamanhoFila();
            sistema.CadastrarChamado(1, "Problema com dados", 3, "Média");
            int tamanhoFilaDepois = sistema.TamanhoFila();

            // Assert
            Assert.Equal(1, tamanhoFilaAntes);
            Assert.Equal(2, tamanhoFilaDepois);
        }

        /// <summary>
        /// T04: Retirar o primeiro chamado da fila
        /// </summary>
        [Fact]
        public void T04_DeveRetirarPrimeiroChamadoDaFila()
        {
            // Arrange
            var sistema = new SistemaAtendimento("dados_t04");
            sistema.CadastrarCliente("Ana Paula", "11985555555", "ana@email.com");
            sistema.CadastrarChamado(1, "Primeira chamada", 2, "Alta");
            sistema.CadastrarChamado(1, "Segunda chamada", 3, "Média");
            int tamanhoFilaAntes = sistema.TamanhoFila();

            // Act
            var chamadoRemovido = sistema.DequeueChamado();
            int tamanhoFilaDepois = sistema.TamanhoFila();

            // Assert
            Assert.NotNull(chamadoRemovido);
            Assert.Equal(1, chamadoRemovido.Numero);
            Assert.Equal(2, tamanhoFilaAntes);
            Assert.Equal(1, tamanhoFilaDepois);
        }

        /// <summary>
        /// T05: Adicionar chamado à lista
        /// </summary>
        [Fact]
        public void T05_DeveAdicionarChamadoNaLista()
        {
            // Arrange
            var sistema = new SistemaAtendimento("dados_t05");
            sistema.CadastrarCliente("Bruno Ferreira", "11984444444", "bruno@email.com");
            int tamanhoListaAntes = sistema.ListarChamados().Count;

            // Act
            sistema.CadastrarChamado(1, "Teste chamado", 2, "Alta");
            var chamados = sistema.ListarChamados();
            int tamanhoListaDepois = chamados.Count;

            // Assert
            Assert.Equal(0, tamanhoListaAntes);
            Assert.Equal(1, tamanhoListaDepois);
            Assert.Contains(chamados, c => c.Numero == 1);
        }

        /// <summary>
        /// T06: Pesquisar chamado existente
        /// </summary>
        [Fact]
        public void T06_DevePesquisarChamadoExistente()
        {
            // Arrange
            var sistema = new SistemaAtendimento("dados_t06");
            sistema.CadastrarCliente("Diana Marques", "11983333333", "diana@email.com");
            sistema.CadastrarChamado(1, "Chamado para pesquisa", 2, "Alta");

            // Act
            var chamado = sistema.ConsultarChamado(1);

            // Assert
            Assert.NotNull(chamado);
            Assert.Equal(1, chamado.Numero);
            Assert.Equal("Chamado para pesquisa", chamado.Descricao);
            Assert.Equal(1, chamado.CodigoCliente);
        }

        /// <summary>
        /// T07: Alterar o status de um chamado
        /// </summary>
        [Fact]
        public void T07_DeveAlterarStatusChamado()
        {
            // Arrange
            var sistema = new SistemaAtendimento("dados_t07");
            sistema.CadastrarCliente("Eduardo Silva", "11982222222", "eduardo@email.com");
            sistema.CadastrarChamado(1, "Chamado com status", 2, "Alta");
            var statusAnterior = sistema.ConsultarChamado(1).Status;

            // Act
            bool resultado = sistema.AlterarStatusChamado(1, StatusChamado.EM_ATENDIMENTO);
            var statusNovo = sistema.ConsultarChamado(1).Status;

            // Assert
            Assert.True(resultado);
            Assert.Equal(StatusChamado.ABERTO, statusAnterior);
            Assert.Equal(StatusChamado.EM_ATENDIMENTO, statusNovo);
        }

        /// <summary>
        /// T08: Registrar uma operação na pilha
        /// </summary>
        [Fact]
        public void T08_DeveRegistrarOperacaoNaPilha()
        {
            // Arrange
            var sistema = new SistemaAtendimento("dados_t08");
            int operacoesAntes = sistema.TamanhoPilhaOperacoes();

            // Act
            sistema.CadastrarCliente("Fernanda Lima", "11981111111", "fernanda@email.com");
            int operacoesDepois = sistema.TamanhoPilhaOperacoes();

            // Assert
            Assert.Equal(0, operacoesAntes);
            Assert.Equal(1, operacoesDepois);
        }

        /// <summary>
        /// T09: Recuperar a última operação da pilha
        /// </summary>
        [Fact]
        public void T09_DeveRecuperarUltimaOperacaoDaPilha()
        {
            // Arrange
            var sistema = new SistemaAtendimento("dados_t09");
            sistema.CadastrarCliente("Gabriel Costa", "11980000000", "gabriel@email.com");
            sistema.CadastrarChamado(1, "Teste operação", 2, "Alta");

            // Act
            var operacoes = sistema.ListarOperacoes();
            var ultimaOperacao = operacoes.FirstOrDefault();

            // Assert
            Assert.NotEmpty(operacoes);
            Assert.NotNull(ultimaOperacao);
            Assert.Contains("Chamado", ultimaOperacao.Descricao);
            Assert.Equal("CADASTRO_CHAMADO", ultimaOperacao.Tipo);
        }

        /// <summary>
        /// T10: Salvar um chamado no arquivo
        /// </summary>
        [Fact]
        public void T10_DeveSalvarChamadoNoArquivo()
        {
            // Arrange
            string diretorioDados = "dados_t10";
            var sistema1 = new SistemaAtendimento(diretorioDados);
            sistema1.CadastrarCliente("Helena Santos", "11979999999", "helena@email.com");
            sistema1.CadastrarChamado(1, "Chamado a salvar", 2, "Alta");
            sistema1.SalvarDados();

            // Act
            var sistema2 = new SistemaAtendimento(diretorioDados);

            // Assert
            Assert.Single(sistema2.ListarChamados());
            var chamado = sistema2.ConsultarChamado(1);
            Assert.NotNull(chamado);
            Assert.Equal("Chamado a salvar", chamado.Descricao);
            Assert.Equal(1, chamado.Numero);
        }

        // ==================== TESTES 3.5 - TESTES COM [THEORY] ====================

        /// <summary>
        /// Teste com múltiplos cenários para cadastrar chamados com dados válidos
        /// </summary>
        [Theory]
        [InlineData(1, 1, "Problema crítico", "Crítica")]
        [InlineData(1, 2, "Problema alto", "Alta")]
        [InlineData(1, 3, "Problema médio", "Média")]
        [InlineData(1, 4, "Problema baixo", "Baixa")]
        [InlineData(1, 5, "Problema muito baixo", "Trivial")]
        public void DeveRegistrarChamadosComDiferentesPrioridades(int codigoCliente, int nivelPrioridade, string descricao, string descricaoPrioridade)
        {
            // Arrange
            string nomeArquivo = $"dados_theory_prioridade_{nivelPrioridade}";
            var sistema = new SistemaAtendimento(nomeArquivo);
            sistema.CadastrarCliente("Cliente Theory", "11977777777", "theory@email.com");

            // Act
            bool resultado = sistema.CadastrarChamado(codigoCliente, descricao, nivelPrioridade, descricaoPrioridade);
            var chamado = sistema.ConsultarChamado(1);

            // Assert
            Assert.True(resultado);
            Assert.NotNull(chamado);
            Assert.Equal(nivelPrioridade, chamado.Prioridade.Nivel);
            Assert.Equal(descricaoPrioridade, chamado.Prioridade.Descricao);
        }

        /// <summary>
        /// Teste com múltiplos cenários para alterar status de chamados
        /// </summary>
        [Theory]
        [InlineData(StatusChamado.EM_ATENDIMENTO)]
        [InlineData(StatusChamado.RESOLVIDO)]
        [InlineData(StatusChamado.CANCELADO)]
        [InlineData(StatusChamado.ABERTO)]
        public void DeveAlterarStatusChamadoParadiferentesStates(StatusChamado novoStatus)
        {
            // Arrange
            string nomeArquivo = $"dados_theory_status_{(int)novoStatus}";
            var sistema = new SistemaAtendimento(nomeArquivo);
            sistema.CadastrarCliente("Cliente Status", "11976666666", "status@email.com");
            sistema.CadastrarChamado(1, "Teste status", 2, "Alta");

            // Act
            bool resultado = sistema.AlterarStatusChamado(1, novoStatus);
            var chamado = sistema.ConsultarChamado(1);

            // Assert
            Assert.True(resultado);
            Assert.Equal(novoStatus, chamado.Status);
        }

        /// <summary>
        /// Teste com múltiplos cenários de quantidade de chamados em fila
        /// </summary>
        [Theory]
        [InlineData(1)]
        [InlineData(3)]
        [InlineData(5)]
        [InlineData(10)]
        [InlineData(20)]
        public void DeveAdicionarQuantidadeVariavelDeChamadosNaFila(int quantidadeChamados)
        {
            // Arrange
            string nomeArquivo = $"dados_theory_fila_{quantidadeChamados}";
            var sistema = new SistemaAtendimento(nomeArquivo);
            sistema.CadastrarCliente("Cliente Fila", "11975555555", "fila@email.com");

            // Act
            for (int i = 0; i < quantidadeChamados; i++)
            {
                sistema.CadastrarChamado(1, $"Chamado {i + 1}", 2, "Alta");
            }
            int tamanhoFila = sistema.TamanhoFila();

            // Assert
            Assert.Equal(quantidadeChamados, tamanhoFila);
        }

        /// <summary>
        /// Teste com múltiplos cenários de clientes e seus chamados
        /// </summary>
        [Theory]
        [InlineData(1, "Cliente Um")]
        [InlineData(2, "Cliente Dois")]
        [InlineData(3, "Cliente Três")]
        [InlineData(5, "Cliente Cinco")]
        [InlineData(10, "Cliente Dez")]
        public void DeveRegistrarMultiplosClientesComNomesDiferentes(int quantidade, string prefixoNome)
        {
            // Arrange
            string nomeArquivo = $"dados_theory_clientes_{quantidade}";
            var sistema = new SistemaAtendimento(nomeArquivo);

            // Act
            for (int i = 1; i <= quantidade; i++)
            {
                sistema.CadastrarCliente(
                    $"{prefixoNome} {i}",
                    $"1199{i:D8}",
                    $"cliente{i}@email.com"
                );
            }
            var clientes = sistema.ListarClientes();

            // Assert
            Assert.Equal(quantidade, clientes.Count);
            Assert.All(clientes, c => Assert.NotEmpty(c.Nome));
        }

        /// <summary>
        /// Teste com múltiplos cenários de descrições de chamados
        /// </summary>
        [Theory]
        [InlineData("Sistema fora do ar")]
        [InlineData("Erro ao fazer login")]
        [InlineData("Problema com banco de dados")]
        [InlineData("Lentidão no sistema")]
        [InlineData("Dúvida sobre funcionalidade")]
        public void DeveRegistrarChamadosComDescricoesDiferentes(string descricao)
        {
            // Arrange
            string nomeArquivo = $"dados_theory_descricao_{descricao.GetHashCode()}";
            var sistema = new SistemaAtendimento(nomeArquivo);
            sistema.CadastrarCliente("Cliente Desc", "11974444444", "desc@email.com");

            // Act
            bool resultado = sistema.CadastrarChamado(1, descricao, 2, "Alta");
            var chamado = sistema.ConsultarChamado(1);

            // Assert
            Assert.True(resultado);
            Assert.NotNull(chamado);
            Assert.Equal(descricao, chamado.Descricao);
        }

        // ==================== TESTES 3.6 - TESTES NEGATIVOS ====================

        /// <summary>
        /// Teste negativo: Cliente inexistente
        /// </summary>
        [Fact]
        public void NaoDeveConsultarClienteInexistente()
        {
            // Arrange
            var sistema = new SistemaAtendimento("dados_neg_cliente_inexistente");

            // Act
            var cliente = sistema.ConsultarCliente(999);

            // Assert
            Assert.Null(cliente);
        }

        /// <summary>
        /// Teste negativo: Chamado inexistente
        /// </summary>
        [Fact]
        public void NaoDeveConsultarChamadoInexistente()
        {
            // Arrange
            var sistema = new SistemaAtendimento("dados_neg_chamado_inexistente");
            sistema.CadastrarCliente("Cliente Teste", "11973333333", "teste@email.com");

            // Act
            var chamado = sistema.ConsultarChamado(999);

            // Assert
            Assert.Null(chamado);
        }

        /// <summary>
        /// Teste negativo: Fila vazia
        /// </summary>
        [Fact]
        public void NaoDeveRetornarChamadoDeFilaVazia()
        {
            // Arrange
            var sistema = new SistemaAtendimento("dados_neg_fila_vazia");

            // Act
            var chamado = sistema.ProximoChamadoFila();
            var chamadoRemovido = sistema.DequeueChamado();

            // Assert
            Assert.Null(chamado);
            Assert.Null(chamadoRemovido);
            Assert.Equal(0, sistema.TamanhoFila());
        }

        /// <summary>
        /// Teste negativo: Lista vazia
        /// </summary>
        [Fact]
        public void NaoDeveConterChamadosAoListarListaVazia()
        {
            // Arrange
            var sistema = new SistemaAtendimento("dados_neg_lista_vazia");

            // Act
            var chamados = sistema.ListarChamados();

            // Assert
            Assert.Empty(chamados);
            Assert.Equal(0, chamados.Count);
        }

        /// <summary>
        /// Teste negativo: Pilha vazia
        /// </summary>
        [Fact]
        public void NaoDeveConterOperacoesAoListarPilhaVazia()
        {
            // Arrange
            var sistema = new SistemaAtendimento("dados_neg_pilha_vazia_novo");

            // Act
            var operacoes = sistema.ListarOperacoes();

            // Assert
            Assert.Empty(operacoes);
            Assert.Equal(0, operacoes.Count);
            Assert.Equal(0, sistema.TamanhoPilhaOperacoes());
        }

        /// <summary>
        /// Teste negativo: Dados inválidos - Cliente com nome vazio
        /// </summary>
        [Fact]
        public void NaoDeveCadastrarClienteComNomeVazio()
        {
            // Arrange
            var sistema = new SistemaAtendimento("dados_neg_nome_vazio");

            // Act
            bool resultado = sistema.CadastrarCliente("", "11972222222", "vazio@email.com");

            // Assert
            Assert.False(resultado);
            Assert.Empty(sistema.ListarClientes());
        }

        /// <summary>
        /// Teste negativo: Dados inválidos - Cliente com telefone vazio
        /// </summary>
        [Fact]
        public void NaoDeveCadastrarClienteComTelefoneVazio()
        {
            // Arrange
            var sistema = new SistemaAtendimento("dados_neg_telefone_vazio");

            // Act
            bool resultado = sistema.CadastrarCliente("Nome Válido", "", "telefone@email.com");

            // Assert
            Assert.False(resultado);
            Assert.Empty(sistema.ListarClientes());
        }

        /// <summary>
        /// Teste negativo: Dados inválidos - Cliente com email vazio
        /// </summary>
        [Fact]
        public void NaoDeveCadastrarClienteComEmailVazio()
        {
            // Arrange
            var sistema = new SistemaAtendimento("dados_neg_email_vazio");

            // Act
            bool resultado = sistema.CadastrarCliente("Nome Válido", "11971111111", "");

            // Assert
            Assert.False(resultado);
            Assert.Empty(sistema.ListarClientes());
        }

        /// <summary>
        /// Teste negativo: Dados inválidos - Chamado com descrição vazia
        /// </summary>
        [Fact]
        public void NaoDeveCadastrarChamadoComDescricaoVazia()
        {
            // Arrange
            var sistema = new SistemaAtendimento("dados_neg_chamado_desc_vazia");
            sistema.CadastrarCliente("Cliente Válido", "11970000000", "valido@email.com");

            // Act
            bool resultado = sistema.CadastrarChamado(1, "", 2, "Alta");

            // Assert
            Assert.False(resultado);
            Assert.Empty(sistema.ListarChamados());
        }

        /// <summary>
        /// Teste negativo: Arquivo inexistente - Recuperar dados de arquivo que não existe
        /// </summary>
        [Fact]
        public void DeveCarregarSistemaVazioQuandoArquivosNaoExistem()
        {
            // Arrange
            string diretorioDados = "dados_neg_arquivo_inexistente_novo";
            if (Directory.Exists(diretorioDados))
            {
                Directory.Delete(diretorioDados, true);
            }

            // Act
            var sistema = new SistemaAtendimento(diretorioDados);
            var clientes = sistema.ListarClientes();
            var chamados = sistema.ListarChamados();

            // Assert
            Assert.Empty(clientes);
            Assert.Empty(chamados);
        }

        /// <summary>
        /// Teste negativo: Tentativa de alterar chamado inexistente
        /// </summary>
        [Fact]
        public void NaoDeveAlterarStatusChamadoInexistente()
        {
            // Arrange
            var sistema = new SistemaAtendimento("dados_neg_alterar_inexistente");
            sistema.CadastrarCliente("Cliente Teste", "11969999999", "teste@email.com");

            // Act
            bool resultado = sistema.AlterarStatusChamado(999, StatusChamado.EM_ATENDIMENTO);

            // Assert
            Assert.False(resultado);
        }

        /// <summary>
        /// Teste negativo: Cadastrar chamado para cliente inexistente
        /// </summary>
        [Fact]
        public void NaoDeveCadastrarChamadoParaClienteInexistente()
        {
            // Arrange
            var sistema = new SistemaAtendimento("dados_neg_chamado_cliente_inexistente");

            // Act
            bool resultado = sistema.CadastrarChamado(999, "Chamado inválido", 2, "Alta");

            // Assert
            Assert.False(resultado);
            Assert.Empty(sistema.ListarChamados());
            Assert.Equal(0, sistema.TamanhoFila());
        }

        /// <summary>
        /// Teste negativo: Listar chamados de cliente inexistente
        /// </summary>
        [Fact]
        public void DeveRetornarListaVaziaParaChamadosDeClienteInexistente()
        {
            // Arrange
            var sistema = new SistemaAtendimento("dados_neg_listar_chamados_cliente_inexistente");
            sistema.CadastrarCliente("Cliente 1", "11968888888", "cliente1@email.com");
            sistema.CadastrarChamado(1, "Chamado do cliente 1", 2, "Alta");

            // Act
            var chamadosCliente999 = sistema.ListarChamadosCliente(999);

            // Assert
            Assert.Empty(chamadosCliente999);
        }

        /// <summary>
        /// Teste negativo: Listar chamados de status que não existe
        /// </summary>
        [Fact]
        public void DeveRetornarListaVaziaAoBuscarChamadosComStatusSemChamados()
        {
            // Arrange
            var sistema = new SistemaAtendimento("dados_neg_status_sem_chamados");
            sistema.CadastrarCliente("Cliente Status", "11967777777", "status@email.com");
            sistema.CadastrarChamado(1, "Chamado aberto", 2, "Alta");

            // Act
            var chamadosResolvidos = sistema.ListarChamadosPorStatus(StatusChamado.RESOLVIDO);

            // Assert
            Assert.Empty(chamadosResolvidos);
        }

        /// <summary>
        /// Teste negativo: Dados inválidos - Nome com apenas espaços
        /// </summary>
        [Fact]
        public void NaoDeveCadastrarClienteComNomeApenasEspacos()
        {
            // Arrange
            var sistema = new SistemaAtendimento("dados_neg_nome_espacos");

            // Act
            bool resultado = sistema.CadastrarCliente("   ", "11966666666", "espacos@email.com");

            // Assert
            Assert.False(resultado);
            Assert.Empty(sistema.ListarClientes());
        }

        /// <summary>
        /// Teste negativo: Dados inválidos - Descrição com apenas espaços
        /// </summary>
        [Fact]
        public void NaoDeveCadastrarChamadoComDescricaoApenasEspacos()
        {
            // Arrange
            var sistema = new SistemaAtendimento("dados_neg_descricao_espacos");
            sistema.CadastrarCliente("Cliente Válido", "11965555555", "valido@email.com");

            // Act
            bool resultado = sistema.CadastrarChamado(1, "   ", 2, "Alta");

            // Assert
            Assert.False(resultado);
            Assert.Empty(sistema.ListarChamados());
        }

        /// <summary>
        /// Teste negativo: Procurar por últimas operações em pilha vazia
        /// </summary>
        [Fact]
        public void DeveRetornarListaVaziaAoSolicitarUltimasOperacoesDePilhaVazia()
        {
            // Arrange
            var sistema = new SistemaAtendimento("dados_neg_ultimas_operacoes_vazia");

            // Act
            var operacoes = sistema.ListarUltimasOperacoes(5);

            // Assert
            Assert.Empty(operacoes);
        }

        /// <summary>
        /// Teste negativo: Verificar persistência com arquivo corrompido
        /// </summary>
        [Fact]
        public void DeveCarregarSistemaVazioAoEncontrarArquivoComFormatoInvalido()
        {
            // Arrange
            string diretorioDados = "dados_neg_formato_invalido";
            if (!Directory.Exists(diretorioDados))
                Directory.CreateDirectory(diretorioDados);

            // Criar arquivo com conteúdo inválido
            string caminhoClientes = Path.Combine(diretorioDados, "clientes.txt");
            File.WriteAllText(caminhoClientes, "dados|invalidos|sem|estrutura|correta|demais");

            // Act
            var sistema = new SistemaAtendimento(diretorioDados);
            var clientes = sistema.ListarClientes();

            // Assert
            Assert.Empty(clientes);
        }

        /// <summary>
        /// Teste negativo: Tentar remover chamado de fila vazia múltiplas vezes
        /// </summary>
        [Fact]
        public void NaoDeveRemoverChamadoDeFilaVaziaAposMultiplasTentativas()
        {
            // Arrange
            var sistema = new SistemaAtendimento("dados_neg_dequeue_multiplas");

            // Act
            var primeiro = sistema.DequeueChamado();
            var segundo = sistema.DequeueChamado();
            var terceiro = sistema.DequeueChamado();

            // Assert
            Assert.Null(primeiro);
            Assert.Null(segundo);
            Assert.Null(terceiro);
        }
    }
}
