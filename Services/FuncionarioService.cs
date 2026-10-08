using MySql.Data.MySqlClient;
using PapJoaoSimoes.Models;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace PapJoaoSimoes.Services
{
    public class FuncionarioService
    {
        private readonly string connectionString =
            ConfigurationManager
                .ConnectionStrings["PAPJoao"]
                .ConnectionString;

        public List<Funcionario> ObterTodos()
        {
            List<Funcionario> funcionarios = new List<Funcionario>();

            string sql = @"
                SELECT
                    FuncionarioId,
                    Nome,
                    Nif,
                    Morada,
                    Telemovel,
                    Email,
                    Informacoes,
                    Ativo
                FROM Funcionario
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
                        Funcionario funcionario = new Funcionario
                        {
                            FuncionarioId =
                                Convert.ToInt32(reader["FuncionarioId"]),

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

                            Informacoes =
                                Convert.ToString(reader["Informacoes"]),

                            Ativo =
                                Convert.ToBoolean(reader["Ativo"])
                        };

                        funcionarios.Add(funcionario);
                    }
                }
            }

            return funcionarios;
        }

        public void Inserir(Funcionario funcionario)
        {
            const string sql = @"
                INSERT INTO Funcionario
                    (Nome, Nif, Morada, Telemovel, Email, Informacoes, Ativo)
                VALUES
                    (@Nome, @Nif, @Morada, @Telemovel, @Email, @Informacoes, @Ativo);";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            using (MySqlCommand command = new MySqlCommand(sql, connection))
            {
                AdicionarParametros(command, funcionario);
                connection.Open();
                command.ExecuteNonQuery();
                funcionario.FuncionarioId = Convert.ToInt32(command.LastInsertedId);
            }
        }

        public void Atualizar(Funcionario funcionario)
        {
            const string sql = @"
                UPDATE Funcionario
                SET Nome = @Nome,
                    Nif = @Nif,
                    Morada = @Morada,
                    Telemovel = @Telemovel,
                    Email = @Email,
                    Informacoes = @Informacoes,
                    Ativo = @Ativo
                WHERE FuncionarioId = @FuncionarioId;";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            using (MySqlCommand command = new MySqlCommand(sql, connection))
            {
                AdicionarParametros(command, funcionario);
                command.Parameters.Add("@FuncionarioId", MySqlDbType.Int32).Value = funcionario.FuncionarioId;
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void Desativar(int funcionarioId)
        {
            const string sql = @"
                UPDATE Funcionario
                SET Ativo = 0
                WHERE FuncionarioId = @FuncionarioId;";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            using (MySqlCommand command = new MySqlCommand(sql, connection))
            {
                command.Parameters.Add("@FuncionarioId", MySqlDbType.Int32).Value = funcionarioId;
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public bool NifExiste(string nif, int funcionarioIdIgnorar)
        {
            const string sql = @"
                SELECT COUNT(*)
                FROM Funcionario
                WHERE Nif = @Nif
                  AND FuncionarioId <> @FuncionarioIdIgnorar;";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            using (MySqlCommand command = new MySqlCommand(sql, connection))
            {
                command.Parameters.Add("@Nif", MySqlDbType.VarChar, 9).Value = nif;
                command.Parameters.Add("@FuncionarioIdIgnorar", MySqlDbType.Int32).Value = funcionarioIdIgnorar;
                connection.Open();
                return Convert.ToInt32(command.ExecuteScalar()) > 0;
            }
        }

        private static void AdicionarParametros(MySqlCommand command, Funcionario funcionario)
        {
            command.Parameters.Add("@Nome", MySqlDbType.VarChar, 100).Value = funcionario.Nome;
            command.Parameters.Add("@Nif", MySqlDbType.VarChar, 9).Value = funcionario.Nif;
            command.Parameters.Add("@Morada", MySqlDbType.VarChar, 255).Value = ValorOuDbNull(funcionario.Morada);
            command.Parameters.Add("@Telemovel", MySqlDbType.VarChar, 20).Value = ValorOuDbNull(funcionario.Telemovel);
            command.Parameters.Add("@Email", MySqlDbType.VarChar, 255).Value = ValorOuDbNull(funcionario.Email);
            command.Parameters.Add("@Informacoes", MySqlDbType.Text).Value = ValorOuDbNull(funcionario.Informacoes);
            command.Parameters.Add("@Ativo", MySqlDbType.Byte).Value = funcionario.Ativo ? 1 : 0;
        }

        private static object ValorOuDbNull(string valor)
        {
            return string.IsNullOrWhiteSpace(valor) ? (object)DBNull.Value : valor;
        }
    }
}




