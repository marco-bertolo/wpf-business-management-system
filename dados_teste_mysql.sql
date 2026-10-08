-- Dados ficticios para testar a base de dados PAPJoao.
-- Execute este ficheiro apenas uma vez, depois de criar a estrutura.

USE `PAPJoao`;

START TRANSACTION;

INSERT INTO `Fornecedor`
    (`FornecedorId`, `Nome`, `Nif`, `Morada`, `Telemovel`, `Email`, `Ativo`)
VALUES
    (1, 'Plasticos do Oeste Lda', '900100001', 'Zona Industrial 10, Leiria', '910100001', 'geral@plasticos-oeste.example', 1),
    (2, 'Utilplastic Lar Lda', '900100002', 'Rua dos Fabricantes 25, Marinha Grande', '910100002', 'comercial@utilplastic-lar.example', 1),
    (3, 'LimpaMais Distribuicao Lda', '900100003', 'Parque Empresarial 4, Braga', '910100003', 'vendas@limpamais.example', 1),
    (4, 'CasaPlast Distribuicao Lda', '900100004', 'Avenida do Comercio 8, Aveiro', '910100004', 'encomendas@casaplast.example', 1);

INSERT INTO `Cliente`
    (`ClienteId`, `Nome`, `Nif`, `Morada`, `Telemovel`, `Email`, `Ativo`)
VALUES
    (1, 'Banca da Maria Unipessoal Lda', '901200001', 'Mercado Municipal, Leiria', '920200001', 'compras@banca-maria.example', 1),
    (2, 'Feira Pratica Lda', '901200002', 'Rua das Feiras 40, Coimbra', '920200002', 'geral@feira-pratica.example', 1),
    (3, 'Utilidades do Centro Lda', '901200003', 'Rua do Mercado 7, Pombal', '920200003', 'encomendas@utilidades-centro.example', 1),
    (4, 'Casa e Feira Unipessoal Lda', '901200004', 'Largo da Feira 15, Aveiro', '920200004', 'compras@casa-feira.example', 1),
    (5, 'Bazar das Familias Lda', '901200005', 'Rua do Comercio 3, Porto', '920200005', 'bazar@familias.example', 1);

INSERT INTO `Funcionario`
    (`FuncionarioId`, `Nome`, `Nif`, `Morada`, `Telemovel`, `Email`, `Informacoes`, `Ativo`)
VALUES
    (1, 'Ana Martins', '902300001', 'Rua das Flores 1, Leiria', '930300001', 'ana@papjoao.example', 'Vendas, atendimento a feirantes e preparacao de encomendas.', 1),
    (2, 'Bruno Ferreira', '902300002', 'Rua do Parque 8, Pombal', '930300002', 'bruno@papjoao.example', 'Responsavel pelo armazem, cargas e controlo de stock.', 1),
    (3, 'Carla Santos', '902300003', 'Avenida Nova 20, Coimbra', '930300003', 'carla@papjoao.example', 'Faturacao, fornecedores e apoio administrativo.', 1);

INSERT INTO `Produto`
    (`ProdutoId`, `Descricao`, `PrecoCompra`, `PrecoVenda`, `StockAtual`, `FornecedorId`, `Ativo`)
VALUES
    (1, 'Vaso redondo em plastico - 30 cm', 2.10, 3.50, 180, 1, 1),
    (2, 'Vaso quadrado em plastico - 25 cm', 2.60, 4.25, 120, 1, 1),
    (3, 'Conjunto de 3 tigelas em plastico', 3.20, 5.50, 90, 2, 1),
    (4, 'Bacia redonda em plastico - 12 litros', 1.80, 3.00, 140, 2, 1),
    (5, 'Balde em plastico com pega - 10 litros', 2.40, 4.00, 110, 2, 1),
    (6, 'Esfregona de microfibras', 1.65, 2.90, 200, 3, 1),
    (7, 'Cabo metalico para esfregona - 120 cm', 1.10, 2.00, 250, 3, 1),
    (8, 'Conjunto de pa e vassoura', 2.75, 4.75, 95, 3, 1),
    (9, 'Caixa de arrumacao em plastico - 30 litros', 5.50, 8.90, 70, 4, 1),
    (10, 'Cesto para roupa em plastico - 45 litros', 4.80, 7.90, 60, 4, 1),
    (11, 'Banco baixo em plastico', 3.90, 6.50, 75, 4, 1),
    (12, 'Regador em plastico - 5 litros', 2.20, 3.75, 130, 1, 1);

INSERT INTO `Venda`
    (`VendaId`, `DataVenda`, `ClienteId`, `FuncionarioId`, `Total`)
VALUES
    (1, '2026-09-20 10:15:00', 1, 1, 197.50),
    (2, '2026-09-21 14:30:00', 2, 2, 165.00),
    (3, '2026-09-22 09:45:00', 3, 1, 201.20),
    (4, '2026-09-23 16:20:00', 4, 3, 253.00),
    (5, '2026-09-24 11:10:00', 5, 1, 192.50);

INSERT INTO `VendaLinha`
    (`VendaLinhaId`, `VendaId`, `ProdutoId`, `Quantidade`, `PrecoUnitario`)
VALUES
    (1, 1, 1, 20, 3.50),
    (2, 1, 3, 10, 5.50),
    (3, 1, 6, 25, 2.90),
    (4, 2, 4, 15, 3.00),
    (5, 2, 5, 15, 4.00),
    (6, 2, 7, 30, 2.00),
    (7, 3, 2, 12, 4.25),
    (8, 3, 9, 8, 8.90),
    (9, 3, 10, 10, 7.90),
    (10, 4, 6, 40, 2.90),
    (11, 4, 7, 40, 2.00),
    (12, 4, 8, 12, 4.75),
    (13, 5, 1, 15, 3.50),
    (14, 5, 11, 10, 6.50),
    (15, 5, 12, 20, 3.75);

COMMIT;

SELECT
    p.`ProdutoId`,
    p.`Descricao`,
    p.`PrecoCompra`,
    p.`PrecoVenda`,
    p.`StockAtual`,
    f.`Nome` AS `Fornecedor`
FROM `Produto` AS p
INNER JOIN `Fornecedor` AS f
    ON f.`FornecedorId` = p.`FornecedorId`
ORDER BY p.`ProdutoId`;

SELECT
    v.`VendaId`,
    v.`DataVenda`,
    c.`Nome` AS `Cliente`,
    fun.`Nome` AS `Funcionario`,
    v.`Total`
FROM `Venda` AS v
INNER JOIN `Cliente` AS c
    ON c.`ClienteId` = v.`ClienteId`
INNER JOIN `Funcionario` AS fun
    ON fun.`FuncionarioId` = v.`FuncionarioId`
ORDER BY v.`DataVenda`;

SELECT
    v.`VendaId`,
    v.`Total` AS `TotalGuardado`,
    SUM(vl.`Quantidade` * vl.`PrecoUnitario`) AS `TotalCalculado`
FROM `Venda` AS v
INNER JOIN `VendaLinha` AS vl
    ON vl.`VendaId` = v.`VendaId`
GROUP BY v.`VendaId`, v.`Total`
ORDER BY v.`VendaId`;
