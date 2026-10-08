using MySql.Data.MySqlClient;
using PapJoaoSimoes.Models;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace PapJoaoSimoes.Services
{
    public class FornecedorService
    {
        private readonly string connectionString =
            ConfigurationManager
                .ConnectionStrings["PAPJoao"]
                .ConnectionString;

        public List<Fornecedor> ObterTodos()
        {
            List<Fornecedor> fornecedores = new List<Fornecedor>();

            string sql = @"
                SELECT
                    FornecedorId,
                    Nome,
                    Nif,
                    Morada,
                    Telemovel,
                    Email,
                    Ativo
                FROM Fornecedor
                ORDER BY Nome";

            using (MySqlConnection connection =
                   new MySqlConnection(connectionString))
            using (MySqlCommand command =
                   new MySqlCommand(sql, connection))
            {
                connection.Open();

                using (MySqlDataReader reader =
                       command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Fornecedor fornecedor = new Fornecedor
                        {
                            FornecedorId =
                                Convert.ToInt32(reader["FornecedorId"]),

                            Nome =
                                Convert.ToString(reader["Nome"]),

                            Nif =
                                Convert.ToString(reader["Nif"]),

                            Morada =
                                Convert.ToString(reader["Morada"]),

                            Telemovel =
                                Convert.ToString(reader["Telemovel"]),

                            Email =
                                Convert.ToString(reader["Email"]),

                            Ativo =
                                Convert.ToBoolean(reader["Ativo"])
                        };

                        fornecedores.Add(fornecedor);
                    }
                }
            }

            return fornecedores;
        }

        public void Inserir(Fornecedor fornecedor)
        {
            const string sql = @"
                INSERT INTO Fornecedor
                    (Nome, Nif, Morada, Telemovel, Email, Ativo)
                VALUES
                    (@Nome, @Nif, @Morada, @Telemovel, @Email, @Ativo);";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            using (MySqlCommand command = new MySqlCommand(sql, connection))
            {
                AdicionarParametros(command, fornecedor);
                connection.Open();
                command.ExecuteNonQuery();
                fornecedor.FornecedorId = Convert.ToInt32(command.LastInsertedId);
            }
        }

        public void Atualizar(Fornecedor fornecedor)
        {
            const string sql = @"
                UPDATE Fornecedor
                SET Nome = @Nome,
                    Nif = @Nif,
                    Morada = @Morada,
                    Telemovel = @Telemovel,
                    Email = @Email,
                    Ativo = @Ativo
                WHERE FornecedorId = @FornecedorId;";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            using (MySqlCommand command = new MySqlCommand(sql, connection))
            {
                AdicionarParametros(command, fornecedor);
                command.Parameters.Add("@FornecedorId", MySqlDbType.Int32).Value = fornecedor.FornecedorId;
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void Desativar(int fornecedorId)
        {
            const string sql = @"
                UPDATE Fornecedor
                SET Ativo = 0
                WHERE FornecedorId = @FornecedorId;";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            using (MySqlCommand command = new MySqlCommand(sql, connection))
            {
                command.Parameters.Add("@FornecedorId", MySqlDbType.Int32).Value = fornecedorId;
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public bool NifExiste(string nif, int fornecedorIdIgnorar)
        {
            const string sql = @"
                SELECT COUNT(*)
                FROM Fornecedor
                WHERE Nif = @Nif
                  AND FornecedorId <> @FornecedorIdIgnorar;";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            using (MySqlCommand command = new MySqlCommand(sql, connection))
            {
                command.Parameters.Add("@Nif", MySqlDbType.VarChar, 9).Value = nif;
                command.Parameters.Add("@FornecedorIdIgnorar", MySqlDbType.Int32).Value = fornecedorIdIgnorar;
                connection.Open();
                return Convert.ToInt32(command.ExecuteScalar()) > 0;
            }
        }

        private static void AdicionarParametros(MySqlCommand command, Fornecedor fornecedor)
        {
            command.Parameters.Add("@Nome", MySqlDbType.VarChar, 100).Value = fornecedor.Nome;
            command.Parameters.Add("@Nif", MySqlDbType.VarChar, 9).Value = fornecedor.Nif;
            command.Parameters.Add("@Morada", MySqlDbType.VarChar, 255).Value = ValorOuDbNull(fornecedor.Morada);
            command.Parameters.Add("@Telemovel", MySqlDbType.VarChar, 20).Value = ValorOuDbNull(fornecedor.Telemovel);
            command.Parameters.Add("@Email", MySqlDbType.VarChar, 255).Value = ValorOuDbNull(fornecedor.Email);
            command.Parameters.Add("@Ativo", MySqlDbType.Byte).Value = fornecedor.Ativo ? 1 : 0;
        }

        private static object ValorOuDbNull(string valor)
        {
            return string.IsNullOrWhiteSpace(valor) ? (object)DBNull.Value : valor;
        }
    }
}


