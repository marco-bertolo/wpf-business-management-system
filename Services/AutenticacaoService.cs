using MySql.Data.MySqlClient;
using PapJoaoSimoes.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Security.Cryptography;

namespace PapJoaoSimoes.Services
{
    public class AutenticacaoService
    {
        private const int TamanhoSalt = 16;
        private const int TamanhoHash = 32;
        private const int Iteracoes = 100000;

        private readonly string connectionString =
            ConfigurationManager.ConnectionStrings["PAPJoao"].ConnectionString;

        public void GarantirTabela()
        {
            const string sql = @"
                CREATE TABLE IF NOT EXISTS Utilizador (
                    UtilizadorId INT UNSIGNED NOT NULL AUTO_INCREMENT,
                    NomeUtilizador VARCHAR(50) NOT NULL,
                    PasswordHash VARBINARY(32) NOT NULL,
                    PasswordSalt VARBINARY(16) NOT NULL,
                    Iteracoes INT UNSIGNED NOT NULL,
                    FuncionarioId INT UNSIGNED NULL,
                    Ativo BOOLEAN NOT NULL DEFAULT TRUE,
                    UltimoAcesso DATETIME NULL,
                    CriadoEm DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    CONSTRAINT PK_Utilizador PRIMARY KEY (UtilizadorId),
                    CONSTRAINT UX_Utilizador_Nome UNIQUE (NomeUtilizador),
                    CONSTRAINT FK_Utilizador_Funcionario
                        FOREIGN KEY (FuncionarioId)
                        REFERENCES Funcionario (FuncionarioId)
                        ON UPDATE CASCADE
                        ON DELETE SET NULL
                ) ENGINE=InnoDB;";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            using (MySqlCommand command = new MySqlCommand(sql, connection))
            {
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public bool ExisteUtilizador()
        {
            const string sql = "SELECT COUNT(*) FROM Utilizador;";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            using (MySqlCommand command = new MySqlCommand(sql, connection))
            {
                connection.Open();
                return Convert.ToInt32(command.ExecuteScalar()) > 0;
            }
        }

        public Utilizador CriarAdministrador(string nomeUtilizador, string password)
        {
            byte[] salt = CriarSalt();
            byte[] hash = CriarHash(password, salt, Iteracoes);

            const string sql = @"
                INSERT INTO Utilizador
                    (NomeUtilizador, PasswordHash, PasswordSalt, Iteracoes, Ativo)
                VALUES
                    (@NomeUtilizador, @PasswordHash, @PasswordSalt, @Iteracoes, 1);";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            using (MySqlCommand command = new MySqlCommand(sql, connection))
            {
                command.Parameters.Add("@NomeUtilizador", MySqlDbType.VarChar, 50).Value = nomeUtilizador.Trim();
                command.Parameters.Add("@PasswordHash", MySqlDbType.VarBinary, TamanhoHash).Value = hash;
                command.Parameters.Add("@PasswordSalt", MySqlDbType.VarBinary, TamanhoSalt).Value = salt;
                command.Parameters.Add("@Iteracoes", MySqlDbType.Int32).Value = Iteracoes;
                connection.Open();
                command.ExecuteNonQuery();

                return new Utilizador
                {
                    UtilizadorId = Convert.ToInt32(command.LastInsertedId),
                    NomeUtilizador = nomeUtilizador.Trim(),
                    Ativo = true,
                    UltimoAcesso = DateTime.Now
                };
            }
        }

        public Utilizador Autenticar(string nomeUtilizador, string password)
        {
            const string sql = @"
                SELECT u.UtilizadorId,
                       u.NomeUtilizador,
                       u.PasswordHash,
                       u.PasswordSalt,
                       u.Iteracoes,
                       u.FuncionarioId,
                       f.Nome AS FuncionarioNome,
                       u.UltimoAcesso,
                       u.Ativo
                FROM Utilizador u
                LEFT JOIN Funcionario f ON f.FuncionarioId = u.FuncionarioId
                WHERE u.NomeUtilizador = @NomeUtilizador
                  AND u.Ativo = 1
                LIMIT 1;";

            Utilizador utilizador;
            byte[] hashGuardado;
            byte[] salt;
            int iteracoes;

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                using (MySqlCommand command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.Add("@NomeUtilizador", MySqlDbType.VarChar, 50).Value = nomeUtilizador.Trim();

                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            return null;
                        }

                        hashGuardado = (byte[])reader["PasswordHash"];
                        salt = (byte[])reader["PasswordSalt"];
                        iteracoes = Convert.ToInt32(reader["Iteracoes"]);
                        utilizador = new Utilizador
                        {
                            UtilizadorId = Convert.ToInt32(reader["UtilizadorId"]),
                            NomeUtilizador = Convert.ToString(reader["NomeUtilizador"]),
                            FuncionarioId = reader["FuncionarioId"] == DBNull.Value
                                ? (int?)null
                                : Convert.ToInt32(reader["FuncionarioId"]),
                            FuncionarioNome = Convert.ToString(reader["FuncionarioNome"]),
                            UltimoAcesso = reader["UltimoAcesso"] == DBNull.Value
                                ? (DateTime?)null
                                : Convert.ToDateTime(reader["UltimoAcesso"]),
                            Ativo = Convert.ToBoolean(reader["Ativo"])
                        };
                    }
                }

                byte[] hashIntroduzido = CriarHash(password, salt, iteracoes);
                if (!SaoIguais(hashGuardado, hashIntroduzido))
                {
                    return null;
                }

                const string atualizarAcesso = @"
                    UPDATE Utilizador
                    SET UltimoAcesso = NOW()
                    WHERE UtilizadorId = @UtilizadorId;";

                using (MySqlCommand command = new MySqlCommand(atualizarAcesso, connection))
                {
                    command.Parameters.Add("@UtilizadorId", MySqlDbType.Int32).Value = utilizador.UtilizadorId;
                    command.ExecuteNonQuery();
                }
            }

