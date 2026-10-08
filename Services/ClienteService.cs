using MySql.Data.MySqlClient;
using PapJoaoSimoes.Models;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace PapJoaoSimoes.Services
{
    public class ClienteService
    {
        private readonly string connectionString =
            ConfigurationManager
                .ConnectionStrings["PAPJoao"]
                .ConnectionString;

        public List<Cliente> ObterTodos()
        {
            List<Cliente> clientes = new List<Cliente>();

            string sql = @"
                SELECT
                    ClienteId,
                    Nome,
                    Nif,
                    Morada,
                    Telemovel,
                    Email,
                    Ativo
                FROM Cliente
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
                        Cliente cliente = new Cliente
                        {
                            ClienteId =
                                Convert.ToInt32(reader["ClienteId"]),

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

                        clientes.Add(cliente);
                    }
                }
            }

            return clientes;
        }

        public void Inserir(Cliente cliente)
        {
            const string sql = @"
                INSERT INTO Cliente
                    (Nome, Nif, Morada, Telemovel, Email, Ativo)
                VALUES
                    (@Nome, @Nif, @Morada, @Telemovel, @Email, @Ativo);";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            using (MySqlCommand command = new MySqlCommand(sql, connection))
            {
                AdicionarParametros(command, cliente);
                connection.Open();
                command.ExecuteNonQuery();
                cliente.ClienteId = Convert.ToInt32(command.LastInsertedId);
            }
        }

        public void Atualizar(Cliente cliente)
        {
            const string sql = @"
                UPDATE Cliente
                SET Nome = @Nome,
                    Nif = @Nif,
                    Morada = @Morada,
                    Telemovel = @Telemovel,
                    Email = @Email,
                    Ativo = @Ativo
                WHERE ClienteId = @ClienteId;";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            using (MySqlCommand command = new MySqlCommand(sql, connection))
            {
                AdicionarParametros(command, cliente);
                command.Parameters.Add("@ClienteId", MySqlDbType.Int32).Value = cliente.ClienteId;
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void Desativar(int clienteId)
        {
            const string sql = @"
                UPDATE Cliente
                SET Ativo = 0
                WHERE ClienteId = @ClienteId;";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            using (MySqlCommand command = new MySqlCommand(sql, connection))
            {
                command.Parameters.Add("@ClienteId", MySqlDbType.Int32).Value = clienteId;
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public bool NifExiste(string nif, int clienteIdIgnorar)
        {
            const string sql = @"
                SELECT COUNT(*)
                FROM Cliente
                WHERE Nif = @Nif
                  AND ClienteId <> @ClienteIdIgnorar;";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            using (MySqlCommand command = new MySqlCommand(sql, connection))
            {
                command.Parameters.Add("@Nif", MySqlDbType.VarChar, 9).Value = nif;
                command.Parameters.Add("@ClienteIdIgnorar", MySqlDbType.Int32).Value = clienteIdIgnorar;
                connection.Open();
                return Convert.ToInt32(command.ExecuteScalar()) > 0;
            }
        }

        private static void AdicionarParametros(MySqlCommand command, Cliente cliente)
        {
            command.Parameters.Add("@Nome", MySqlDbType.VarChar, 100).Value = cliente.Nome;
            command.Parameters.Add("@Nif", MySqlDbType.VarChar, 9).Value = cliente.Nif;
            command.Parameters.Add("@Morada", MySqlDbType.VarChar, 255).Value = ValorOuDbNull(cliente.Morada);
            command.Parameters.Add("@Telemovel", MySqlDbType.VarChar, 20).Value = ValorOuDbNull(cliente.Telemovel);
            command.Parameters.Add("@Email", MySqlDbType.VarChar, 255).Value = ValorOuDbNull(cliente.Email);
            command.Parameters.Add("@Ativo", MySqlDbType.Byte).Value = cliente.Ativo ? 1 : 0;
        }

        private static object ValorOuDbNull(string valor)
        {
            return string.IsNullOrWhiteSpace(valor) ? (object)DBNull.Value : valor;
        }
    }
}
