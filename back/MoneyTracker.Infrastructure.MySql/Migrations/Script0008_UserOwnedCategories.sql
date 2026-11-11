-- =========================================================
-- Script0008_UserOwnedCategories
-- Las categorias de gasto e ingreso pasan a tener dueno:
--   user_id NULL     -> categoria COMUN del sistema (todos la ven, solo lectura)
--   user_id = <user> -> categoria PROPIA de ese usuario (crear/editar/borrar)
--
-- Aditivo: solo ADD COLUMN nullable + FK + reparto de las filas existentes.
-- No borra ni mueve movimientos, presupuestos ni ninguna FK existente.
-- DbUp lo ejecuta una sola vez.
-- =========================================================

-- --- 1. Columna de propiedad -----------------------------------------------
ALTER TABLE `ExpenseCategories` ADD COLUMN `user_id` INT UNSIGNED NULL;
ALTER TABLE `ExpenseCategories`
  ADD CONSTRAINT `fk_expcat_user` FOREIGN KEY (`user_id`)
  REFERENCES `Users` (`id`) ON DELETE CASCADE ON UPDATE CASCADE;
CREATE INDEX `ix_expcat_user` ON `ExpenseCategories` (`user_id`);

ALTER TABLE `IncomeCategories` ADD COLUMN `user_id` INT UNSIGNED NULL;
ALTER TABLE `IncomeCategories`
  ADD CONSTRAINT `fk_inccat_user` FOREIGN KEY (`user_id`)
  REFERENCES `Users` (`id`) ON DELETE CASCADE ON UPDATE CASCADE;
CREATE INDEX `ix_inccat_user` ON `IncomeCategories` (`user_id`);

-- --- 2. Reparto de las categorias que ya existen ---------------------------
-- Criterio: si de una categoria solo tira UN usuario (por sus gastos/ingresos o
-- sus presupuestos), pasa a ser suya. Si no la usa nadie, o la usan varios,
-- se queda como comun (user_id NULL).

CREATE TEMPORARY TABLE `tmp_expcat_owner` AS
SELECT `cid`, MIN(`uid`) AS `uid`, COUNT(DISTINCT `uid`) AS `n`
FROM (
      SELECT `category_id` AS `cid`, `user_id` AS `uid` FROM `Expenses`
  UNION
      SELECT `category_id`, `user_id` FROM `CategoryBudgets`
  UNION
      SELECT `category_id`, `user_id` FROM `Budgets`
) `t`
GROUP BY `cid`;

UPDATE `ExpenseCategories` `c`
  JOIN `tmp_expcat_owner` `o` ON `o`.`cid` = `c`.`id` AND `o`.`n` = 1
SET `c`.`user_id` = `o`.`uid`;

DROP TEMPORARY TABLE `tmp_expcat_owner`;

CREATE TEMPORARY TABLE `tmp_inccat_owner` AS
SELECT `category_id` AS `cid`, MIN(`user_id`) AS `uid`, COUNT(DISTINCT `user_id`) AS `n`
FROM `Incomes`
GROUP BY `category_id`;

UPDATE `IncomeCategories` `c`
  JOIN `tmp_inccat_owner` `o` ON `o`.`cid` = `c`.`id` AND `o`.`n` = 1
SET `c`.`user_id` = `o`.`uid`;

DROP TEMPORARY TABLE `tmp_inccat_owner`;

-- --- 3. Coherencia del arbol ----------------------------------------------
-- Una categoria no puede colgar de un padre que su dueno no ve, asi que:
--   a) si un padre con dueno tiene hijos de OTRO usuario, el padre pasa a comun;
--   b) un hijo sin dueno cuyo padre SI tiene dueno pasa a ese dueno (si no,
--      quedaria visible para todos colgando de un padre que no ven).
-- Se repite por niveles (el arbol es de pocos niveles; las sentencias son
-- idempotentes, la ultima pasada no cambia nada).

UPDATE `ExpenseCategories` `p` JOIN `ExpenseCategories` `ch` ON `ch`.`parent_id` = `p`.`id`
SET `p`.`user_id` = NULL
WHERE `p`.`user_id` IS NOT NULL AND `ch`.`user_id` IS NOT NULL AND `ch`.`user_id` <> `p`.`user_id`;
UPDATE `ExpenseCategories` `p` JOIN `ExpenseCategories` `ch` ON `ch`.`parent_id` = `p`.`id`
SET `p`.`user_id` = NULL
WHERE `p`.`user_id` IS NOT NULL AND `ch`.`user_id` IS NOT NULL AND `ch`.`user_id` <> `p`.`user_id`;
UPDATE `ExpenseCategories` `p` JOIN `ExpenseCategories` `ch` ON `ch`.`parent_id` = `p`.`id`
SET `p`.`user_id` = NULL
WHERE `p`.`user_id` IS NOT NULL AND `ch`.`user_id` IS NOT NULL AND `ch`.`user_id` <> `p`.`user_id`;

UPDATE `IncomeCategories` `p` JOIN `IncomeCategories` `ch` ON `ch`.`parent_id` = `p`.`id`
SET `p`.`user_id` = NULL
WHERE `p`.`user_id` IS NOT NULL AND `ch`.`user_id` IS NOT NULL AND `ch`.`user_id` <> `p`.`user_id`;
UPDATE `IncomeCategories` `p` JOIN `IncomeCategories` `ch` ON `ch`.`parent_id` = `p`.`id`
SET `p`.`user_id` = NULL
WHERE `p`.`user_id` IS NOT NULL AND `ch`.`user_id` IS NOT NULL AND `ch`.`user_id` <> `p`.`user_id`;
UPDATE `IncomeCategories` `p` JOIN `IncomeCategories` `ch` ON `ch`.`parent_id` = `p`.`id`
SET `p`.`user_id` = NULL
WHERE `p`.`user_id` IS NOT NULL AND `ch`.`user_id` IS NOT NULL AND `ch`.`user_id` <> `p`.`user_id`;

