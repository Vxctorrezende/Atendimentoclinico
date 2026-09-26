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
        [Fact]
        public void TestCadastrarCliente()
        {
            // Arrange
            var sistema = new SistemaAtendimento("dados_teste_1");

            // Act
            bool resultado = sistema.CadastrarCliente("João Silva", "11999999999", "joao@email.com");

            // Assert
            Assert.True(resultado);
            Assert.Single(sistema.ListarClientes());
            Assert.Equal("João Silva", sistema.ListarClientes()[0].Nome);
        }

        [Fact]
        public void TestCadastrarChamado()
        {
            // Arrange
            var sistema = new SistemaAtendimento("dados_teste_2");
            sistema.CadastrarCliente("Maria Silva", "11988888888", "maria@email.com");

            // Act
            bool resultado = sistema.CadastrarChamado(1, "Problema de conexão", 2, "Alta");

            // Assert
            Assert.True(resultado);
            Assert.Single(sistema.ListarChamados());
            Assert.Equal("Problema de conexão", sistema.ListarChamados()[0].Descricao);
        }

        [Fact]
        public void TestFilaChamados()
        {
            // Arrange
            var sistema = new SistemaAtendimento("dados_teste_3");
            sistema.CadastrarCliente("Pedro Costa", "11987654321", "pedro@email.com");
            sistema.CadastrarChamado(1, "Erro ao logar", 1, "Crítica");
            sistema.CadastrarChamado(1, "Dúvida sobre sistema", 3, "Baixa");

            // Act
            var proximoChamado = sistema.ProximoChamadoFila();

            // Assert
            Assert.NotNull(proximoChamado);
            Assert.Equal(1, proximoChamado.Numero);
            Assert.Equal(2, sistema.TamanhoFila());
        }

        [Fact]
        public void TestAlterarStatusChamado()
        {
            // Arrange
            var sistema = new SistemaAtendimento("dados_teste_4");
            sistema.CadastrarCliente("Ana Santos", "11991111111", "ana@email.com");
            sistema.CadastrarChamado(1, "Suporte técnico", 2, "Alta");

            // Act
            bool resultado = sistema.AlterarStatusChamado(1, StatusChamado.EM_ATENDIMENTO);

            // Assert
            Assert.True(resultado);
            Assert.Equal(StatusChamado.EM_ATENDIMENTO, sistema.ConsultarChamado(1).Status);
        }

        [Fact]
        public void TestPilhaOperacoes()
        {
            // Arrange
            var sistema = new SistemaAtendimento("dados_teste_5");

            // Act
            sistema.CadastrarCliente("Lucas Oliveira", "11992222222", "lucas@email.com");
            var operacoes = sistema.ListarOperacoes();

            // Assert
            Assert.NotEmpty(operacoes);
            Assert.Contains("cadastrado", operacoes[0].Descricao);
        }

        [Fact]
        public void TestConsultarClienteInexistente()
        {
            // Arrange
            var sistema = new SistemaAtendimento("dados_teste_6");

            // Act
            var cliente = sistema.ConsultarCliente(999);

            // Assert
            Assert.Null(cliente);
        }

        [Fact]
        public void TestCadastrarChamadoClienteInexistente()
        {
            // Arrange
            var sistema = new SistemaAtendimento("dados_teste_7");

            // Act
            bool resultado = sistema.CadastrarChamado(999, "Teste", 1, "Normal");

            // Assert
            Assert.False(resultado);
        }

        [Fact]
        public void TestPersistenciaDados()
        {
            // Arrange
            var sistema1 = new SistemaAtendimento("dados_teste_8");
            sistema1.CadastrarCliente("Teste Persistência", "11999999999", "teste@email.com");
            sistema1.CadastrarChamado(1, "Teste chamado", 2, "Alta");
            sistema1.SalvarDados();

            // Act
            var sistema2 = new SistemaAtendimento("dados_teste_8");

            // Assert
            Assert.Single(sistema2.ListarClientes());
            Assert.Single(sistema2.ListarChamados());
            Assert.Equal("Teste Persistência", sistema2.ListarClientes()[0].Nome);
        }

        [Fact]
        public void TestListarChamadosPorStatus()
        {
            // Arrange
            var sistema = new SistemaAtendimento("dados_teste_9");
            sistema.CadastrarCliente("Cliente Teste", "11999999999", "teste@email.com");
            sistema.CadastrarChamado(1, "Chamado 1", 1, "Crítica");
            sistema.CadastrarChamado(1, "Chamado 2", 2, "Alta");
            sistema.AlterarStatusChamado(1, StatusChamado.EM_ATENDIMENTO);

            // Act
            var chamadosAbertos = sistema.ListarChamadosPorStatus(StatusChamado.ABERTO);
            var chamadosAtendimento = sistema.ListarChamadosPorStatus(StatusChamado.EM_ATENDIMENTO);

            // Assert
            Assert.Single(chamadosAbertos);
            Assert.Single(chamadosAtendimento);
        }

        [Fact]
        public void TestDequeueChamado()
        {
            // Arrange
            var sistema = new SistemaAtendimento("dados_teste_10");
            sistema.CadastrarCliente("Cliente Queue", "11999999999", "teste@email.com");
            sistema.CadastrarChamado(1, "Primeiro", 1, "Normal");
            sistema.CadastrarChamado(1, "Segundo", 2, "Alta");

            // Act
            var chamadoRemovido = sistema.DequeueChamado();
            int tamanhoAposFila = sistema.TamanhoFila();

            // Assert
            Assert.NotNull(chamadoRemovido);
            Assert.Equal(1, chamadoRemovido.Numero);
            Assert.Equal(1, tamanhoAposFila);
        }
    }
}
