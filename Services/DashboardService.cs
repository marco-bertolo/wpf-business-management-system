using MySql.Data.MySqlClient;
using PapJoaoSimoes.Models;
using System;
using System.Configuration;

namespace PapJoaoSimoes.Services
{
    public class DashboardService
    {
        private readonly string connectionString =
            ConfigurationManager.ConnectionStrings["PAPJoao"].ConnectionString;

        public DashboardResumo ObterResumo()
        {
            DashboardResumo resumo = new DashboardResumo
            {
                VendasRecentes = new System.Collections.Generic.List<Venda>(),
                ProdutosStockBaixo = new System.Collections.Generic.List<Produto>()
            };

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                CarregarMetricas(connection, resumo);
                CarregarVendasRecentes(connection, resumo);
                CarregarProdutosStockBaixo(connection, resumo);
            }

            return resumo;
        }

        private static void CarregarMetricas(MySqlConnection connection, DashboardResumo resumo)
        {
            const string sql = @"
                SELECT
                    (SELECT COUNT(*) FROM Cliente WHERE Ativo = 1) AS ClientesAtivos,
                    (SELECT COUNT(*) FROM Produto WHERE Ativo = 1) AS ProdutosAtivos,
                    (SELECT COUNT(*) FROM Venda) AS TotalVendas,
                    (SELECT COALESCE(SUM(Total), 0) FROM Venda) AS TotalVendido;";

            using (MySqlCommand command = new MySqlCommand(sql, connection))
            using (MySqlDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    resumo.ClientesAtivos = Convert.ToInt32(reader["ClientesAtivos"]);
                    resumo.ProdutosAtivos = Convert.ToInt32(reader["ProdutosAtivos"]);
                    resumo.TotalVendas = Convert.ToInt32(reader["TotalVendas"]);
                    resumo.TotalVendido = Convert.ToDecimal(reader["TotalVendido"]);
                }
            }
        }

        private static void CarregarVendasRecentes(MySqlConnection connection, DashboardResumo resumo)
        {
            const string sql = @"
                SELECT v.VendaId,
                       v.DataVenda,
                       v.ClienteId,
                       c.Nome AS ClienteNome,
                       v.FuncionarioId,
                       f.Nome AS FuncionarioNome,
                       v.Total
                FROM Venda v
                INNER JOIN Cliente c ON c.ClienteId = v.ClienteId
                INNER JOIN Funcionario f ON f.FuncionarioId = v.FuncionarioId
                ORDER BY v.DataVenda DESC, v.VendaId DESC
                LIMIT 5;";

            using (MySqlCommand command = new MySqlCommand(sql, connection))
            using (MySqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    resumo.VendasRecentes.Add(new Venda
                    {
                        VendaId = Convert.ToInt32(reader["VendaId"]),
                        DataVenda = Convert.ToDateTime(reader["DataVenda"]),
                        ClienteId = Convert.ToInt32(reader["ClienteId"]),
                        ClienteNome = Convert.ToString(reader["ClienteNome"]),
                        FuncionarioId = Convert.ToInt32(reader["FuncionarioId"]),
                        FuncionarioNome = Convert.ToString(reader["FuncionarioNome"]),
                        Total = Convert.ToDecimal(reader["Total"])
                    });
                }
            }
        }

        private static void CarregarProdutosStockBaixo(MySqlConnection connection, DashboardResumo resumo)
        {
            const string sql = @"
                SELECT p.ProdutoId,
                       p.Descricao,
                       p.StockAtual,
                       p.PrecoCompra,
                       p.PrecoVenda,
                       p.FornecedorId,
                       f.Nome AS FornecedorNome,
                       p.Ativo
                FROM Produto p
                INNER JOIN Fornecedor f ON f.FornecedorId = p.FornecedorId
                WHERE p.Ativo = 1
                ORDER BY p.StockAtual ASC, p.Descricao ASC
                LIMIT 5;";

            using (MySqlCommand command = new MySqlCommand(sql, connection))
            using (MySqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    resumo.ProdutosStockBaixo.Add(new Produto
                    {
                        ProdutoId = Convert.ToInt32(reader["ProdutoId"]),
                        Descricao = Convert.ToString(reader["Descricao"]),
                        StockAtual = Convert.ToInt32(reader["StockAtual"]),
                        PrecoCompra = Convert.ToDecimal(reader["PrecoCompra"]),
                        PrecoVenda = Convert.ToDecimal(reader["PrecoVenda"]),
                        FornecedorId = Convert.ToInt32(reader["FornecedorId"]),
                        FornecedorNome = Convert.ToString(reader["FornecedorNome"]),
                        Ativo = Convert.ToBoolean(reader["Ativo"])
                    });
                }
            }
        }
    }
}
