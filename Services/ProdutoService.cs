using MySql.Data.MySqlClient;
using PapJoaoSimoes.Models;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace PapJoaoSimoes.Services
{
    public class ProdutoService
    {
        private readonly string connectionString =
            ConfigurationManager.ConnectionStrings["PAPJoao"].ConnectionString;

        public List<Produto> ObterTodos()
        {
            const string sql = @"
                SELECT p.ProdutoId,
                       p.Descricao,
                       p.PrecoCompra,
                       p.PrecoVenda,
                       p.StockAtual,
                       p.FornecedorId,
                       f.Nome AS FornecedorNome,
                       p.Ativo
                FROM Produto p
                INNER JOIN Fornecedor f ON f.FornecedorId = p.FornecedorId
                ORDER BY p.Descricao;";

            List<Produto> produtos = new List<Produto>();

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            using (MySqlCommand command = new MySqlCommand(sql, connection))
            {
                connection.Open();

                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        produtos.Add(new Produto
                        {
                            ProdutoId = Convert.ToInt32(reader["ProdutoId"]),
                            Descricao = Convert.ToString(reader["Descricao"]),
                            PrecoCompra = Convert.ToDecimal(reader["PrecoCompra"]),
                            PrecoVenda = Convert.ToDecimal(reader["PrecoVenda"]),
                            StockAtual = Convert.ToInt32(reader["StockAtual"]),
                            FornecedorId = Convert.ToInt32(reader["FornecedorId"]),
                            FornecedorNome = Convert.ToString(reader["FornecedorNome"]),
                            Ativo = Convert.ToBoolean(reader["Ativo"])
                        });
                    }
                }
            }

            return produtos;
        }

        public void Inserir(Produto produto)
        {
            const string sql = @"
                INSERT INTO Produto
                    (Descricao, PrecoCompra, PrecoVenda, StockAtual, FornecedorId, Ativo)
                VALUES
                    (@Descricao, @PrecoCompra, @PrecoVenda, @StockAtual, @FornecedorId, @Ativo);";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            using (MySqlCommand command = new MySqlCommand(sql, connection))
            {
                AdicionarParametros(command, produto);
                connection.Open();
                command.ExecuteNonQuery();
                produto.ProdutoId = Convert.ToInt32(command.LastInsertedId);
            }
        }

        public void Atualizar(Produto produto)
        {
            const string sql = @"
                UPDATE Produto
                SET Descricao = @Descricao,
                    PrecoCompra = @PrecoCompra,
                    PrecoVenda = @PrecoVenda,
                    StockAtual = @StockAtual,
                    FornecedorId = @FornecedorId,
                    Ativo = @Ativo
                WHERE ProdutoId = @ProdutoId;";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            using (MySqlCommand command = new MySqlCommand(sql, connection))
            {
                AdicionarParametros(command, produto);
                command.Parameters.Add("@ProdutoId", MySqlDbType.Int32).Value = produto.ProdutoId;
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void Desativar(int produtoId)
        {
            const string sql = @"
                UPDATE Produto
                SET Ativo = 0
                WHERE ProdutoId = @ProdutoId;";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            using (MySqlCommand command = new MySqlCommand(sql, connection))
            {
                command.Parameters.Add("@ProdutoId", MySqlDbType.Int32).Value = produtoId;
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        private static void AdicionarParametros(MySqlCommand command, Produto produto)
        {
            command.Parameters.Add("@Descricao", MySqlDbType.VarChar, 150).Value = produto.Descricao;
            command.Parameters.Add("@PrecoCompra", MySqlDbType.Decimal).Value = produto.PrecoCompra;
            command.Parameters.Add("@PrecoVenda", MySqlDbType.Decimal).Value = produto.PrecoVenda;
            command.Parameters.Add("@StockAtual", MySqlDbType.Int32).Value = produto.StockAtual;
            command.Parameters.Add("@FornecedorId", MySqlDbType.Int32).Value = produto.FornecedorId;
            command.Parameters.Add("@Ativo", MySqlDbType.Byte).Value = produto.Ativo ? 1 : 0;
        }
    }
}