UPDATE `ExpenseCategories` `ch` JOIN `ExpenseCategories` `p` ON `p`.`id` = `ch`.`parent_id`
SET `ch`.`user_id` = `p`.`user_id`
WHERE `ch`.`user_id` IS NULL AND `p`.`user_id` IS NOT NULL;
UPDATE `ExpenseCategories` `ch` JOIN `ExpenseCategories` `p` ON `p`.`id` = `ch`.`parent_id`
SET `ch`.`user_id` = `p`.`user_id`
WHERE `ch`.`user_id` IS NULL AND `p`.`user_id` IS NOT NULL;
UPDATE `ExpenseCategories` `ch` JOIN `ExpenseCategories` `p` ON `p`.`id` = `ch`.`parent_id`
SET `ch`.`user_id` = `p`.`user_id`
WHERE `ch`.`user_id` IS NULL AND `p`.`user_id` IS NOT NULL;

UPDATE `IncomeCategories` `ch` JOIN `IncomeCategories` `p` ON `p`.`id` = `ch`.`parent_id`
SET `ch`.`user_id` = `p`.`user_id`
WHERE `ch`.`user_id` IS NULL AND `p`.`user_id` IS NOT NULL;
UPDATE `IncomeCategories` `ch` JOIN `IncomeCategories` `p` ON `p`.`id` = `ch`.`parent_id`
SET `ch`.`user_id` = `p`.`user_id`
WHERE `ch`.`user_id` IS NULL AND `p`.`user_id` IS NOT NULL;
UPDATE `IncomeCategories` `ch` JOIN `IncomeCategories` `p` ON `p`.`id` = `ch`.`parent_id`
SET `ch`.`user_id` = `p`.`user_id`
WHERE `ch`.`user_id` IS NULL AND `p`.`user_id` IS NOT NULL;

-- --- 4. Semilla de categorias comunes -------------------------------------
-- Un juego minimo de categorias comunes para que un usuario nuevo tenga de que
-- tirar. Solo se inserta la que no exista ya con ese nombre (de nadie ni comun),
-- asi que no duplica nada de lo que ya hay.

INSERT INTO `ExpenseCategories` (`name`, `parent_id`, `user_id`, `created_at`, `updated_at`)
SELECT 'Alimentación', NULL, NULL, NOW(), NOW() FROM DUAL
WHERE NOT EXISTS (SELECT 1 FROM `ExpenseCategories` `x` WHERE `x`.`name` = 'Alimentación');

INSERT INTO `ExpenseCategories` (`name`, `parent_id`, `user_id`, `created_at`, `updated_at`)
SELECT 'Hogar', NULL, NULL, NOW(), NOW() FROM DUAL
WHERE NOT EXISTS (SELECT 1 FROM `ExpenseCategories` `x` WHERE `x`.`name` = 'Hogar');

INSERT INTO `ExpenseCategories` (`name`, `parent_id`, `user_id`, `created_at`, `updated_at`)
SELECT 'Transporte', NULL, NULL, NOW(), NOW() FROM DUAL
WHERE NOT EXISTS (SELECT 1 FROM `ExpenseCategories` `x` WHERE `x`.`name` = 'Transporte');

INSERT INTO `ExpenseCategories` (`name`, `parent_id`, `user_id`, `created_at`, `updated_at`)
SELECT 'Salud', NULL, NULL, NOW(), NOW() FROM DUAL
WHERE NOT EXISTS (SELECT 1 FROM `ExpenseCategories` `x` WHERE `x`.`name` = 'Salud');

INSERT INTO `ExpenseCategories` (`name`, `parent_id`, `user_id`, `created_at`, `updated_at`)
SELECT 'Ocio', NULL, NULL, NOW(), NOW() FROM DUAL
WHERE NOT EXISTS (SELECT 1 FROM `ExpenseCategories` `x` WHERE `x`.`name` = 'Ocio');

INSERT INTO `ExpenseCategories` (`name`, `parent_id`, `user_id`, `created_at`, `updated_at`)
SELECT 'Otros gastos', NULL, NULL, NOW(), NOW() FROM DUAL
WHERE NOT EXISTS (SELECT 1 FROM `ExpenseCategories` `x` WHERE `x`.`name` = 'Otros gastos');

INSERT INTO `IncomeCategories` (`name`, `parent_id`, `user_id`, `created_at`, `updated_at`)
SELECT 'Nómina', NULL, NULL, NOW(), NOW() FROM DUAL
WHERE NOT EXISTS (SELECT 1 FROM `IncomeCategories` `x` WHERE `x`.`name` = 'Nómina');

INSERT INTO `IncomeCategories` (`name`, `parent_id`, `user_id`, `created_at`, `updated_at`)
SELECT 'Otros ingresos', NULL, NULL, NOW(), NOW() FROM DUAL
WHERE NOT EXISTS (SELECT 1 FROM `IncomeCategories` `x` WHERE `x`.`name` = 'Otros ingresos');