            utilizador.UltimoAcesso = DateTime.Now;
            return utilizador;
        }

        public IList<Utilizador> ObterUtilizadores()
        {
            const string sql = @"
                SELECT u.UtilizadorId,
                       u.NomeUtilizador,
                       u.FuncionarioId,
                       f.Nome AS FuncionarioNome,
                       u.UltimoAcesso,
                       u.CriadoEm,
                       u.Ativo
                FROM Utilizador u
                LEFT JOIN Funcionario f ON f.FuncionarioId = u.FuncionarioId
                ORDER BY u.Ativo DESC, u.NomeUtilizador;";

            List<Utilizador> utilizadores = new List<Utilizador>();

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            using (MySqlCommand command = new MySqlCommand(sql, connection))
            {
                connection.Open();
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        utilizadores.Add(new Utilizador
                        {
                            UtilizadorId = Convert.ToInt32(reader["UtilizadorId"]),
                            NomeUtilizador = Convert.ToString(reader["NomeUtilizador"]),
                            FuncionarioId = reader["FuncionarioId"] == DBNull.Value
                                ? (int?)null
                                : Convert.ToInt32(reader["FuncionarioId"]),
                            FuncionarioNome = Convert.ToString(reader["FuncionarioNome"]),
                            UltimoAcesso = reader["UltimoAcesso"] == DBNull.Value
                                ? (DateTime?)null
                                : Convert.ToDateTime(reader["UltimoAcesso"]),
                            CriadoEm = Convert.ToDateTime(reader["CriadoEm"]),
                            Ativo = Convert.ToBoolean(reader["Ativo"])
                        });
                    }
                }
            }

            return utilizadores;
        }

        public bool NomeUtilizadorExiste(string nomeUtilizador, int ignorarUtilizadorId)
        {
            const string sql = @"
                SELECT COUNT(*)
                FROM Utilizador
                WHERE NomeUtilizador = @NomeUtilizador
                  AND UtilizadorId <> @UtilizadorId;";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            using (MySqlCommand command = new MySqlCommand(sql, connection))
            {
                command.Parameters.Add("@NomeUtilizador", MySqlDbType.VarChar, 50).Value = nomeUtilizador.Trim();
                command.Parameters.Add("@UtilizadorId", MySqlDbType.Int32).Value = ignorarUtilizadorId;
                connection.Open();
                return Convert.ToInt32(command.ExecuteScalar()) > 0;
            }
        }

        public void AtualizarAdministrador(Utilizador utilizador, string novaPassword)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                using (MySqlTransaction transaction = connection.BeginTransaction())
                {
                    bool estavaAtivo = ObterEstadoUtilizador(connection, transaction, utilizador.UtilizadorId);
                    if (estavaAtivo && !utilizador.Ativo)
                    {
                        GarantirOutroUtilizadorAtivo(connection, transaction, utilizador.UtilizadorId);
                    }

                    bool alterarPassword = !string.IsNullOrEmpty(novaPassword);
                    string sql = alterarPassword
                        ? @"UPDATE Utilizador
                            SET NomeUtilizador = @NomeUtilizador,
                                Ativo = @Ativo,
                                PasswordHash = @PasswordHash,
                                PasswordSalt = @PasswordSalt,
                                Iteracoes = @Iteracoes
                            WHERE UtilizadorId = @UtilizadorId;"
                        : @"UPDATE Utilizador
                            SET NomeUtilizador = @NomeUtilizador,
                                Ativo = @Ativo
                            WHERE UtilizadorId = @UtilizadorId;";

                    using (MySqlCommand command = new MySqlCommand(sql, connection, transaction))
                    {
                        command.Parameters.Add("@NomeUtilizador", MySqlDbType.VarChar, 50).Value = utilizador.NomeUtilizador.Trim();
                        command.Parameters.Add("@Ativo", MySqlDbType.Bit).Value = utilizador.Ativo;
                        command.Parameters.Add("@UtilizadorId", MySqlDbType.Int32).Value = utilizador.UtilizadorId;

                        if (alterarPassword)
                        {
                            byte[] salt = CriarSalt();
                            byte[] hash = CriarHash(novaPassword, salt, Iteracoes);
                            command.Parameters.Add("@PasswordHash", MySqlDbType.VarBinary, TamanhoHash).Value = hash;
                            command.Parameters.Add("@PasswordSalt", MySqlDbType.VarBinary, TamanhoSalt).Value = salt;
                            command.Parameters.Add("@Iteracoes", MySqlDbType.Int32).Value = Iteracoes;
                        }

                        if (command.ExecuteNonQuery() == 0)
                        {
                            throw new InvalidOperationException("A conta já não existe.");
                        }
                    }

                    transaction.Commit();
                }
            }
        }

        public void EliminarAdministrador(int utilizadorId)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                using (MySqlTransaction transaction = connection.BeginTransaction())
                {
                    bool estavaAtivo = ObterEstadoUtilizador(connection, transaction, utilizadorId);
                    if (estavaAtivo)
                    {
                        GarantirOutroUtilizadorAtivo(connection, transaction, utilizadorId);
                    }

                    using (MySqlCommand command = new MySqlCommand(
                        "DELETE FROM Utilizador WHERE UtilizadorId = @UtilizadorId;",
                        connection,
                        transaction))
                    {
                        command.Parameters.Add("@UtilizadorId", MySqlDbType.Int32).Value = utilizadorId;
                        if (command.ExecuteNonQuery() == 0)
                        {
                            throw new InvalidOperationException("A conta já não existe.");
                        }
                    }

                    transaction.Commit();
                }
            }
        }

        private static bool ObterEstadoUtilizador(
            MySqlConnection connection,
            MySqlTransaction transaction,
            int utilizadorId)
        {
            using (MySqlCommand command = new MySqlCommand(
                "SELECT Ativo FROM Utilizador WHERE UtilizadorId = @UtilizadorId FOR UPDATE;",
                connection,
                transaction))
            {
                command.Parameters.Add("@UtilizadorId", MySqlDbType.Int32).Value = utilizadorId;
                object resultado = command.ExecuteScalar();
                if (resultado == null)
                {
                    throw new InvalidOperationException("A conta já não existe.");
                }

                return Convert.ToBoolean(resultado);
            }
        }

        private static void GarantirOutroUtilizadorAtivo(
            MySqlConnection connection,
            MySqlTransaction transaction,
            int utilizadorId)
        {
            const string sql = @"
                SELECT COUNT(*)
                FROM Utilizador
                WHERE Ativo = 1
                  AND UtilizadorId <> @UtilizadorId;";

            using (MySqlCommand command = new MySqlCommand(sql, connection, transaction))
            {
                command.Parameters.Add("@UtilizadorId", MySqlDbType.Int32).Value = utilizadorId;
                if (Convert.ToInt32(command.ExecuteScalar()) == 0)
                {
                    throw new InvalidOperationException("Tem de existir pelo menos uma conta ativa.");
                }
            }
        }

        private static byte[] CriarSalt()
        {
            byte[] salt = new byte[TamanhoSalt];
            using (RandomNumberGenerator gerador = RandomNumberGenerator.Create())
            {
                gerador.GetBytes(salt);
            }

            return salt;
        }

        private static byte[] CriarHash(string password, byte[] salt, int iteracoes)
        {
            using (Rfc2898DeriveBytes derivador = new Rfc2898DeriveBytes(
                password,
                salt,
                iteracoes,
                HashAlgorithmName.SHA256))
            {
                return derivador.GetBytes(TamanhoHash);
            }
        }

        private static bool SaoIguais(byte[] primeiro, byte[] segundo)
        {
            if (primeiro == null || segundo == null || primeiro.Length != segundo.Length)
            {
                return false;
            }

            int diferenca = 0;
            for (int i = 0; i < primeiro.Length; i++)
            {
                diferenca |= primeiro[i] ^ segundo[i];
            }

            return diferenca == 0;
        }
    }
}
