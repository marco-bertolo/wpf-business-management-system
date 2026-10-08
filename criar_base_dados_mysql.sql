CREATE DATABASE IF NOT EXISTS `PAPJoao`
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;

USE `PAPJoao`;

CREATE TABLE IF NOT EXISTS `Cliente` (
    `ClienteId` INT UNSIGNED NOT NULL AUTO_INCREMENT,
    `Nome` VARCHAR(100) NOT NULL,
    `Nif` CHAR(9) NOT NULL,
    `Morada` VARCHAR(255) NULL,
    `Telemovel` VARCHAR(20) NULL,
    `Email` VARCHAR(255) NULL,
    `Ativo` BOOLEAN NOT NULL DEFAULT TRUE,
    CONSTRAINT `PK_Cliente` PRIMARY KEY (`ClienteId`),
    CONSTRAINT `UX_Cliente_Nif` UNIQUE (`Nif`),
    CONSTRAINT `CK_Cliente_Nif` CHECK (`Nif` REGEXP '^[0-9]{9}$')
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS `Fornecedor` (
    `FornecedorId` INT UNSIGNED NOT NULL AUTO_INCREMENT,
    `Nome` VARCHAR(100) NOT NULL,
    `Nif` CHAR(9) NOT NULL,
    `Morada` VARCHAR(255) NULL,
    `Telemovel` VARCHAR(20) NULL,
    `Email` VARCHAR(255) NULL,
    `Ativo` BOOLEAN NOT NULL DEFAULT TRUE,
    CONSTRAINT `PK_Fornecedor` PRIMARY KEY (`FornecedorId`),
    CONSTRAINT `UX_Fornecedor_Nif` UNIQUE (`Nif`),
    CONSTRAINT `CK_Fornecedor_Nif` CHECK (`Nif` REGEXP '^[0-9]{9}$')
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS `Funcionario` (
    `FuncionarioId` INT UNSIGNED NOT NULL AUTO_INCREMENT,
    `Nome` VARCHAR(100) NOT NULL,
    `Nif` CHAR(9) NOT NULL,
    `Morada` VARCHAR(255) NULL,
    `Telemovel` VARCHAR(20) NULL,
    `Email` VARCHAR(255) NULL,
    `Informacoes` TEXT NULL,
    `Ativo` BOOLEAN NOT NULL DEFAULT TRUE,
    CONSTRAINT `PK_Funcionario` PRIMARY KEY (`FuncionarioId`),
    CONSTRAINT `UX_Funcionario_Nif` UNIQUE (`Nif`),
    CONSTRAINT `CK_Funcionario_Nif` CHECK (`Nif` REGEXP '^[0-9]{9}$')
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS `Utilizador` (
    `UtilizadorId` INT UNSIGNED NOT NULL AUTO_INCREMENT,
    `NomeUtilizador` VARCHAR(50) NOT NULL,
    `PasswordHash` VARBINARY(32) NOT NULL,
    `PasswordSalt` VARBINARY(16) NOT NULL,
    `Iteracoes` INT UNSIGNED NOT NULL,
    `FuncionarioId` INT UNSIGNED NULL,
    `Ativo` BOOLEAN NOT NULL DEFAULT TRUE,
    `UltimoAcesso` DATETIME NULL,
    `CriadoEm` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT `PK_Utilizador` PRIMARY KEY (`UtilizadorId`),
    CONSTRAINT `UX_Utilizador_Nome` UNIQUE (`NomeUtilizador`),
    CONSTRAINT `FK_Utilizador_Funcionario`
        FOREIGN KEY (`FuncionarioId`)
        REFERENCES `Funcionario` (`FuncionarioId`)
        ON UPDATE CASCADE
        ON DELETE SET NULL
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS `Produto` (
    `ProdutoId` INT UNSIGNED NOT NULL AUTO_INCREMENT,
    `Descricao` VARCHAR(150) NOT NULL,
    `PrecoCompra` DECIMAL(10,2) NOT NULL,
    `PrecoVenda` DECIMAL(10,2) NOT NULL,
    `StockAtual` INT UNSIGNED NOT NULL DEFAULT 0,
    `FornecedorId` INT UNSIGNED NOT NULL,
    `Ativo` BOOLEAN NOT NULL DEFAULT TRUE,
    CONSTRAINT `PK_Produto` PRIMARY KEY (`ProdutoId`),
    CONSTRAINT `CK_Produto_PrecoCompra` CHECK (`PrecoCompra` >= 0),
    CONSTRAINT `CK_Produto_PrecoVenda` CHECK (`PrecoVenda` >= 0),
    CONSTRAINT `FK_Produto_Fornecedor`
        FOREIGN KEY (`FornecedorId`)
        REFERENCES `Fornecedor` (`FornecedorId`)
        ON UPDATE CASCADE
        ON DELETE RESTRICT
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS `Venda` (
    `VendaId` INT UNSIGNED NOT NULL AUTO_INCREMENT,
    `DataVenda` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `ClienteId` INT UNSIGNED NOT NULL,
    `FuncionarioId` INT UNSIGNED NOT NULL,
    `Total` DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    CONSTRAINT `PK_Venda` PRIMARY KEY (`VendaId`),
    CONSTRAINT `CK_Venda_Total` CHECK (`Total` >= 0),
    CONSTRAINT `FK_Venda_Cliente`
        FOREIGN KEY (`ClienteId`)
        REFERENCES `Cliente` (`ClienteId`)
        ON UPDATE CASCADE
        ON DELETE RESTRICT,
    CONSTRAINT `FK_Venda_Funcionario`
        FOREIGN KEY (`FuncionarioId`)
        REFERENCES `Funcionario` (`FuncionarioId`)
        ON UPDATE CASCADE
        ON DELETE RESTRICT
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS `VendaLinha` (
    `VendaLinhaId` INT UNSIGNED NOT NULL AUTO_INCREMENT,
    `VendaId` INT UNSIGNED NOT NULL,
    `ProdutoId` INT UNSIGNED NOT NULL,
    `Quantidade` INT UNSIGNED NOT NULL,
    `PrecoUnitario` DECIMAL(10,2) NOT NULL,
    CONSTRAINT `PK_VendaLinha` PRIMARY KEY (`VendaLinhaId`),
    CONSTRAINT `UX_VendaLinha_Produto` UNIQUE (`VendaId`, `ProdutoId`),
    CONSTRAINT `CK_VendaLinha_Quantidade` CHECK (`Quantidade` > 0),
    CONSTRAINT `CK_VendaLinha_PrecoUnitario` CHECK (`PrecoUnitario` >= 0),
    CONSTRAINT `FK_VendaLinha_Venda`
        FOREIGN KEY (`VendaId`)
        REFERENCES `Venda` (`VendaId`)
        ON UPDATE CASCADE
        ON DELETE CASCADE,
    CONSTRAINT `FK_VendaLinha_Produto`
        FOREIGN KEY (`ProdutoId`)
        REFERENCES `Produto` (`ProdutoId`)
        ON UPDATE CASCADE
        ON DELETE RESTRICT
) ENGINE=InnoDB;
