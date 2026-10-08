using MySql.Data.MySqlClient;
using PapJoaoSimoes.Models;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace PapJoaoSimoes.Services
{
    public class VendaService
    {
        private readonly string connectionString =
            ConfigurationManager.ConnectionStrings["PAPJoao"].ConnectionString;

        public List<Venda> ObterTodas()
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
                ORDER BY v.DataVenda DESC, v.VendaId DESC;";

            List<Venda> vendas = new List<Venda>();

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            using (MySqlCommand command = new MySqlCommand(sql, connection))
            {
                connection.Open();

                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        vendas.Add(new Venda
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

            return vendas;
        }

        public void Registar(Venda venda, IList<VendaLinha> linhas)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                using (MySqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        ValidarEntidadeAtiva(connection, transaction, "Cliente", "ClienteId", venda.ClienteId, "cliente");
                        ValidarEntidadeAtiva(connection, transaction, "Funcionario", "FuncionarioId", venda.FuncionarioId, "funcionário");

                        const string inserirVenda = @"
                            INSERT INTO Venda (DataVenda, ClienteId, FuncionarioId, Total)
                            VALUES (NOW(), @ClienteId, @FuncionarioId, 0);";

                        using (MySqlCommand command = new MySqlCommand(inserirVenda, connection, transaction))
                        {
                            command.Parameters.Add("@ClienteId", MySqlDbType.Int32).Value = venda.ClienteId;
                            command.Parameters.Add("@FuncionarioId", MySqlDbType.Int32).Value = venda.FuncionarioId;
                            command.ExecuteNonQuery();
                            venda.VendaId = Convert.ToInt32(command.LastInsertedId);
                        }

                        decimal total = 0;

                        foreach (VendaLinha linha in linhas)
                        {
                            Produto produto = ObterProdutoParaVenda(connection, transaction, linha.ProdutoId);

                            if (!produto.Ativo)
                            {
                                throw new InvalidOperationException("O produto \"" + produto.Descricao + "\" está inativo.");
                            }

                            if (linha.Quantidade <= 0 || produto.StockAtual < linha.Quantidade)
                            {
                                throw new InvalidOperationException(
                                    "Stock insuficiente para \"" + produto.Descricao + "\". Disponível: " + produto.StockAtual + ".");
                            }

                            InserirLinha(connection, transaction, venda.VendaId, produto, linha.Quantidade);
                            RetirarStock(connection, transaction, produto.ProdutoId, linha.Quantidade);
                            total += produto.PrecoVenda * linha.Quantidade;
                        }

                        const string atualizarTotal = @"
                            UPDATE Venda SET Total = @Total WHERE VendaId = @VendaId;";

                        using (MySqlCommand command = new MySqlCommand(atualizarTotal, connection, transaction))
                        {
                            command.Parameters.Add("@Total", MySqlDbType.Decimal).Value = total;
                            command.Parameters.Add("@VendaId", MySqlDbType.Int32).Value = venda.VendaId;
                            command.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        venda.Total = total;
                        venda.DataVenda = DateTime.Now;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        private static void ValidarEntidadeAtiva(
            MySqlConnection connection,
            MySqlTransaction transaction,
            string tabela,
            string colunaId,
            int id,
            string nomeEntidade)
        {
            string sql = "SELECT COUNT(*) FROM " + tabela +
                         " WHERE " + colunaId + " = @Id AND Ativo = 1;";

            using (MySqlCommand command = new MySqlCommand(sql, connection, transaction))
            {
                command.Parameters.Add("@Id", MySqlDbType.Int32).Value = id;

                if (Convert.ToInt32(command.ExecuteScalar()) == 0)
                {
                    throw new InvalidOperationException("Seleciona um " + nomeEntidade + " ativo.");
                }
            }
        }

        private static Produto ObterProdutoParaVenda(
            MySqlConnection connection,
            MySqlTransaction transaction,
            int produtoId)
        {
            const string sql = @"
                SELECT ProdutoId, Descricao, PrecoVenda, StockAtual, Ativo
                FROM Produto
                WHERE ProdutoId = @ProdutoId
                FOR UPDATE;";

            using (MySqlCommand command = new MySqlCommand(sql, connection, transaction))
            {
                command.Parameters.Add("@ProdutoId", MySqlDbType.Int32).Value = produtoId;

                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        throw new InvalidOperationException("Um dos produtos selecionados já não existe.");
                    }

                    return new Produto
                    {
                        ProdutoId = Convert.ToInt32(reader["ProdutoId"]),
                        Descricao = Convert.ToString(reader["Descricao"]),
                        PrecoVenda = Convert.ToDecimal(reader["PrecoVenda"]),
                        StockAtual = Convert.ToInt32(reader["StockAtual"]),
                        Ativo = Convert.ToBoolean(reader["Ativo"])
                    };
                }
            }
        }

        private static void InserirLinha(
            MySqlConnection connection,
            MySqlTransaction transaction,
            int vendaId,
            Produto produto,
            int quantidade)
        {
            const string sql = @"
                INSERT INTO VendaLinha (VendaId, ProdutoId, Quantidade, PrecoUnitario)
                VALUES (@VendaId, @ProdutoId, @Quantidade, @PrecoUnitario);";

            using (MySqlCommand command = new MySqlCommand(sql, connection, transaction))
            {
                command.Parameters.Add("@VendaId", MySqlDbType.Int32).Value = vendaId;
                command.Parameters.Add("@ProdutoId", MySqlDbType.Int32).Value = produto.ProdutoId;
                command.Parameters.Add("@Quantidade", MySqlDbType.Int32).Value = quantidade;
                command.Parameters.Add("@PrecoUnitario", MySqlDbType.Decimal).Value = produto.PrecoVenda;
                command.ExecuteNonQuery();
            }
        }

        private static void RetirarStock(
            MySqlConnection connection,
            MySqlTransaction transaction,
            int produtoId,
            int quantidade)
        {
            const string sql = @"
                UPDATE Produto
                SET StockAtual = StockAtual - @Quantidade
                WHERE ProdutoId = @ProdutoId;";

            using (MySqlCommand command = new MySqlCommand(sql, connection, transaction))
            {
                command.Parameters.Add("@Quantidade", MySqlDbType.Int32).Value = quantidade;
                command.Parameters.Add("@ProdutoId", MySqlDbType.Int32).Value = produtoId;
                command.ExecuteNonQuery();
            }
        }
    }
}
