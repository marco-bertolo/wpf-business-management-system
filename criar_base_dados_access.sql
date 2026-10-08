-- Base de dados: PAPJoao.accdb
-- O Microsoft Access nao suporta a instrucao CREATE DATABASE.
-- Primeiro crie o ficheiro PAPJoao.accdb e depois execute cada instrucao
-- separadamente e pela ordem apresentada.

CREATE TABLE Cliente (
    ClienteId AUTOINCREMENT CONSTRAINT PK_Cliente PRIMARY KEY,
    Nome TEXT(100) NOT NULL,
    Nif TEXT(9) NOT NULL,
    Morada TEXT(255),
    Telemovel TEXT(20),
    Email TEXT(255),
    Ativo YESNO NOT NULL
);

CREATE TABLE Fornecedor (
    FornecedorId AUTOINCREMENT CONSTRAINT PK_Fornecedor PRIMARY KEY,
    Nome TEXT(100) NOT NULL,
    Nif TEXT(9) NOT NULL,
    Morada TEXT(255),
    Telemovel TEXT(20),
    Email TEXT(255),
    Ativo YESNO NOT NULL
);

CREATE TABLE Funcionario (
    FuncionarioId AUTOINCREMENT CONSTRAINT PK_Funcionario PRIMARY KEY,
    Nome TEXT(100) NOT NULL,
    Nif TEXT(9) NOT NULL,
    Morada TEXT(255),
    Telemovel TEXT(20),
    Email TEXT(255),
    Informacoes LONGTEXT,
    Ativo YESNO NOT NULL
);

CREATE TABLE Produto (
    ProdutoId AUTOINCREMENT CONSTRAINT PK_Produto PRIMARY KEY,
    Descricao TEXT(150) NOT NULL,
    PrecoCompra CURRENCY NOT NULL,
    PrecoVenda CURRENCY NOT NULL,
    StockAtual LONG NOT NULL,
    FornecedorId LONG NOT NULL,
    Ativo YESNO NOT NULL
);

CREATE TABLE Venda (
    VendaId AUTOINCREMENT CONSTRAINT PK_Venda PRIMARY KEY,
    DataVenda DATETIME NOT NULL,
    ClienteId LONG NOT NULL,
    FuncionarioId LONG NOT NULL,
    Total CURRENCY NOT NULL
);

CREATE TABLE VendaLinha (
    VendaLinhaId AUTOINCREMENT CONSTRAINT PK_VendaLinha PRIMARY KEY,
    VendaId LONG NOT NULL,
    ProdutoId LONG NOT NULL,
    Quantidade LONG NOT NULL,
    PrecoUnitario CURRENCY NOT NULL
);

CREATE UNIQUE INDEX UX_Cliente_Nif
ON Cliente (Nif);

CREATE UNIQUE INDEX UX_Fornecedor_Nif
ON Fornecedor (Nif);

CREATE UNIQUE INDEX UX_Funcionario_Nif
ON Funcionario (Nif);

CREATE UNIQUE INDEX UX_VendaLinha_Produto
ON VendaLinha (VendaId, ProdutoId);

ALTER TABLE Produto
ADD CONSTRAINT FK_Produto_Fornecedor
FOREIGN KEY (FornecedorId)
REFERENCES Fornecedor (FornecedorId);

ALTER TABLE Venda
ADD CONSTRAINT FK_Venda_Cliente
FOREIGN KEY (ClienteId)
REFERENCES Cliente (ClienteId);

ALTER TABLE Venda
ADD CONSTRAINT FK_Venda_Funcionario
FOREIGN KEY (FuncionarioId)
REFERENCES Funcionario (FuncionarioId);

ALTER TABLE VendaLinha
ADD CONSTRAINT FK_VendaLinha_Venda
FOREIGN KEY (VendaId)
REFERENCES Venda (VendaId);

ALTER TABLE VendaLinha
ADD CONSTRAINT FK_VendaLinha_Produto
FOREIGN KEY (ProdutoId)
REFERENCES Produto (ProdutoId);
